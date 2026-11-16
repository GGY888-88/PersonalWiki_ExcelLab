using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed class DatabaseImportService
{
    private static readonly string[] ImportOrder =
    {
        "schema_migrations", "import_batches", "entries", "entry_fields",
        "entry_notes", "entry_images", "entry_sources", "entry_tags"
    };

    public object ImportIntoEmptyDatabase(string databasePath, string importPath)
    {
        var preview = new DatabaseImportPreviewService().Preview(databasePath, importPath);
        if (!preview.IsValid || !preview.CanImport)
            throw new InvalidOperationException("匯入預檢未通過；請先執行 db-import-preview 並排除錯誤或衝突。");

        using var importStream = new FileStream(Path.GetFullPath(importPath), FileMode.Open, FileAccess.Read, FileShare.Read);
        using var document = JsonDocument.Parse(importStream, new JsonDocumentOptions { MaxDepth = 64 });
        var tables = document.RootElement.GetProperty("tables");
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path.GetFullPath(databasePath), Mode = SqliteOpenMode.ReadWrite, Pooling = false }.ToString());
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var currentEntries = Scalar(connection, transaction, "SELECT COUNT(*) FROM entries;");
        var currentBatches = Scalar(connection, transaction, "SELECT COUNT(*) FROM import_batches;");
        if (currentEntries != 0 || currentBatches != 0)
            throw new InvalidOperationException("正式匯入目前只允許寫入沒有條目及匯入批次的空白知識庫；原資料未修改。");

        var counts = new Dictionary<string, int>();
        foreach (var table in ImportOrder)
        {
            var rows = tables.GetProperty(table);
            var allowedColumns = ReadColumns(connection, transaction, table);
            var count = 0;
            foreach (var row in rows.EnumerateArray())
            {
                var properties = row.EnumerateObject().ToArray();
                if (properties.Any(p => !allowedColumns.Contains(p.Name, StringComparer.Ordinal)))
                    throw new InvalidDataException($"資料表 {table} 含有目標 Schema 不支援的欄位。");
                if (properties.Length == 0) throw new InvalidDataException($"資料表 {table} 出現空白資料列。");
                var names = properties.Select(p => $"\"{p.Name}\"").ToArray();
                var parameters = properties.Select((p, i) => $"$p{i}").ToArray();
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = $"{(table == "schema_migrations" ? "INSERT OR IGNORE" : "INSERT INTO")} \"{table}\" ({string.Join(",", names)}) VALUES ({string.Join(",", parameters)});";
                for (var i = 0; i < properties.Length; i++)
                    command.Parameters.AddWithValue(parameters[i], ToDbValue(properties[i].Value));
                command.ExecuteNonQuery();
                count++;
            }
            counts[table] = count;
        }

        using (var rebuild = connection.CreateCommand())
        {
            rebuild.Transaction = transaction;
            rebuild.CommandText = "INSERT INTO entries_fts(entries_fts) VALUES('delete-all'); INSERT INTO entries_fts(entry_id,title,summary) SELECT id,title,COALESCE(summary,'') FROM entries; INSERT INTO entries_fts(entries_fts) VALUES('integrity-check');";
            rebuild.ExecuteNonQuery();
        }
        using (var foreignKeys = connection.CreateCommand())
        {
            foreignKeys.Transaction = transaction;
            foreignKeys.CommandText = "PRAGMA foreign_key_check;";
            using var reader = foreignKeys.ExecuteReader();
            if (reader.Read()) throw new InvalidDataException("匯入後外鍵檢查失敗；交易已取消。");
        }
        transaction.Commit();
        return new { Imported = true, TableCounts = counts, TotalRows = counts.Values.Sum(), DatabaseModified = true, Message = "JSON 資料已匯入；圖片檔案本身未複製。" };
    }

    private static HashSet<string> ReadColumns(SqliteConnection connection, SqliteTransaction transaction, string table)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"PRAGMA table_info(\"{table}\");";
        using var reader = command.ExecuteReader();
        var columns = new HashSet<string>(StringComparer.Ordinal);
        while (reader.Read()) columns.Add(reader.GetString(1));
        return columns;
    }

    private static long Scalar(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using var command = connection.CreateCommand(); command.Transaction = transaction; command.CommandText = sql;
        return Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static object ToDbValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null => DBNull.Value,
        JsonValueKind.String => value.GetString()!,
        JsonValueKind.Number when value.TryGetInt64(out var integer) => integer,
        JsonValueKind.Number => value.GetDouble(),
        JsonValueKind.True => 1L,
        JsonValueKind.False => 0L,
        _ => throw new InvalidDataException("匯入欄位含有不支援的 JSON 型別。")
    };
}
