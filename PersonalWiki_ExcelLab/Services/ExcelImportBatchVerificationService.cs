using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ExcelImportBatchVerificationResult(string BatchId, bool IsVerified, long BatchImportedRows, long ActualEntries, long ActualSources, long ActualFields, long MissingFtsEntries, long ForeignKeyViolations, IReadOnlyList<string> Issues);

public sealed class ExcelImportBatchVerificationService
{
    public ExcelImportBatchVerificationResult Verify(string databasePath, string batchId)
    {
        if (!File.Exists(databasePath)) throw new FileNotFoundException("找不到知識庫。", databasePath);
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path.GetFullPath(databasePath), Mode = SqliteOpenMode.ReadOnly }.ToString());
        connection.Open();
        string status;
        long expected;
        using (var batch = connection.CreateCommand())
        {
            batch.CommandText = "SELECT status, imported_row_count FROM import_batches WHERE id=$id;";
            batch.Parameters.AddWithValue("$id", batchId);
            using var reader = batch.ExecuteReader();
            if (!reader.Read()) throw new InvalidDataException("找不到指定的匯入批次。");
            status = reader.GetString(0); expected = reader.GetInt64(1);
        }
        long entries = Count(connection, "SELECT COUNT(*) FROM entries e JOIN entry_sources s ON s.entry_id=e.id WHERE s.batch_id=$id;", batchId);
        long sources = Count(connection, "SELECT COUNT(*) FROM entry_sources WHERE batch_id=$id;", batchId);
        long fields = Count(connection, "SELECT COUNT(*) FROM entry_fields WHERE source_batch_id=$id;", batchId);
        long missingFts = Count(connection, "SELECT COUNT(*) FROM entries e JOIN entry_sources s ON s.entry_id=e.id WHERE s.batch_id=$id AND NOT EXISTS (SELECT 1 FROM entries_fts f WHERE f.entry_id=e.id);", batchId);
        long fk = 0;
        using (var fkCommand = connection.CreateCommand())
        {
            fkCommand.CommandText = "PRAGMA foreign_key_check;";
            using var reader = fkCommand.ExecuteReader(); while (reader.Read()) fk++;
        }
        var issues = new List<string>();
        if (status != "Completed") issues.Add($"批次狀態不是 Completed：{status}");
        if (entries != expected) issues.Add($"批次預期匯入 {expected} 筆，實際來源關聯條目 {entries} 筆。");
        if (sources != expected) issues.Add($"來源追溯筆數 {sources} 與預期 {expected} 不一致。");
        if (missingFts > 0) issues.Add($"有 {missingFts} 筆條目缺少全文索引。");
        if (fk > 0) issues.Add($"資料庫存在 {fk} 筆外鍵違規。");
        return new ExcelImportBatchVerificationResult(batchId, issues.Count == 0, expected, entries, sources, fields, missingFts, fk, issues);
    }

    private static long Count(SqliteConnection c, string sql, string id)
    {
        using var command = c.CreateCommand(); command.CommandText = sql; command.Parameters.AddWithValue("$id", id); return (long)(command.ExecuteScalar() ?? 0L);
    }
}
