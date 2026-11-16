using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ImportCleaningRule(
    string RuleId,
    bool Enabled,
    string Action,
    IReadOnlyList<string> TargetFields,
    string? Description);

public sealed record ImportCleaningRulesSheet(
    string SheetName,
    IReadOnlyList<ImportCleaningRule> Rules);

public sealed record ImportCleaningRulesProfile(
    int SchemaVersion,
    string SourceFileName,
    string MappingFileName,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<ImportCleaningRulesSheet> Sheets,
    IReadOnlyList<string> Instructions);

public sealed record ImportCleaningRulesIssue(string Severity, string SheetName, string Message);

public sealed record ImportCleaningRulesCheckResult(
    bool IsValid,
    int SheetCount,
    int EnabledRuleCount,
    IReadOnlyList<ImportCleaningRulesIssue> Issues,
    IReadOnlyList<string> SafetyNotes);

public static class ImportCleaningRulesService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly (string Id, string Description)[] SupportedRules =
    {
        ("missing-title", "標題欄位空白或缺少內容"),
        ("duplicate-title", "標題重複（去除前後空白後、不區分大小寫）"),
        ("edge-whitespace", "欄位值含前後空白"),
        ("control-characters", "欄位值含非 Tab、CR、LF 的控制字元"),
        ("excel-error", "欄位值為 Excel 錯誤值")
    };

    public static ImportCleaningRulesProfile CreateTemplate(string workbookPath, string mappingPath)
    {
        var mappingCheck = ImportMappingService.Validate(workbookPath, mappingPath);
        if (!mappingCheck.IsValid)
            throw new InvalidDataException("欄位對應設定含有 Error；請先修正 mapping-check 結果。");

        var mapping = JsonSerializer.Deserialize<ImportMappingProfile>(File.ReadAllText(mappingPath))
            ?? throw new InvalidDataException("欄位對應 JSON 無法解析。");
        var sheets = mapping.Sheets.Select(sheet => new ImportCleaningRulesSheet(
            sheet.SheetName,
            SupportedRules.Select(rule => new ImportCleaningRule(
                rule.Id,
                true,
                "Review",
                Array.Empty<string>(),
                rule.Description)).ToList())).ToList();

        return new ImportCleaningRulesProfile(
            1,
            Path.GetFileName(workbookPath),
            Path.GetFileName(mappingPath),
            DateTimeOffset.UtcNow,
            sheets,
            new[]
            {
                "此規則檔只定義檢查項目，不會直接修改 Excel 或資料庫。",
                "Action 目前只允許 Review：將問題列為待人工確認。",
                "TargetFields 留空代表套用該工作表所有已對應欄位；標題規則固定使用 Mode=Title 欄位。",
                "Enabled=false 可停用單一規則；停用不會刪除原始資料。",
                "新增規則 ID 必須是程式支援的規則 ID。"
            });
    }

    public static void SaveTemplate(string workbookPath, string mappingPath, string outputPath)
    {
        var profile = CreateTemplate(workbookPath, mappingPath);
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
        SafeFileOutput.WriteAllText(fullPath, JsonSerializer.Serialize(profile, JsonOptions));
    }

    public static ImportCleaningRulesCheckResult Validate(
        string workbookPath, string mappingPath, string rulesPath)
    {
        var mappingCheck = ImportMappingService.Validate(workbookPath, mappingPath);
        var issues = new List<ImportCleaningRulesIssue>();
        if (!mappingCheck.IsValid)
        {
            issues.Add(new ImportCleaningRulesIssue("Error", "", "欄位對應設定含有 Error，請先修正 mapping-check。"));
            return new ImportCleaningRulesCheckResult(false, 0, 0, issues,
                new[] { "因 mapping 尚未通過驗證，已停止後續規則比對。", "此命令只讀取設定，不會修改 Excel 或資料庫。" });
        }

        if (!File.Exists(rulesPath))
            throw new FileNotFoundException("找不到清理規則設定檔。", rulesPath);

        var rules = JsonSerializer.Deserialize<ImportCleaningRulesProfile>(File.ReadAllText(rulesPath))
            ?? throw new InvalidDataException("清理規則 JSON 無法解析。");
        if (rules.Sheets is null)
            throw new InvalidDataException("rules.Sheets 不可為 null。");
        if (rules.SchemaVersion != 1)
            throw new InvalidDataException($"不支援的清理規則版本：{rules.SchemaVersion}");

        var mapping = JsonSerializer.Deserialize<ImportMappingProfile>(File.ReadAllText(mappingPath))
            ?? throw new InvalidDataException("欄位對應 JSON 無法解析。");
        if (mapping.Sheets is null || mapping.Sheets.Any(sheet => sheet is null || string.IsNullOrWhiteSpace(sheet.SheetName))
            || mapping.Sheets.Select(sheet => sheet.SheetName).Distinct(StringComparer.Ordinal).Count() != mapping.Sheets.Count)
            throw new InvalidDataException("mapping.Sheets 含有空值、空白名稱或重複工作表；請先修正 mapping-check。");
        var mappingSheets = mapping.Sheets.ToDictionary(sheet => sheet.SheetName, StringComparer.Ordinal);
        var enabledCount = 0;
        var seenSheets = new HashSet<string>(StringComparer.Ordinal);

        foreach (var sheetRules in rules.Sheets)
        {
            if (sheetRules is null || string.IsNullOrWhiteSpace(sheetRules.SheetName) || sheetRules.Rules is null)
            {
                issues.Add(new ImportCleaningRulesIssue("Error", "", "工作表規則設定、名稱及 Rules 不可為 null 或空白。"));
                continue;
            }
            if (!seenSheets.Add(sheetRules.SheetName))
            {
                issues.Add(new ImportCleaningRulesIssue("Error", sheetRules.SheetName, "清理規則中有重複工作表設定。"));
                continue;
            }
            if (!mappingSheets.TryGetValue(sheetRules.SheetName, out var mappingSheet))
            {
                issues.Add(new ImportCleaningRulesIssue("Error", sheetRules.SheetName, "此工作表不存在於欄位對應設定。"));
                continue;
            }

            var sheetIgnored = mappingSheet.Columns.Count > 0 && mappingSheet.Columns.All(column =>
                string.Equals(column.Mode, "Ignore", StringComparison.OrdinalIgnoreCase));
            if (sheetIgnored)
                continue;

            var mappedTargets = mappingSheet.Columns
                .Where(column => column.Mode is not null
                    && !column.Mode.Equals("Unmapped", StringComparison.OrdinalIgnoreCase)
                    && !column.Mode.Equals("Ignore", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(column.TargetField))
                .Select(column => column.TargetField!.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var titleColumns = mappingSheet.Columns
                .Where(column => column is not null && string.Equals(column.Mode, "Title", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var seenRuleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rule in sheetRules.Rules)
            {
                if (rule is null || string.IsNullOrWhiteSpace(rule.RuleId) || string.IsNullOrWhiteSpace(rule.Action)
                    || rule.TargetFields is null)
                {
                    issues.Add(new ImportCleaningRulesIssue("Error", sheetRules.SheetName,
                        "規則項目、RuleId、Action 及 TargetFields 不可為 null 或空白。"));
                    continue;
                }
                if (!seenRuleIds.Add(rule.RuleId))
                    issues.Add(new ImportCleaningRulesIssue("Error", sheetRules.SheetName, $"規則 ID 重複：{rule.RuleId}"));
                if (!SupportedRules.Any(item => item.Id.Equals(rule.RuleId, StringComparison.OrdinalIgnoreCase)))
                    issues.Add(new ImportCleaningRulesIssue("Error", sheetRules.SheetName, $"不支援的規則 ID：{rule.RuleId}"));
                if (!rule.Action.Equals("Review", StringComparison.OrdinalIgnoreCase))
                    issues.Add(new ImportCleaningRulesIssue("Error", sheetRules.SheetName,
                        $"規則 {rule.RuleId} 的 Action 必須是 Review；目前不允許自動修改或刪除。"));
                if (rule.Enabled)
                    enabledCount++;

                if (rule.RuleId.Equals("missing-title", StringComparison.OrdinalIgnoreCase)
                    || rule.RuleId.Equals("duplicate-title", StringComparison.OrdinalIgnoreCase))
                {
                    if (titleColumns.Count != 1)
                        issues.Add(new ImportCleaningRulesIssue("Error", sheetRules.SheetName,
                            $"規則 {rule.RuleId} 需要且只能有一個 Title 欄位。"));
                    if (rule.TargetFields.Count > 0)
                        issues.Add(new ImportCleaningRulesIssue("Error", sheetRules.SheetName,
                            $"規則 {rule.RuleId} 不可指定 TargetFields；它固定使用 Title 欄位。"));
                }

                foreach (var target in rule.TargetFields)
                {
                    if (string.IsNullOrWhiteSpace(target) || !string.Equals(target, target.Trim(), StringComparison.Ordinal))
                    {
                        issues.Add(new ImportCleaningRulesIssue("Error", sheetRules.SheetName,
                            $"規則 {rule.RuleId} 的 TargetFields 不可空白或含前後空白。"));
                        continue;
                    }
                    if (!mappedTargets.Contains(target))
                        issues.Add(new ImportCleaningRulesIssue("Error", sheetRules.SheetName,
                            $"規則 {rule.RuleId} 指定了未對應的 TargetField：{target}"));
                }
            }

            foreach (var supported in SupportedRules)
            {
                if (!seenRuleIds.Contains(supported.Id))
                    issues.Add(new ImportCleaningRulesIssue("Warning", sheetRules.SheetName,
                        $"未設定支援的規則：{supported.Id}"));
            }
        }

        foreach (var mappingSheet in mapping.Sheets)
        {
            if (!seenSheets.Contains(mappingSheet.SheetName))
                issues.Add(new ImportCleaningRulesIssue("Warning", mappingSheet.SheetName, "此工作表尚未加入清理規則設定。"));
        }

        if (enabledCount == 0)
            issues.Add(new ImportCleaningRulesIssue("Warning", "", "目前沒有啟用任何清理規則。"));

        return new ImportCleaningRulesCheckResult(
            issues.All(issue => issue.Severity != "Error"),
            rules.Sheets.Count,
            enabledCount,
            issues,
            new[]
            {
                "驗證過程只讀取 Excel、mapping JSON 與 rules JSON。",
                "本版本只支援 Review，不會自動刪除、覆寫或清理資料。",
                "此命令只驗證設定，不會執行清理或匯入。"
            });
    }
}
