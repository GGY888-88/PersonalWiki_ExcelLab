using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed class DatabaseExportService
{
    private static readonly string[] ExportTables =
    {
        "schema_migrations", "import_batches", "entries", "entry_fields",
        "entry_notes", "entry_images", "entry_sources", "entry_tags"
    };

    public object ExportJson(string databasePath, string outputPath)
    {
        var source = Path.GetFullPath(databasePath);
        var destination = Path.GetFullPath(outputPath);
        if (!File.Exists(source)) throw new FileNotFoundException("找不到知識庫。", source);
        if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("匯出檔不可與來源資料庫相同。");
        var directory = Path.GetDirectoryName(destination);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        if (File.Exists(destination)) throw new IOException($"目的檔已存在，為避免覆蓋資料已停止：{destination}");

        var temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = source, Mode = SqliteOpenMode.ReadOnly, Pooling = false
            }.ToString()))
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();
                var version = ReadSchemaVersion(connection, transaction);
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("formatVersion", 1);
                    writer.WriteNumber("databaseSchemaVersion", version);
                    writer.WriteString("exportedAtUtc", DateTimeOffset.UtcNow);
                    writer.WriteString("sourceDatabase", Path.GetFileName(source));
                    writer.WriteStartObject("tables");
                    var counts = new Dictionary<string, long>();
                    foreach (var table in ExportTables)
                    {
                        writer.WriteStartArray(table);
                        using var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText = $"SELECT * FROM \"{table}\" ORDER BY 1;";
                        using var reader = command.ExecuteReader();
                        long count = 0;
                        while (reader.Read())
                        {
                            writer.WriteStartObject();
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                writer.WritePropertyName(reader.GetName(i));
                                if (reader.IsDBNull(i)) writer.WriteNullValue();
                                else WriteValue(writer, reader.GetValue(i));
                            }
                            writer.WriteEndObject();
                            count++;
                        }
                        writer.WriteEndArray();
                        counts[table] = count;
                    }
                    writer.WriteEndObject();
                    writer.WriteStartArray("excludedDerivedData");
                    writer.WriteStringValue("entries_fts (FTS5 搜尋索引，可由 db-index-rebuild 重建)");
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    writer.Flush();
                    transaction.Commit();
                    stream.Flush(true);
                    var total = counts.Values.Sum();
                    File.Move(temporary, destination, false);
                    return new { ExportFile = destination, FormatVersion = 1, DatabaseSchemaVersion = version, TableCounts = counts, TotalRows = total, FtsIndexIncluded = false, Exported = true };
                }
            }
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static int ReadSchemaVersion(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "PRAGMA user_version;";
        return Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void WriteValue(Utf8JsonWriter writer, object value)
    {
        switch (value)
        {
            case long number: writer.WriteNumberValue(number); break;
            case int number: writer.WriteNumberValue(number); break;
            case double number: writer.WriteNumberValue(number); break;
            case float number: writer.WriteNumberValue(number); break;
            case byte[] bytes: writer.WriteBase64StringValue(bytes); break;
            default: writer.WriteStringValue(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)); break;
        }
    }
}
