using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed class DatabaseImportPreviewService
{
    private static readonly string[] RequiredTables =
    {
        "schema_migrations", "import_batches", "entries", "entry_fields",
        "entry_notes", "entry_images", "entry_sources", "entry_tags"
    };

    public dynamic Preview(string databasePath, string importPath)
    {
        var database = Path.GetFullPath(databasePath);
        var importFile = Path.GetFullPath(importPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到目標知識庫。", database);
        if (!File.Exists(importFile)) throw new FileNotFoundException("找不到 JSON 匯入檔。", importFile);

        using var importStream = new FileStream(importFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var document = JsonDocument.Parse(importStream, new JsonDocumentOptions { MaxDepth = 64 });
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object) throw new InvalidDataException("JSON 根節點必須是物件。");
        if (!root.TryGetProperty("formatVersion", out var format) || !format.TryGetInt32(out var formatVersion) || formatVersion != 1)
            throw new InvalidDataException("不支援的 JSON 格式版本；目前只接受 formatVersion 1。");
        if (!root.TryGetProperty("databaseSchemaVersion", out var schema) || !schema.TryGetInt32(out var sourceSchema))
            throw new InvalidDataException("缺少有效的 databaseSchemaVersion。");
        if (!root.TryGetProperty("tables", out var tables) || tables.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("tables 必須是 JSON 物件。");

        var errors = new List<string>();
        var warnings = new List<string>();
        var sourceCounts = new Dictionary<string, int>();
        foreach (var table in RequiredTables)
        {
            if (!tables.TryGetProperty(table, out var rows) || rows.ValueKind != JsonValueKind.Array)
                errors.Add($"缺少資料表或格式錯誤：{table}");
            else sourceCounts[table] = rows.GetArrayLength();
        }
        if (errors.Count > 0) return new { IsValid = false, CanImport = false, Errors = errors, Warnings = warnings, SourceTableCounts = sourceCounts };

        var entryIds = new HashSet<string>(StringComparer.Ordinal);
        var entryKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in tables.GetProperty("entries").EnumerateArray())
        {
            var id = GetString(row, "id");
            var title = GetString(row, "title");
            var normalized = GetString(row, "normalized_title");
            var type = GetString(row, "entry_type");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(normalized) || string.IsNullOrWhiteSpace(type))
            { errors.Add("entries 存在缺少 id/title/normalized_title/entry_type 的資料。"); continue; }
            if (!entryIds.Add(id)) errors.Add($"匯入檔 entries 出現重複 id：{id}");
            if (!entryKeys.Add(normalized + "\u001f" + type)) errors.Add($"匯入檔存在重複標題與類型：{title} ({type})");
        }

        foreach (var tableName in new[] { "entry_fields", "entry_notes", "entry_images", "entry_sources", "entry_tags" })
        {
            var rowNumber = 0;
            foreach (var row in tables.GetProperty(tableName).EnumerateArray())
            {
                rowNumber++;
                var entryId = GetString(row, "entry_id");
                if (string.IsNullOrWhiteSpace(entryId) || !entryIds.Contains(entryId))
                    errors.Add($"{tableName} 第 {rowNumber} 筆參照不存在的匯入 entry_id。");
            }
        }

        var targetSchema = 0;
        var existingIds = new HashSet<string>(StringComparer.Ordinal);
        var existingKeys = new HashSet<string>(StringComparer.Ordinal);
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using (var command = connection.CreateCommand()) { command.CommandText = "PRAGMA user_version;"; targetSchema = Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture); }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT id, normalized_title, entry_type FROM entries;";
                using var reader = command.ExecuteReader();
                while (reader.Read()) { existingIds.Add(reader.GetString(0)); existingKeys.Add(reader.GetString(1) + "\u001f" + reader.GetString(2)); }
            }
        }
        if (sourceSchema != targetSchema) errors.Add($"資料庫 Schema 不相容：匯入檔 {sourceSchema}，目標資料庫 {targetSchema}。");
        var idConflicts = entryIds.Count(existingIds.Contains);
        var keyConflicts = entryKeys.Count(existingKeys.Contains);
        if (idConflicts > 0) warnings.Add($"有 {idConflicts} 個條目 ID 已存在於目標資料庫。");
        if (keyConflicts > 0) warnings.Add($"有 {keyConflicts} 組標題與類型已存在於目標資料庫。");
        if (sourceCounts["entry_images"] > 0) warnings.Add("圖片檔案本身不會匯入；僅處理圖片關聯與路徑 metadata。");
        if (errors.Count == 0 && (idConflicts > 0 || keyConflicts > 0)) warnings.Add("目前版本只提供匯入預覽，尚未執行寫入或自動合併。");

        return new { IsValid = errors.Count == 0, CanImport = errors.Count == 0 && idConflicts == 0 && keyConflicts == 0, SourceSchemaVersion = sourceSchema, TargetSchemaVersion = targetSchema, SourceTableCounts = sourceCounts, SourceEntryCount = entryIds.Count, ExistingIdConflicts = idConflicts, ExistingTitleTypeConflicts = keyConflicts, Errors = errors, Warnings = warnings, DatabaseModified = false };
    }

    private static string? GetString(JsonElement row, string property) =>
        row.ValueKind == JsonValueKind.Object && row.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}
