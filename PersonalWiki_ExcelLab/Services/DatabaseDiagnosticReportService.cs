using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseDiagnosticReport(
    string Format,
    DateTimeOffset CreatedAtUtc,
    string Database,
    long DatabaseBytes,
    DatabaseHealthResult Health,
    SearchIndexCheckResult SearchIndex,
    long PageSizeBytes,
    long PageCount,
    long FreePageCount,
    long UsedPageCount,
    int IndexCount,
    IReadOnlyList<DatabaseTableInventory> Tables,
    IReadOnlyList<string> Warnings,
    bool IsHealthy,
    string ReportPath);

/// <summary>Creates a consolidated, read-only database diagnostic report without exposing row contents.</summary>
public sealed class DatabaseDiagnosticReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseDiagnosticReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("診斷報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("診斷報告已存在；為避免覆蓋已停止輸出。");

        DatabaseHealthResult health;
        SearchIndexCheckResult index;
        IReadOnlyList<DatabaseTableInventory> tables;
        long pageSize;
        long pageCount;
        long freePageCount;
        int indexCount;
        var warnings = new List<string>();

        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using var transaction = connection.BeginTransaction(deferred: true);
            // Establish one read snapshot before collecting any diagnostic values.
            _ = ScalarLong(connection, transaction, "SELECT COUNT(*) FROM sqlite_master;");

            var quickCheck = new List<string>();
            try
            {
                using var command = CreateCommand(connection, transaction, "PRAGMA quick_check;");
                using var reader = command.ExecuteReader();
                while (reader.Read()) quickCheck.Add(reader.GetString(0));
            }
            catch (SqliteException ex)
            {
                quickCheck.Add("error: " + ex.Message);
                warnings.Add("SQLite quick_check 執行失敗：" + ex.Message);
            }

            var foreignKeyIssues = new List<object>();
            try
            {
                using var command = CreateCommand(connection, transaction, "PRAGMA foreign_key_check;");
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    foreignKeyIssues.Add(new
                    {
                        Table = reader.GetString(0),
                        RowId = reader.IsDBNull(1) ? (long?)null : reader.GetInt64(1),
                        ParentTable = reader.GetString(2),
                        ForeignKeyIndex = reader.GetInt64(3)
                    });
            }
            catch (SqliteException ex)
            {
                warnings.Add("SQLite foreign_key_check 執行失敗：" + ex.Message);
            }

            var schemaVersion = checked((int)ScalarLong(connection, transaction, "PRAGMA user_version;"));
            var quickCheckPassed = quickCheck.Count == 1 && quickCheck[0].Equals("ok", StringComparison.OrdinalIgnoreCase);
            health = new DatabaseHealthResult(database, schemaVersion, quickCheck, quickCheckPassed,
                foreignKeyIssues.Count, foreignKeyIssues, quickCheckPassed && foreignKeyIssues.Count == 0 && warnings.Count == 0, true);

            try
            {
                var entryCount = ScalarLong(connection, transaction, "SELECT COUNT(*) FROM entries;");
                var indexedCount = ScalarLong(connection, transaction, "SELECT COUNT(*) FROM entries_fts;");
                var missingCount = ScalarLong(connection, transaction,
                    "SELECT COUNT(*) FROM entries e WHERE NOT EXISTS (SELECT 1 FROM entries_fts f WHERE f.entry_id=e.id);");
                var orphanCount = ScalarLong(connection, transaction,
                    "SELECT COUNT(*) FROM entries_fts f WHERE NOT EXISTS (SELECT 1 FROM entries e WHERE e.id=f.entry_id);");
                var indexConsistent = missingCount == 0 && orphanCount == 0 && entryCount == indexedCount;
                index = new SearchIndexCheckResult(database, entryCount, indexedCount, missingCount, orphanCount, indexConsistent, true);
                if (!indexConsistent) warnings.Add("全文索引筆數或關聯與條目資料不一致。");
            }
            catch (SqliteException ex)
            {
                index = new SearchIndexCheckResult(database, -1, -1, -1, -1, false, true);
                warnings.Add("無法完成全文索引檢查：" + ex.Message);
            }

            pageSize = ScalarLong(connection, transaction, "PRAGMA page_size;");
            pageCount = ScalarLong(connection, transaction, "PRAGMA page_count;");
            freePageCount = ScalarLong(connection, transaction, "PRAGMA freelist_count;");
            indexCount = checked((int)ScalarLong(connection, transaction,
                "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name NOT LIKE 'sqlite_%';"));

            var tableNames = new List<string>();
            using (var command = CreateCommand(connection, transaction,
                "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;"))
            using (var reader = command.ExecuteReader())
                while (reader.Read()) tableNames.Add(reader.GetString(0));

            var tableCounts = new List<DatabaseTableInventory>(tableNames.Count);
            foreach (var name in tableNames)
            {
                var quotedName = "\"" + name.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
                try
                {
                    var rows = ScalarLong(connection, transaction, $"SELECT COUNT(*) FROM {quotedName};");
                    tableCounts.Add(new DatabaseTableInventory(name, rows));
                }
                catch (SqliteException ex)
                {
                    tableCounts.Add(new DatabaseTableInventory(name, -1));
                    warnings.Add($"無法讀取資料表 {name} 的筆數：{ex.Message}");
                }
            }
            tables = tableCounts;
            transaction.Commit();
        }

        var fileInfo = new FileInfo(database);
        var healthy = health.IsHealthy && index.IsConsistent && warnings.Count == 0 && tables.All(x => x.Rows >= 0);
        var result = new DatabaseDiagnosticReport(
            "personalwiki-database-diagnostic-v1", DateTimeOffset.UtcNow, database,
            fileInfo.Length, health, index, pageSize, pageCount, freePageCount,
            Math.Max(0, pageCount - freePageCount), indexCount, tables, warnings, healthy, output);

        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        var temporary = output + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, result, JsonOptions);
                stream.Flush(true);
            }
            // Atomic no-clobber commit: a concurrently created destination is never overwritten.
            File.Move(temporary, output, overwrite: false);
        }
        catch
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); } catch { }
            throw;
        }
        return result;
    }

    private static SqliteCommand CreateCommand(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        return command;
    }

    private static long ScalarLong(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using var command = CreateCommand(connection, transaction, sql);
        return Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }
}
