using Microsoft.Data.Sqlite;
using System.Text;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record TagAuditFinding(string Category, string Severity, string EntryId,
    string EntryTitle, string Detail);

public sealed record DatabaseTagAuditReport(string Database, DateTimeOffset CreatedAtUtc,
    int EntryCount, int TagCount, int BlankTagCount, int NormalizationMismatchCount,
    int DuplicateNormalizedTagCount, IReadOnlyList<TagAuditFinding> Findings, string ReportPath);

/// <summary>Creates a read-only audit of tag normalization without exporting tag text.</summary>
public sealed class DatabaseTagAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseTagAuditReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("標籤稽核報告已存在；為避免覆蓋已停止輸出。");

        var findings = new List<TagAuditFinding>();
        var tags = new List<(string EntryId, string Title, string Tag, string Normalized)>();
        var entryIds = new HashSet<string>(StringComparer.Ordinal);
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT t.entry_id, e.title, t.tag, t.normalized_tag
                FROM entry_tags t INNER JOIN entries e ON e.id = t.entry_id
                ORDER BY e.title COLLATE NOCASE, t.normalized_tag;
                """;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = (reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
                tags.Add(item);
                entryIds.Add(item.Item1);
                if (string.IsNullOrWhiteSpace(item.Item3))
                    findings.Add(new TagAuditFinding("BlankTag", "Warning", item.Item1, item.Item2,
                        "標籤名稱為空白或只有空白字元。"));
                else
                {
                    var expected = item.Item3.Trim().Normalize(NormalizationForm.FormKC).ToUpperInvariant();
                    if (!string.Equals(item.Item4, expected, StringComparison.Ordinal))
                        findings.Add(new TagAuditFinding("NormalizationMismatch", "Warning", item.Item1,
                            item.Item2, "標籤標準化欄位與依現行規則計算的結果不一致。"));
                    if (!string.Equals(item.Item3, item.Item3.Trim(), StringComparison.Ordinal))
                        findings.Add(new TagAuditFinding("TagWhitespace", "Review", item.Item1, item.Item2,
                            "標籤名稱含有前後空白。"));
                }
            }
        }

        var duplicateCount = 0;
        foreach (var group in tags.Where(x => !string.IsNullOrWhiteSpace(x.Tag))
                     .GroupBy(x => (x.EntryId, Normalized: x.Tag.Trim().Normalize(NormalizationForm.FormKC).ToUpperInvariant()))
                     .Where(g => g.Count() > 1))
        {
            var members = group.ToList();
            duplicateCount += members.Count;
            foreach (var item in members)
                findings.Add(new TagAuditFinding("DuplicateNormalizedTagCandidate", "Review", item.EntryId,
                    item.Title, $"同一條目有 {members.Count} 個依現行規則視為相同的標籤；僅供人工複核。"));
        }

        var result = new DatabaseTagAuditReport(database, DateTimeOffset.UtcNow, entryIds.Count,
            tags.Count, findings.Count(x => x.Category == "BlankTag"),
            findings.Count(x => x.Category == "NormalizationMismatch"), duplicateCount, findings, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
