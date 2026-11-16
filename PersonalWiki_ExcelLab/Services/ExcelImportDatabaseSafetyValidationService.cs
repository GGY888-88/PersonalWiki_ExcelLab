using System.Security.Cryptography;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ExcelImportDatabaseSafetyValidationResult(
    bool IsValid,
    bool BackupExists,
    bool BackupHashMatches,
    bool BackupHealthy,
    bool BackupSchemaMatches,
    string ExpectedBackupSha256,
    string ActualBackupSha256,
    IReadOnlyList<string> Issues,
    ExcelImportDatabaseSafetyResult Report);

/// <summary>Revalidates a saved pre-import safety report and its backup file without modifying either.</summary>
public sealed class ExcelImportDatabaseSafetyValidationService
{
    public ExcelImportDatabaseSafetyValidationResult Validate(string reportPath)
    {
        var fullReport = Path.GetFullPath(reportPath);
        if (!File.Exists(fullReport)) throw new FileNotFoundException("找不到資料庫安全檢查報告。", fullReport);
        var report = JsonSerializer.Deserialize<ExcelImportDatabaseSafetyResult>(File.ReadAllText(fullReport))
            ?? throw new InvalidDataException("安全檢查報告 JSON 無法解析。");
        var issues = new List<string>();
        var backupExists = File.Exists(report.BackupDatabase);
        if (!backupExists)
        {
            issues.Add("備份檔不存在。");
            return new(false, false, false, false, false, report.BackupSha256, string.Empty, issues, report);
        }

        // Keep memory usage bounded for large databases and prevent concurrent writers
        // from changing the file while its integrity fingerprint is being calculated.
        using var backupStream = new FileStream(
            report.BackupDatabase,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1024 * 128,
            options: FileOptions.SequentialScan);
        var actualHash = Convert.ToHexString(SHA256.HashData(backupStream)).ToLowerInvariant();
        var hashMatches = string.Equals(actualHash, report.BackupSha256, StringComparison.OrdinalIgnoreCase);
        if (!hashMatches) issues.Add("備份檔 SHA-256 與安全檢查報告不一致，檔案可能已變更。");
        var health = new DatabaseHealthService().Check(report.BackupDatabase);
        if (!health.IsHealthy) issues.Add("備份資料庫完整性或外鍵檢查未通過。");
        var schemaMatches = health.SchemaVersion == report.ActualSchemaVersion && health.SchemaVersion == report.ExpectedSchemaVersion;
        if (!schemaMatches) issues.Add("備份資料庫 Schema 版本與報告不一致。");
        return new(issues.Count == 0, true, hashMatches, health.IsHealthy, schemaMatches,
            report.BackupSha256, actualHash, issues, report);
    }
}
