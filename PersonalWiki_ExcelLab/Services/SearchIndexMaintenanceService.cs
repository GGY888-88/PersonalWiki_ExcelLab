using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record SearchIndexCheckResult(string Database, long EntryCount, long IndexedCount,
    long MissingCount, long OrphanCount, bool IsConsistent, bool ReadOnly);

public sealed record SearchIndexRebuildResult(string Database, long EntryCount, long IndexedCount,
    bool Rebuilt, string Message);

public sealed class SearchIndexMaintenanceService
{
    public SearchIndexCheckResult Check(string databasePath)
    {
        var fullPath = Path.GetFullPath(databasePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("找不到知識庫。", fullPath);
        using var connection = Open(fullPath, SqliteOpenMode.ReadOnly);
        var entryCount = ScalarLong(connection, "SELECT COUNT(*) FROM entries;");
        var indexedCount = ScalarLong(connection, "SELECT COUNT(*) FROM entries_fts;");
        var missing = ScalarLong(connection, "SELECT COUNT(*) FROM entries e WHERE NOT EXISTS (SELECT 1 FROM entries_fts f WHERE f.entry_id=e.id);");
        var orphan = ScalarLong(connection, "SELECT COUNT(*) FROM entries_fts f WHERE NOT EXISTS (SELECT 1 FROM entries e WHERE e.id=f.entry_id);");
        return new SearchIndexCheckResult(fullPath, entryCount, indexedCount, missing, orphan,
            missing == 0 && orphan == 0 && entryCount == indexedCount, true);
    }

    public SearchIndexRebuildResult Rebuild(string databasePath)
    {
        var fullPath = Path.GetFullPath(databasePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("找不到知識庫。", fullPath);
        using var connection = Open(fullPath, SqliteOpenMode.ReadWrite);
        using var transaction = connection.BeginTransaction();
        var entryCount = ScalarLong(connection, "SELECT COUNT(*) FROM entries;", transaction);
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "INSERT INTO entries_fts(entries_fts) VALUES('delete-all');";
            command.ExecuteNonQuery();
        }
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "INSERT INTO entries_fts(entry_id,title,summary) SELECT id,title,COALESCE(summary,'') FROM entries;";
            command.ExecuteNonQuery();
        }
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "INSERT INTO entries_fts(entries_fts) VALUES('integrity-check');";
            command.ExecuteNonQuery();
        }
        transaction.Commit();
        var indexedCount = ScalarLong(connection, "SELECT COUNT(*) FROM entries_fts;");
        return new SearchIndexRebuildResult(fullPath, entryCount, indexedCount,
            entryCount == indexedCount, entryCount == indexedCount ? "全文索引重建完成。" : "索引筆數與條目筆數不一致。請執行 db-index-check。");
    }

    private static SqliteConnection Open(string path, SqliteOpenMode mode)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path, Mode = mode, Pooling = false
        }.ToString());
        connection.Open();
        return connection;
    }

    private static long ScalarLong(SqliteConnection connection, string sql, SqliteTransaction? transaction = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        return Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }
}
