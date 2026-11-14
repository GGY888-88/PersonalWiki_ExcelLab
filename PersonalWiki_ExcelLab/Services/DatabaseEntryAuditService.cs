using Microsoft.Data.Sqlite;
using System.Text.Json;
using System.Text;

namespace PersonalWiki.ExcelLab.Services;

public sealed record EntryAuditFinding(string Category, string Severity, string EntryId, string Title, string Detail);
public sealed record EntryAuditResult(string Database, DateTimeOffset CreatedAt, int EntryCount,
    int NormalizationMismatchCount, int CrossTypeTitleCandidateCount, int MissingImageCount,
    IReadOnlyList<EntryAuditFinding> Findings, string ReportPath);

/// <summary>Read-only audit for suspicious entry title normalization, cross-type title collisions and missing image files.</summary>
public sealed class DatabaseEntryAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public EntryAuditResult Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase)) throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("資料品質報告已存在；為避免覆蓋已停止輸出。");

        var findings = new List<EntryAuditFinding>();
        var entries = new List<(string Id, string Title, string Type, string Normalized)>();
        var missingImages = 0;
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT id,title,entry_type,normalized_title FROM entries ORDER BY title COLLATE NOCASE,id;";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var item = (reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
                    entries.Add(item);
                    var expected = Normalize(item.Item2);
                    if (!string.Equals(expected, item.Item4, StringComparison.Ordinal))
                        findings.Add(new EntryAuditFinding("TitleNormalizationMismatch", "Warning", item.Item1, item.Item2,
                            $"資料庫 normalized_title 與標題標準化結果不同；目前='{item.Item4}'，預期='{expected}'。"));
                }
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT id,entry_id,file_path FROM entry_images ORDER BY entry_id,sort_order,id;";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetString(0);
                    var entryId = reader.GetString(1);
                    var path = reader.GetString(2);
                    if (!File.Exists(path))
                    {
                        missingImages++;
                        var entry = entries.FirstOrDefault(x => x.Id == entryId);
                        findings.Add(new EntryAuditFinding("MissingImageFile", "Warning", entryId,
                            entry.Title ?? string.Empty, $"圖片關聯 {id} 指向不存在的檔案：{path}"));
                    }
                }
            }
        }

        var candidates = entries.GroupBy(x => Normalize(x.Title), StringComparer.Ordinal)
            .Where(g => g.Select(x => x.Type).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1);
        var candidateCount = 0;
        foreach (var group in candidates)
        {
            var members = group.ToList();
            candidateCount += members.Count;
            foreach (var item in members)
                findings.Add(new EntryAuditFinding("SameTitleDifferentType", "Review", item.Id, item.Title,
                    "其他條目具有相同標準化標題但類型不同；這是人工檢查候選，不代表重複資料。"));
        }

        var result = new EntryAuditResult(database, DateTimeOffset.UtcNow, entries.Count,
            findings.Count(x => x.Category == "TitleNormalizationMismatch"), candidateCount,
            missingImages, findings, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }

    private static string Normalize(string value) => value.Trim().Normalize(NormalizationForm.FormKC).ToUpperInvariant();
}
