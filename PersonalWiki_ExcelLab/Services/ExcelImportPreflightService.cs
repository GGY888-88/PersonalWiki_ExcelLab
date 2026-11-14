using System.Security.Cryptography;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ExcelImportPreflightResult(
    string SourceFile,
    string SourceSha256,
    string MappingSha256,
    string CleaningRulesSha256,
    ImportMappingCheckResult Mapping,
    ImportCleaningRulesCheckResult CleaningRules,
    ImportDryRunReport? DryRun,
    bool CanImport,
    IReadOnlyList<string> BlockingReasons,
    IReadOnlyList<string> SafetyNotes);

/// <summary>Runs the existing Excel import checks as one read-only preflight operation.</summary>
public sealed class ExcelImportPreflightService
{
    public ExcelImportPreflightResult Check(string workbookPath, string mappingPath, string rulesPath)
    {
        var fullWorkbook = Path.GetFullPath(workbookPath);
        var fullMapping = Path.GetFullPath(mappingPath);
        var fullRules = Path.GetFullPath(rulesPath);
        if (!File.Exists(fullWorkbook)) throw new FileNotFoundException("找不到來源 Excel。", fullWorkbook);
        if (!File.Exists(fullMapping)) throw new FileNotFoundException("找不到欄位對應設定檔。", fullMapping);
        if (!File.Exists(fullRules)) throw new FileNotFoundException("找不到清理規則設定檔。", fullRules);

        // Capture a stable input fingerprint before analysis, then verify it again afterwards.
        // This prevents a report from claiming a clean preflight when any input changed mid-scan.
        var sourceHashBefore = HashFile(fullWorkbook);
        var mappingHashBefore = HashFile(fullMapping);
        var rulesHashBefore = HashFile(fullRules);
        var mapping = ImportMappingService.Validate(fullWorkbook, fullMapping);
        var rules = ImportCleaningRulesService.Validate(fullWorkbook, fullMapping, fullRules);
        ImportDryRunReport? dryRun = null;
        var blockers = new List<string>();
        if (!mapping.IsValid) blockers.Add("欄位對應設定未通過驗證。");
        if (!rules.IsValid) blockers.Add("清理規則未通過驗證。");
        if (mapping.IsValid && rules.IsValid)
        {
            dryRun = ImportDryRunService.Analyze(fullWorkbook, mappingPath, rulesPath);
            if (dryRun.RowsRequiringReview > 0) blockers.Add($"有 {dryRun.RowsRequiringReview} 列需要人工檢查。");
        }
        var sourceHashAfter = HashFile(fullWorkbook);
        var mappingHashAfter = HashFile(fullMapping);
        var rulesHashAfter = HashFile(fullRules);
        if (!string.Equals(sourceHashBefore, sourceHashAfter, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(mappingHashBefore, mappingHashAfter, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(rulesHashBefore, rulesHashAfter, StringComparison.OrdinalIgnoreCase))
            throw new IOException("預檢分析期間來源 Excel 或設定檔發生變動；本次結果已作廢，請確認檔案穩定後重新預檢。未執行資料庫寫入。");

        return new ExcelImportPreflightResult(fullWorkbook,
            sourceHashAfter, mappingHashAfter, rulesHashAfter,
            mapping, rules, dryRun, blockers.Count == 0, blockers,
            new[] { "本命令只讀取 Excel 與設定檔，不寫入 SQLite。", "不修改來源 Excel、圖片或設定檔。", "CanImport=true 代表目前預檢條件通過；正式匯入仍會重新執行檢查。", "正式匯入前仍建議先備份資料庫。" });
    }

    internal static string HashFile(string path)
    {
        // Stream hashing keeps large workbooks from being loaded entirely into memory.
        // FileShare.Read permits other readers but denies concurrent writers/deletion on Windows.
        using var stream = new FileStream(Path.GetFullPath(path), FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 128, FileOptions.SequentialScan);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
