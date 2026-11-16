using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed class DatabaseExportValidationService
{
    private static readonly string[] RequiredTables =
    {
        "schema_migrations", "import_batches", "entries", "entry_fields",
        "entry_notes", "entry_images", "entry_sources", "entry_tags"
    };

    public dynamic Validate(string exportPath)
    {
        var fullPath = Path.GetFullPath(exportPath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("找不到 JSON 匯出檔。", fullPath);
        using var exportStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var document = JsonDocument.Parse(exportStream, new JsonDocumentOptions { MaxDepth = 64 });
        var root = document.RootElement;
        var errors = new List<string>();
        var warnings = new List<string>();
        if (root.ValueKind != JsonValueKind.Object) throw new InvalidDataException("JSON 根節點必須是物件。");
        var formatVersion = 0;
        var schemaVersion = 0;
        if (!root.TryGetProperty("formatVersion", out var format) || !format.TryGetInt32(out formatVersion) || formatVersion != 1)
            errors.Add("formatVersion 必須為 1。");
        if (!root.TryGetProperty("databaseSchemaVersion", out var schema) || !schema.TryGetInt32(out schemaVersion) || schemaVersion < 1)
            errors.Add("databaseSchemaVersion 缺少或無效。");
        if (!root.TryGetProperty("tables", out var tables) || tables.ValueKind != JsonValueKind.Object)
            errors.Add("tables 必須是 JSON 物件。");

        var counts = new Dictionary<string, int>();
        if (tables.ValueKind == JsonValueKind.Object)
        {
            foreach (var name in RequiredTables)
            {
                if (!tables.TryGetProperty(name, out var rows)) { errors.Add($"缺少資料表：{name}"); continue; }
                if (rows.ValueKind != JsonValueKind.Array) { errors.Add($"資料表 {name} 必須是陣列。"); continue; }
                counts[name] = rows.GetArrayLength();
                var rowNumber = 0;
                foreach (var row in rows.EnumerateArray())
                {
                    rowNumber++;
                    if (row.ValueKind != JsonValueKind.Object) { errors.Add($"{name} 第 {rowNumber} 筆不是物件。"); continue; }
                    if (name == "entries" && (!HasNonEmptyString(row, "id") || !HasNonEmptyString(row, "title") || !HasNonEmptyString(row, "normalized_title") || !HasNonEmptyString(row, "entry_type")))
                        errors.Add($"entries 第 {rowNumber} 筆缺少必要識別或標題欄位。");
                    if ((name is "entry_notes" or "entry_images") && (!HasNonEmptyString(row, "id") || !HasNonEmptyString(row, "entry_id")))
                        errors.Add($"{name} 第 {rowNumber} 筆缺少 id 或 entry_id。");
                    if (name == "entry_fields" && (!HasNonEmptyString(row, "entry_id") || !HasNonEmptyString(row, "field_key")))
                        errors.Add($"entry_fields 第 {rowNumber} 筆缺少 entry_id 或 field_key。");
                    if (name == "entry_tags" && (!HasNonEmptyString(row, "entry_id") || !HasNonEmptyString(row, "tag") || !HasNonEmptyString(row, "normalized_tag")))
                        errors.Add($"entry_tags 第 {rowNumber} 筆缺少關聯或標籤欄位。");
                }
            }
            foreach (var property in tables.EnumerateObject())
                if (!RequiredTables.Contains(property.Name, StringComparer.Ordinal)) warnings.Add($"發現未知資料表：{property.Name}（驗證器未檢查其內容）。");
        }
        return new { File = fullPath, IsValid = errors.Count == 0, FormatVersion = formatVersion, DatabaseSchemaVersion = schemaVersion, TableCounts = counts, Errors = errors, Warnings = warnings };
    }

    private static bool HasNonEmptyString(JsonElement row, string name) =>
        row.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString());
}
