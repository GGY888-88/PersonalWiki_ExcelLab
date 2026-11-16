using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed class DatabaseImportVerificationService
{
    private static readonly string[] Tables =
    {
        "schema_migrations", "import_batches", "entries", "entry_fields",
        "entry_notes", "entry_images", "entry_sources", "entry_tags"
    };

    public dynamic Verify(string databasePath, string importPath)
    {
        var database = Path.GetFullPath(databasePath);
        var importFile = Path.GetFullPath(importPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (!File.Exists(importFile)) throw new FileNotFoundException("找不到 JSON 匯入檔。", importFile);
        using var importStream = new FileStream(importFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var document = JsonDocument.Parse(importStream, new JsonDocumentOptions { MaxDepth = 64 });
        var root = document.RootElement;
        if (!root.TryGetProperty("tables", out var sourceTables) || sourceTables.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("JSON 缺少 tables 物件。");

        var expected = new Dictionary<string, int>();
        foreach (var table in Tables)
        {
            if (!sourceTables.TryGetProperty(table, out var rows) || rows.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException($"JSON 缺少資料表或格式錯誤：{table}");
            expected[table] = rows.GetArrayLength();
        }

        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open();
        var actual = new Dictionary<string, long>();
        foreach (var table in Tables)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM \"{table}\";";
            actual[table] = Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
        }
        var differences = Tables.Where(t => actual[t] != expected[t])
            .Select(t => new { Table = t, Expected = expected[t], Actual = actual[t] }).ToArray();
        long foreignKeyViolations;
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA foreign_key_check;";
            using var reader = command.ExecuteReader();
            foreignKeyViolations = 0;
            while (reader.Read()) foreignKeyViolations++;
        }
        long entryCount = actual["entries"], indexedCount;
        using (var command = connection.CreateCommand()) { command.CommandText = "SELECT COUNT(*) FROM entries_fts;"; indexedCount = Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture); }
        var indexConsistent = entryCount == indexedCount;
        return new { Database = database, IsVerified = differences.Length == 0 && foreignKeyViolations == 0 && indexConsistent, ExpectedTableCounts = expected, ActualTableCounts = actual, CountDifferences = differences, ForeignKeyViolations = foreignKeyViolations, EntryCount = entryCount, IndexedCount = indexedCount, SearchIndexConsistent = indexConsistent, ReadOnly = true };
    }
}
