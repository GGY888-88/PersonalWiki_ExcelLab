using System.Security.Cryptography;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseIntegrityReportValidationResult(string Database, string Report,
    bool DatabasePathMatches, bool FileSizeMatches, bool Sha256Matches, bool SchemaMatches,
    bool HealthPassed, bool SearchIndexPassed, bool TableInventoryMatches, bool IsValid,
    IReadOnlyList<string> Issues);

/// <summary>Revalidates a previously generated integrity report against the current database.</summary>
public sealed class DatabaseIntegrityReportValidationService
{
    public DatabaseIntegrityReportValidationResult Validate(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var report = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (!File.Exists(report)) throw new FileNotFoundException("找不到完整性報告。", report);
        var expected = JsonSerializer.Deserialize<DatabaseIntegrityReportResult>(File.ReadAllText(report),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("完整性報告內容為空或格式錯誤。");
        var issues = new List<string>();
        var pathMatches = string.Equals(Path.GetFullPath(expected.Database), database, StringComparison.OrdinalIgnoreCase);
        if (!pathMatches) issues.Add("資料庫路徑與報告不一致。");
        var info = new FileInfo(database);
        var sizeMatches = info.Length == expected.DatabaseBytes;
        if (!sizeMatches) issues.Add("資料庫檔案大小已變更。");
        using var stream = new FileStream(database, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var hashMatches = Convert.ToHexString(SHA256.HashData(stream)).Equals(expected.DatabaseSha256, StringComparison.OrdinalIgnoreCase);
        if (!hashMatches) issues.Add("資料庫 SHA-256 已變更。");
        var health = new DatabaseHealthService().Check(database);
        var schemaMatches = health.SchemaVersion == expected.SchemaVersion;
        if (!schemaMatches) issues.Add("Schema 版本已變更。");
        var healthPassed = health.IsHealthy && expected.Health.IsHealthy;
        if (!healthPassed) issues.Add("資料庫健康檢查未通過。");
        var index = new SearchIndexMaintenanceService().Check(database);
        var indexPassed = index.IsConsistent && expected.SearchIndex.IsConsistent;
        if (!indexPassed) issues.Add("全文索引一致性檢查未通過。");
        var currentTables = new DatabaseTableInventoryService().GetCounts(database);
        var tableMatches = currentTables.Count == expected.Tables.Count && currentTables.All(current =>
            expected.Tables.Any(old => old.Table == current.Table && old.Rows == current.Rows));
        if (!tableMatches) issues.Add("一般資料表集合或筆數已變更。");
        return new DatabaseIntegrityReportValidationResult(database, report, pathMatches, sizeMatches,
            hashMatches, schemaMatches, healthPassed, indexPassed, tableMatches,
            pathMatches && sizeMatches && hashMatches && schemaMatches && healthPassed && indexPassed && tableMatches, issues);
    }
}
