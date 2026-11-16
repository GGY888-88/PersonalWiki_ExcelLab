using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseSourceTraceAuditReport(
    string Database,
    DateTimeOffset CreatedAtUtc,
    int TotalSourceRecords,
    int EntriesWithSourceRecords,
    int EntriesWithoutSourceRecords,
    int SourcesWithMissingEntry,
    int SourcesWithBlankSourceFile,
    int SourcesWithBlankSourceSheet,
    int SourcesWithInvalidRow,
    int SourcesWithBlankImportedAt,
    int SourcesWithMissingBatch,
    int SourcesWithoutBatch,
    string ReportPath);

/// <summary>Audits source-trace completeness using aggregate counts only; database access is read-only.</summary>
public sealed class DatabaseSourceTraceAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseSourceTraceAuditReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("來源追溯稽核報告已存在；為避免覆蓋已停止輸出。");

        int Scalar(SqliteConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            return Convert.ToInt32(command.ExecuteScalar());
        }

        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open();
        var totalSources = Scalar(connection, "SELECT COUNT(*) FROM entry_sources;");
        var entriesWithSources = Scalar(connection, "SELECT COUNT(DISTINCT entry_id) FROM entry_sources;");
        var totalEntries = Scalar(connection, "SELECT COUNT(*) FROM entries;");
        var missingEntry = Scalar(connection, "SELECT COUNT(*) FROM entry_sources s LEFT JOIN entries e ON e.id=s.entry_id WHERE e.id IS NULL;");
        var blankFile = Scalar(connection, "SELECT COUNT(*) FROM entry_sources WHERE trim(source_file)='';");
        var blankSheet = Scalar(connection, "SELECT COUNT(*) FROM entry_sources WHERE trim(source_sheet)='';");
        var invalidRow = Scalar(connection, "SELECT COUNT(*) FROM entry_sources WHERE source_row<=0;");
        var blankImportedAt = Scalar(connection, "SELECT COUNT(*) FROM entry_sources WHERE trim(imported_at)='';");
        var missingBatch = Scalar(connection, "SELECT COUNT(*) FROM entry_sources s LEFT JOIN import_batches b ON b.id=s.batch_id WHERE s.batch_id IS NOT NULL AND b.id IS NULL;");
        var withoutBatch = Scalar(connection, "SELECT COUNT(*) FROM entry_sources WHERE batch_id IS NULL;");

        var result = new DatabaseSourceTraceAuditReport(database, DateTimeOffset.UtcNow, totalSources,
            entriesWithSources, Math.Max(0, totalEntries - entriesWithSources), missingEntry,
            blankFile, blankSheet, invalidRow, blankImportedAt, missingBatch, withoutBatch, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
