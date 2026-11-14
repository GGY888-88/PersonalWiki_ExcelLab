using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record FieldAuditFinding(string Category, string Severity, string EntryId,
    string EntryTitle, string FieldKey, string Detail);

public sealed record DatabaseFieldAuditReport(string Database, DateTimeOffset CreatedAtUtc,
    int EntryCount, int FieldCount, int BlankKeyCount, int BlankLabelCount,
    int BlankValueCount, int DuplicateLabelCandidateCount,
    IReadOnlyList<FieldAuditFinding> Findings, string ReportPath);

/// <summary>Creates a read-only audit of entry fields without exporting field values.</summary>
public sealed class DatabaseFieldAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseFieldAuditReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("欄位稽核報告已存在；為避免覆蓋已停止輸出。");

        var findings = new List<FieldAuditFinding>();
        var fields = new List<(string EntryId, string Title, string Key, string Label, string? Value)>();
        var entryIds = new HashSet<string>(StringComparer.Ordinal);
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT f.entry_id, e.title, f.field_key, f.field_label, f.field_value
                FROM entry_fields f INNER JOIN entries e ON e.id = f.entry_id
                ORDER BY e.title COLLATE NOCASE, f.sort_order, f.id;
                """;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = (reader.GetString(0), reader.GetString(1), reader.GetString(2),
                    reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4));
                fields.Add(item);
                entryIds.Add(item.Item1);
                if (string.IsNullOrWhiteSpace(item.Item3))
                    findings.Add(new FieldAuditFinding("BlankFieldKey", "Warning", item.Item1,
                        item.Item2, item.Item3, "欄位識別鍵為空白。"));
                if (string.IsNullOrWhiteSpace(item.Item4))
                    findings.Add(new FieldAuditFinding("BlankFieldLabel", "Warning", item.Item1,
                        item.Item2, item.Item3, "欄位顯示名稱為空白。"));
                if (string.IsNullOrWhiteSpace(item.Item5))
                    findings.Add(new FieldAuditFinding("BlankFieldValue", "Review", item.Item1,
                        item.Item2, item.Item3, "欄位值為空白或只有空白字元。"));
            }
        }

        var duplicateLabelCandidates = 0;
        foreach (var group in fields.Where(x => !string.IsNullOrWhiteSpace(x.Label))
                     .GroupBy(x => (x.EntryId, Label: x.Label.Trim()),
                         EqualityComparer<(string EntryId, string Label)>.Create(
                             (a, b) => a.EntryId == b.EntryId && string.Equals(a.Label, b.Label, StringComparison.OrdinalIgnoreCase),
                             x => HashCode.Combine(x.EntryId, StringComparer.OrdinalIgnoreCase.GetHashCode(x.Label))))
                     .Where(g => g.Count() > 1))
        {
            var members = group.ToList();
            duplicateLabelCandidates += members.Count;
            foreach (var field in members)
                findings.Add(new FieldAuditFinding("DuplicateFieldLabelCandidate", "Review",
                    field.EntryId, field.Title, field.Key,
                    $"同一條目有 {members.Count} 個顯示名稱相同的欄位；僅供人工複核。"));
        }

        var result = new DatabaseFieldAuditReport(database, DateTimeOffset.UtcNow, entryIds.Count,
            fields.Count, findings.Count(x => x.Category == "BlankFieldKey"),
            findings.Count(x => x.Category == "BlankFieldLabel"),
            findings.Count(x => x.Category == "BlankFieldValue"), duplicateLabelCandidates, findings, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
