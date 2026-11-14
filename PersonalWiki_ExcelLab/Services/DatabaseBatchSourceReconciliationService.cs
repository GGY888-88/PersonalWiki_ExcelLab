using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseBatchSourceReconciliationReport(
    string Database, DateTimeOffset CreatedAtUtc, int BatchCount, int BatchesWithSourceCount,
    int BatchesWithoutSourceCount, int ImportedCounterMismatchCount,
    int SourceRowsExceedImportedCounterCount, int UnbatchedSourceCount,
    int SourcesWithMissingBatchCount, string ReportPath);

/// <summary>Read-only aggregate reconciliation between import batch counters and source-trace rows.</summary>
public sealed class DatabaseBatchSourceReconciliationService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseBatchSourceReconciliationReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("批次來源核對報告已存在；為避免覆蓋已停止輸出。");

        int Scalar(SqliteConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            return Convert.ToInt32(command.ExecuteScalar());
        }

        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open();
        var batchCount = Scalar(connection, "SELECT COUNT(*) FROM import_batches;");
        var withSources = Scalar(connection, "SELECT COUNT(*) FROM import_batches b WHERE EXISTS (SELECT 1 FROM entry_sources s WHERE s.batch_id=b.id);");
        var withoutSources = batchCount - withSources;
        var mismatch = Scalar(connection, "SELECT COUNT(*) FROM import_batches b WHERE b.imported_row_count <> (SELECT COUNT(*) FROM entry_sources s WHERE s.batch_id=b.id);");
        var exceeds = Scalar(connection, "SELECT COUNT(*) FROM import_batches b WHERE (SELECT COUNT(*) FROM entry_sources s WHERE s.batch_id=b.id) > b.imported_row_count;");
        var unbatched = Scalar(connection, "SELECT COUNT(*) FROM entry_sources WHERE batch_id IS NULL;");
        var missingBatch = Scalar(connection, "SELECT COUNT(*) FROM entry_sources s LEFT JOIN import_batches b ON b.id=s.batch_id WHERE s.batch_id IS NOT NULL AND b.id IS NULL;");

        var result = new DatabaseBatchSourceReconciliationReport(database, DateTimeOffset.UtcNow,
            batchCount, withSources, withoutSources, mismatch, exceeds, unbatched, missingBatch, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
