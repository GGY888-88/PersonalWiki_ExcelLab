using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ImportMappingColumn(
    int SourceIndex,
    string SourceName,
    string? TargetField,
    string Mode,
    string? Description);

public sealed record ImportMappingSheet(
    string SheetName,
    uint HeaderRow,
    uint FirstDataRow,
    IReadOnlyList<ImportMappingColumn> Columns);

public sealed record ImportMappingProfile(
    int SchemaVersion,
    string SourceFileName,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<ImportMappingSheet> Sheets,
    IReadOnlyList<string> Instructions);

public sealed record ImportMappingIssue(string Severity, string SheetName, string Message);

public sealed record ImportMappingCheckResult(
    bool IsValid,
    int SheetCount,
    int MappedColumnCount,
    IReadOnlyList<ImportMappingIssue> Issues);

public static class ImportMappingService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly HashSet<string> AllowedModes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Unmapped", "Title", "Content", "Attribute", "Ignore"
    };

    public static ImportMappingProfile CreateTemplate(string workbookPath)
    {
        var preview = ImportPreviewService.Analyze(workbookPath, 1);
        var sheets = preview.Sheets.Select(sheet => new ImportMappingSheet(
            sheet.Name,
            sheet.HeaderRow,
            sheet.HeaderRow == 0 ? 0 : sheet.HeaderRow + 1,
            sheet.Columns.Select(column => new ImportMappingColumn(
                column.Index,
                column.Name,
                null,
                "Unmapped",
                null)).ToList())).ToList();

        return new ImportMappingProfile(
            1,
            Path.GetFileName(workbookPath),
            DateTimeOffset.UtcNow,
            sheets,
            new[]
            {
                "此檔案只是欄位對應設定範本，不會匯入資料。",
                "Mode 可設定為 Unmapped、Title、Content、Attribute 或 Ignore。",
                "Title 表示百科條目名稱；Content 表示主要文字內容；Attribute 表示一般屬性欄位；Ignore 表示不匯入。",
                "請為要匯入的工作表指定欄位用途；每張工作表最多只能有一個 Title 欄位。",
                "TargetField 請填入資料庫欄位代碼；相同工作表內不可重複。"
            });
    }

    public static void SaveTemplate(string workbookPath, string outputPath)
    {
        var profile = CreateTemplate(workbookPath);
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
        SafeFileOutput.WriteAllText(fullPath, JsonSerializer.Serialize(profile, JsonOptions));
    }

    public static ImportMappingCheckResult Validate(string workbookPath, string mappingPath)
    {
        if (!File.Exists(mappingPath))
            throw new FileNotFoundException("找不到欄位對應設定檔。", mappingPath);

        var profile = JsonSerializer.Deserialize<ImportMappingProfile>(File.ReadAllText(mappingPath))
            ?? throw new InvalidDataException("欄位對應 JSON 無法解析。");
        if (profile.SchemaVersion != 1)
            throw new InvalidDataException($"不支援的欄位對應版本：{profile.SchemaVersion}");
        if (profile.Sheets is null)
            throw new InvalidDataException("mapping.Sheets 不可為 null。");

        var source = ImportPreviewService.Analyze(workbookPath, 1);
        var issues = new List<ImportMappingIssue>();
        var mappedCount = 0;
        var sourceSheets = source.Sheets.ToDictionary(sheet => sheet.Name, StringComparer.Ordinal);
        var seenSheets = new HashSet<string>(StringComparer.Ordinal);

        foreach (var mappingSheet in profile.Sheets)
        {
            if (mappingSheet is null)
            {
                issues.Add(new ImportMappingIssue("Error", "", "工作表設定不可為 null。"));
                continue;
            }
            if (string.IsNullOrWhiteSpace(mappingSheet.SheetName))
            {
                issues.Add(new ImportMappingIssue("Error", "", "工作表名稱不可空白。"));
                continue;
            }
            if (!seenSheets.Add(mappingSheet.SheetName))
            {
                issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName, "工作表設定重複。"));
                continue;
            }
            if (!sourceSheets.TryGetValue(mappingSheet.SheetName, out var sourceSheet))
            {
                issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName, "來源活頁簿找不到此工作表。"));
                continue;
            }
            if (mappingSheet.Columns is null)
            {
                issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName, "Columns 不可為 null。"));
                continue;
            }
            if (mappingSheet.HeaderRow != sourceSheet.HeaderRow)
                issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName,
                    $"標題列已變更：設定為 {mappingSheet.HeaderRow}，目前為 {sourceSheet.HeaderRow}。"));
            if (mappingSheet.HeaderRow == 0)
                issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName, "來源工作表沒有可用標題列。"));
            if (mappingSheet.FirstDataRow <= mappingSheet.HeaderRow)
                issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName,
                    $"FirstDataRow 必須大於 HeaderRow（目前 {mappingSheet.FirstDataRow}）。"));

            var titleCount = 0;
            var targetFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var sourceIndexes = new HashSet<int>();
            foreach (var column in mappingSheet.Columns)
            {
                if (column is null)
                {
                    issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName, "欄位設定不可為 null。"));
                    continue;
                }
                if (column.SourceIndex <= 0 || !sourceIndexes.Add(column.SourceIndex))
                {
                    issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName,
                        $"SourceIndex 必須為正整數且不可重複：{column.SourceIndex}。"));
                    continue;
                }
                if (string.IsNullOrWhiteSpace(column.Mode) || !AllowedModes.Contains(column.Mode))
                {
                    issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName,
                        $"欄位 {column.SourceName} 使用不支援的 Mode：{column.Mode}。"));
                    continue;
                }

                var currentColumn = sourceSheet.Columns.FirstOrDefault(item => item.Index == column.SourceIndex);
                if (currentColumn is null || !string.Equals(currentColumn.Name, column.SourceName, StringComparison.Ordinal))
                {
                    issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName,
                        $"來源欄位已變更或不存在：第 {column.SourceIndex} 欄「{column.SourceName}」。"));
                    continue;
                }

                if (column.Mode.Equals("Unmapped", StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(new ImportMappingIssue("Warning", mappingSheet.SheetName,
                        $"欄位「{column.SourceName}」尚未設定用途。"));
                    if (!string.IsNullOrWhiteSpace(column.TargetField))
                        issues.Add(new ImportMappingIssue("Warning", mappingSheet.SheetName,
                            $"未對應欄位「{column.SourceName}」仍填有 TargetField，該值不會使用。"));
                    continue;
                }
                if (column.Mode.Equals("Ignore", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(column.TargetField))
                        issues.Add(new ImportMappingIssue("Warning", mappingSheet.SheetName,
                            $"忽略欄位「{column.SourceName}」仍填有 TargetField，該值不會使用。"));
                    continue;
                }
                mappedCount++;
                if (column.Mode.Equals("Attribute", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(column.TargetField))
                    {
                        issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName,
                            $"屬性欄位「{column.SourceName}」缺少欄位代碼。"));
                        continue;
                    }
                    if (!string.Equals(column.TargetField, column.TargetField.Trim(), StringComparison.Ordinal))
                        issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName,
                            $"屬性欄位代碼「{column.TargetField}」前後不可有空白。"));
                    if (!targetFields.Add(column.TargetField.Trim()))
                        issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName,
                            $"屬性欄位代碼重複：{column.TargetField}。"));
                }
                if (column.Mode.Equals("Title", StringComparison.OrdinalIgnoreCase))
                    titleCount++;
            }

            if (sourceIndexes.Count != sourceSheet.Columns.Count)
                issues.Add(new ImportMappingIssue("Warning", mappingSheet.SheetName,
                    $"欄位設定數量 {sourceIndexes.Count} 與來源欄位數量 {sourceSheet.Columns.Count} 不同；未列出的來源欄位不會被對應。"));
            if (titleCount > 1)
                issues.Add(new ImportMappingIssue("Error", mappingSheet.SheetName, "每張工作表最多只能指定一個 Title 欄位。"));
            var sheetIgnored = mappingSheet.Columns.Count > 0 && mappingSheet.Columns.All(column =>
                string.Equals(column.Mode, "Ignore", StringComparison.OrdinalIgnoreCase));
            if (titleCount == 0 && !sheetIgnored)
                issues.Add(new ImportMappingIssue("Warning", mappingSheet.SheetName, "尚未指定 Title 欄位；若此工作表不匯入，請將全部欄位設為「忽略」。"));
        }

        foreach (var sourceSheet in source.Sheets)
        {
            if (!seenSheets.Contains(sourceSheet.Name))
                issues.Add(new ImportMappingIssue("Warning", sourceSheet.Name, "此工作表尚未加入欄位對應設定。"));
        }

        return new ImportMappingCheckResult(
            issues.All(issue => issue.Severity != "Error"),
            profile.Sheets.Count,
            mappedCount,
            issues);
    }

}
