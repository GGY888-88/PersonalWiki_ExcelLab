using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ExcelImportPreflightValidationResult(
    bool IsCurrent,
    bool CanImport,
    bool SourceWorkbookMatches,
    bool MappingMatches,
    bool CleaningRulesMatch,
    IReadOnlyList<string> Issues,
    ExcelImportPreflightResult Report);

/// <summary>Checks whether a saved preflight report still matches its input files.</summary>
public sealed class ExcelImportPreflightValidationService
{
    public ExcelImportPreflightValidationResult Validate(string reportPath, string workbookPath, string mappingPath, string rulesPath)
    {
        if (!File.Exists(reportPath)) throw new FileNotFoundException("找不到預檢報告。", reportPath);
        var report = JsonSerializer.Deserialize<ExcelImportPreflightResult>(File.ReadAllText(reportPath))
            ?? throw new InvalidDataException("預檢報告 JSON 無法解析。");
        var issues = new List<string>();
        var sourceMatches = string.Equals(report.SourceSha256, ExcelImportPreflightService.HashFile(workbookPath), StringComparison.OrdinalIgnoreCase);
        var mappingMatches = string.Equals(report.MappingSha256, ExcelImportPreflightService.HashFile(mappingPath), StringComparison.OrdinalIgnoreCase);
        var rulesMatch = string.Equals(report.CleaningRulesSha256, ExcelImportPreflightService.HashFile(rulesPath), StringComparison.OrdinalIgnoreCase);
        if (!sourceMatches) issues.Add("來源 Excel 與預檢報告產生時不同，必須重新預檢。");
        if (!mappingMatches) issues.Add("欄位對應設定與預檢報告產生時不同，必須重新預檢。");
        if (!rulesMatch) issues.Add("清理規則與預檢報告產生時不同，必須重新預檢。");
        if (!report.CanImport) issues.Add("原預檢結果未通過匯入條件。");
        return new ExcelImportPreflightValidationResult(sourceMatches && mappingMatches && rulesMatch,
            sourceMatches && mappingMatches && rulesMatch && report.CanImport,
            sourceMatches, mappingMatches, rulesMatch, issues, report);
    }
}
