using System.Text;
using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed class SearchIndexer
{
    private const string TableName = "documents_fts";

    public (long IndexedRows, string Tokenizer) Rebuild(string sourceFolder, string databasePath)
    {
        if (!Directory.Exists(sourceFolder))
            throw new DirectoryNotFoundException($"找不到來源資料夾：{sourceFolder}");

        var fullDb = Path.GetFullPath(databasePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullDb)!);
        var temporaryDb = fullDb + ".building";
        DeleteDatabaseFiles(temporaryDb);

        try
        {
            var result = BuildTemporaryIndex(sourceFolder, temporaryDb);
            // 只有整個新索引成功建立並關閉後，才替換舊索引。
            File.Move(temporaryDb, fullDb, overwrite: true);
            DeleteDatabaseSidecars(temporaryDb);
            return result;
        }
        catch
        {
            DeleteDatabaseFiles(temporaryDb);
            throw;
        }
    }

    private (long IndexedRows, string Tokenizer) BuildTemporaryIndex(string sourceFolder, string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            // The temporary database is renamed after the connection closes.
            // Disable pooling so SQLite does not retain a Windows file handle.
            Pooling = false
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using (var setup = connection.CreateCommand())
        {
            setup.CommandText = "PRAGMA journal_mode=DELETE; PRAGMA synchronous=FULL;";
            setup.ExecuteNonQuery();
        }

        var tokenizer = TryCreateFtsTable(connection, "trigram") ? "trigram" : "unicode61";
        if (tokenizer == "unicode61" && !TryCreateFtsTable(connection, tokenizer))
            throw new InvalidOperationException("目前 SQLite 不支援 FTS5，無法建立全文索引。");

        using (var meta = connection.CreateCommand())
        {
            meta.CommandText = "CREATE TABLE index_info (key TEXT PRIMARY KEY, value TEXT NOT NULL); INSERT INTO index_info(key,value) VALUES ('tokenizer',$tokenizer),('created_at',$created);";
            meta.Parameters.AddWithValue("$tokenizer", tokenizer);
            meta.Parameters.AddWithValue("$created", DateTimeOffset.Now.ToString("O"));
            meta.ExecuteNonQuery();
        }

        long count = 0;
        using var transaction = connection.BeginTransaction();
        using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = $"INSERT INTO {TableName}(name,content,file_path,sheet_name,row_number) VALUES ($name,$content,$path,$sheet,$row);";
        var nameParameter = insert.Parameters.Add("$name", SqliteType.Text);
        var contentParameter = insert.Parameters.Add("$content", SqliteType.Text);
        var pathParameter = insert.Parameters.Add("$path", SqliteType.Text);
        var sheetParameter = insert.Parameters.Add("$sheet", SqliteType.Text);
        var rowParameter = insert.Parameters.Add("$row", SqliteType.Integer);
        insert.Prepare();

        var files = Directory.EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
            .Where(p => !Path.GetFileName(p).StartsWith("~$", StringComparison.Ordinal))
            .Where(p => Path.GetExtension(p).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                     || Path.GetExtension(p).Equals(".xlsm", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            foreach (var row in WorkbookReader.ReadRows(file))
            {
                nameParameter.Value = row.Name;
                contentParameter.Value = row.Content;
                pathParameter.Value = row.FilePath;
                sheetParameter.Value = row.SheetName;
                rowParameter.Value = (long)row.RowNumber;
                insert.ExecuteNonQuery();
                count++;
            }
        }

        transaction.Commit();
        return (count, tokenizer);
    }

    private static void DeleteDatabaseFiles(string path)
    {
        DeleteDatabaseSidecars(path);
        if (File.Exists(path))
            File.Delete(path);
    }

    private static void DeleteDatabaseSidecars(string path)
    {
        foreach (var suffix in new[] { "-wal", "-shm" })
        {
            var sidecar = path + suffix;
            if (File.Exists(sidecar))
                File.Delete(sidecar);
        }
    }

    public IReadOnlyList<IndexedRow> Search(string databasePath, string keyword, int limit = 50)
    {
        if (!File.Exists(databasePath))
            throw new FileNotFoundException("找不到索引資料庫，請先執行 index。", databasePath);
        if (string.IsNullOrWhiteSpace(keyword))
            return Array.Empty<IndexedRow>();
        if (limit is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(limit), "limit 必須介於 1 至 1000。");

        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = databasePath, Mode = SqliteOpenMode.ReadOnly }.ToString());
        connection.Open();
        var tokenizer = ReadTokenizer(connection);
        using var command = connection.CreateCommand();
        if (tokenizer == "trigram" && CountRunes(keyword) >= 3)
        {
            command.CommandText = $"SELECT name,content,file_path,sheet_name,row_number FROM {TableName} WHERE {TableName} MATCH $query LIMIT $limit;";
            command.Parameters.AddWithValue("$query", "\"" + keyword.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"");
        }
        else
        {
            command.CommandText = $"SELECT name,content,file_path,sheet_name,row_number FROM {TableName} WHERE instr(name,$query)>0 OR instr(content,$query)>0 LIMIT $limit;";
            command.Parameters.AddWithValue("$query", keyword);
        }
        command.Parameters.AddWithValue("$limit", limit);

        var results = new List<IndexedRow>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var rowNumber = uint.Parse(reader.GetString(4), System.Globalization.CultureInfo.InvariantCulture);
            results.Add(new IndexedRow(
                reader.GetString(2), reader.GetString(3), rowNumber,
                reader.GetString(0), reader.GetString(1)));
        }
        return results;
    }

    private static int CountRunes(string value)
    {
        var count = 0;
        foreach (var _ in value.EnumerateRunes())
            count++;
        return count;
    }

    private static bool TryCreateFtsTable(SqliteConnection connection, string tokenizer)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"CREATE VIRTUAL TABLE {TableName} USING fts5(name,content,file_path UNINDEXED,sheet_name UNINDEXED,row_number UNINDEXED,tokenize='{tokenizer}');";
            command.ExecuteNonQuery();
            return true;
        }
        catch (SqliteException)
        {
            using var cleanup = connection.CreateCommand();
            cleanup.CommandText = $"DROP TABLE IF EXISTS {TableName};";
            cleanup.ExecuteNonQuery();
            return false;
        }
    }

    private static string ReadTokenizer(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM index_info WHERE key='tokenizer';";
        return (string?)command.ExecuteScalar() ?? "unicode61";
    }
}
