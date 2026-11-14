using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ForeignKeyIssueSummary(string ChildTable, int IssueCount);
public sealed record DatabaseRelationshipAuditReport(string Database, DateTimeOffset CreatedAtUtc,
    int TotalForeignKeyIssues, IReadOnlyList<ForeignKeyIssueSummary> IssuesByChildTable, string ReportPath);

/// <summary>Read-only aggregate audit of SQLite foreign-key relationships; never emits row identifiers or values.</summary>
public sealed class DatabaseRelationshipAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseRelationshipAuditReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("關聯稽核報告已存在；為避免覆蓋已停止輸出。");

        var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_key_check;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                // Only the child-table name is retained. Rowid, parent and FK index are deliberately discarded.
                var table = reader.IsDBNull(0) ? "(unknown)" : reader.GetString(0);
                counts[table] = counts.TryGetValue(table, out var current) ? current + 1 : 1;
            }
        }

        var issues = counts.Select(x => new ForeignKeyIssueSummary(x.Key, x.Value)).ToArray();
        var result = new DatabaseRelationshipAuditReport(database, DateTimeOffset.UtcNow,
            issues.Sum(x => x.IssueCount), issues, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
