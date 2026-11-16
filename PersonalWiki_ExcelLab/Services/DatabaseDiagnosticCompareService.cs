using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseDiagnosticTableChange(string Table, long BeforeRows, long AfterRows, long Delta);
public sealed record DatabaseDiagnosticCompareReport(
    string Format,
    DateTimeOffset CreatedAtUtc,
    string BaselineReport,
    string CurrentReport,
    string DatabaseBefore,
    string DatabaseAfter,
    bool SameDatabasePath,
    int SchemaVersionBefore,
    int SchemaVersionAfter,
    bool SchemaVersionChanged,
    bool HealthyBefore,
    bool HealthyAfter,
    bool QuickCheckPassedBefore,
    bool QuickCheckPassedAfter,
    int ForeignKeyIssuesBefore,
    int ForeignKeyIssuesAfter,
    bool SearchIndexConsistentBefore,
    bool SearchIndexConsistentAfter,
    long PageSizeBytesBefore,
    long PageSizeBytesAfter,
    long PageCountBefore,
    long PageCountAfter,
    long FreePageCountBefore,
    long FreePageCountAfter,
    long UsedPageCountBefore,
    long UsedPageCountAfter,
    int IndexCountBefore,
    int IndexCountAfter,
    long DatabaseBytesBefore,
    long DatabaseBytesAfter,
    long DatabaseBytesDelta,
    long EntriesBefore,
    long EntriesAfter,
    long EntriesDelta,
    long IndexedEntriesBefore,
    long IndexedEntriesAfter,
    long MissingFtsBefore,
    long MissingFtsAfter,
    long OrphanFtsBefore,
    long OrphanFtsAfter,
    IReadOnlyList<DatabaseDiagnosticTableChange> TableChanges,
    IReadOnlyList<string> AddedTables,
    IReadOnlyList<string> RemovedTables,
    IReadOnlyList<string> WarningsBefore,
    IReadOnlyList<string> WarningsAfter,
    IReadOnlyList<string> AddedWarnings,
    IReadOnlyList<string> RemovedWarnings,
    bool HasChanges,
    string OutputPath);

/// <summary>Validates and compares two diagnostic snapshots without opening or changing either database.</summary>
public sealed class DatabaseDiagnosticCompareService
{
    private const string ExpectedFormat = "personalwiki-database-diagnostic-v1";
    private const long MaxReportBytes = 32L * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DatabaseDiagnosticCompareReport Create(string baselinePath, string currentPath, string outputPath)
    {
        var baselineFull = Path.GetFullPath(baselinePath);
        var currentFull = Path.GetFullPath(currentPath);
        var outputFull = Path.GetFullPath(outputPath);
        if (string.Equals(outputFull, baselineFull, StringComparison.OrdinalIgnoreCase)
            || string.Equals(outputFull, currentFull, StringComparison.OrdinalIgnoreCase))
            throw new IOException("比較報告輸出路徑不可與任一輸入報告相同。");

        var before = ReadAndValidate(baselineFull);
        var after = ReadAndValidate(currentFull);
        var beforeTables = before.Tables.ToDictionary(x => x.Table, x => x.Rows, StringComparer.Ordinal);
        var afterTables = after.Tables.ToDictionary(x => x.Table, x => x.Rows, StringComparer.Ordinal);
        var added = afterTables.Keys.Except(beforeTables.Keys, StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var removed = beforeTables.Keys.Except(afterTables.Keys, StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var common = beforeTables.Keys.Intersect(afterTables.Keys, StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal);
        var changes = common
            .Where(name => beforeTables[name] != afterTables[name])
            .Select(name => new DatabaseDiagnosticTableChange(name, beforeTables[name], afterTables[name], checked(afterTables[name] - beforeTables[name])))
            .ToArray();
        var beforeWarnings = before.Warnings.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var afterWarnings = after.Warnings.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var addedWarnings = afterWarnings.Except(beforeWarnings, StringComparer.Ordinal).ToArray();
        var removedWarnings = beforeWarnings.Except(afterWarnings, StringComparer.Ordinal).ToArray();
        var samePath = string.Equals(before.Database, after.Database, StringComparison.OrdinalIgnoreCase);
        var report = new DatabaseDiagnosticCompareReport(
            "personalwiki-database-diagnostic-compare-v1", DateTimeOffset.UtcNow,
            baselineFull, currentFull, before.Database, after.Database, samePath,
            before.Health.SchemaVersion, after.Health.SchemaVersion,
            before.Health.SchemaVersion != after.Health.SchemaVersion,
            before.IsHealthy, after.IsHealthy,
            before.Health.QuickCheckPassed, after.Health.QuickCheckPassed,
            before.Health.ForeignKeyIssueCount, after.Health.ForeignKeyIssueCount,
            before.SearchIndex.IsConsistent, after.SearchIndex.IsConsistent,
            before.PageSizeBytes, after.PageSizeBytes,
            before.PageCount, after.PageCount, before.FreePageCount, after.FreePageCount,
            before.UsedPageCount, after.UsedPageCount,
            before.IndexCount, after.IndexCount,
            before.DatabaseBytes, after.DatabaseBytes, checked(after.DatabaseBytes - before.DatabaseBytes),
            before.SearchIndex.EntryCount, after.SearchIndex.EntryCount,
            checked(after.SearchIndex.EntryCount - before.SearchIndex.EntryCount),
            before.SearchIndex.IndexedCount, after.SearchIndex.IndexedCount,
            before.SearchIndex.MissingCount, after.SearchIndex.MissingCount,
            before.SearchIndex.OrphanCount, after.SearchIndex.OrphanCount,
            changes, added, removed, beforeWarnings, afterWarnings, addedWarnings, removedWarnings,
            !samePath || before.IsHealthy != after.IsHealthy || before.Health.SchemaVersion != after.Health.SchemaVersion
                || before.Health.QuickCheckPassed != after.Health.QuickCheckPassed
                || before.Health.ForeignKeyIssueCount != after.Health.ForeignKeyIssueCount
                || before.SearchIndex.IsConsistent != after.SearchIndex.IsConsistent
                || before.DatabaseBytes != after.DatabaseBytes
                || before.PageSizeBytes != after.PageSizeBytes
                || before.PageCount != after.PageCount || before.FreePageCount != after.FreePageCount
                || before.UsedPageCount != after.UsedPageCount || before.IndexCount != after.IndexCount
                || before.SearchIndex.EntryCount != after.SearchIndex.EntryCount
                || before.SearchIndex.IndexedCount != after.SearchIndex.IndexedCount
                || before.SearchIndex.MissingCount != after.SearchIndex.MissingCount
                || before.SearchIndex.OrphanCount != after.SearchIndex.OrphanCount
                || changes.Length > 0 || added.Length > 0 || removed.Length > 0
                || addedWarnings.Length > 0 || removedWarnings.Length > 0,
            outputFull);

        var directory = Path.GetDirectoryName(outputFull);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        var temporary = outputFull + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, report, new JsonSerializerOptions { WriteIndented = true });
                stream.Flush(true);
            }
            File.Move(temporary, outputFull, overwrite: false);
        }
        catch
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); } catch { }
            throw;
        }
        return report;
    }

    private static DatabaseDiagnosticReport ReadAndValidate(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("找不到資料庫診斷報告。", path);
        var info = new FileInfo(path);
        if (info.Length <= 0 || info.Length > MaxReportBytes)
            throw new InvalidDataException("診斷報告大小無效或超過 32 MiB 限制。");
        DatabaseDiagnosticReport? report;
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length <= 0 || stream.Length > MaxReportBytes)
                throw new InvalidDataException("診斷報告大小無效或超過 32 MiB 限制。");
            report = JsonSerializer.Deserialize<DatabaseDiagnosticReport>(stream, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("診斷報告不是有效的 JSON 或格式不相容。", ex);
        }
        if (report is null || report.Format != ExpectedFormat || report.Health is null || report.SearchIndex is null
            || report.Tables is null || report.Warnings is null || report.Warnings.Any(string.IsNullOrWhiteSpace)
            || string.IsNullOrWhiteSpace(report.Database) || string.IsNullOrWhiteSpace(report.ReportPath)
            || report.DatabaseBytes < 0 || report.PageSizeBytes <= 0 || report.PageCount < 0
            || report.FreePageCount < 0 || report.FreePageCount > report.PageCount
            || report.UsedPageCount != report.PageCount - report.FreePageCount || report.IndexCount < 0
            || report.SearchIndex.Database is null || report.Health.Database is null
            || !string.Equals(report.Database, report.SearchIndex.Database, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(report.Database, report.Health.Database, StringComparison.OrdinalIgnoreCase)
            || !report.SearchIndex.ReadOnly || !report.Health.ReadOnly
            || report.SearchIndex.EntryCount < -1 || report.SearchIndex.IndexedCount < -1
            || report.SearchIndex.MissingCount < -1 || report.SearchIndex.OrphanCount < -1
            || report.Health.SchemaVersion < 0 || report.Health.ForeignKeyIssueCount < 0
            || report.Health.QuickCheck is null || report.Health.ForeignKeyIssues is null
            || report.Health.QuickCheck.Any(string.IsNullOrWhiteSpace)
            || report.Health.ForeignKeyIssueCount != report.Health.ForeignKeyIssues.Count
            || report.Health.QuickCheckPassed != (report.Health.QuickCheck.Count == 1
                && report.Health.QuickCheck[0].Equals("ok", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("診斷報告缺少必要欄位或包含無效統計值。");

        var indexCountsUnavailable = report.SearchIndex.EntryCount == -1
            && report.SearchIndex.IndexedCount == -1 && report.SearchIndex.MissingCount == -1
            && report.SearchIndex.OrphanCount == -1;
        var indexCountsValid = report.SearchIndex.EntryCount >= 0 && report.SearchIndex.IndexedCount >= 0
            && report.SearchIndex.MissingCount >= 0 && report.SearchIndex.OrphanCount >= 0;
        if ((!indexCountsUnavailable && !indexCountsValid)
            || (indexCountsUnavailable && report.SearchIndex.IsConsistent)
            || (indexCountsValid && report.SearchIndex.IsConsistent !=
                (report.SearchIndex.MissingCount == 0 && report.SearchIndex.OrphanCount == 0
                    && report.SearchIndex.EntryCount == report.SearchIndex.IndexedCount))
            || (report.Health.IsHealthy && (!report.Health.QuickCheckPassed || report.Health.ForeignKeyIssueCount != 0))
            || (report.IsHealthy && (!report.Health.IsHealthy || !report.SearchIndex.IsConsistent
                || report.Warnings.Count != 0 || report.Tables.Any(x => x is null || x.Rows < 0))))
            throw new InvalidDataException("診斷報告的健康狀態或全文索引統計互相矛盾。");

        if (report.Tables.Any(x => x is null || string.IsNullOrWhiteSpace(x.Table) || x.Rows < -1)
            || report.Tables.Select(x => x.Table).Distinct(StringComparer.Ordinal).Count() != report.Tables.Count)
            throw new InvalidDataException("診斷報告的資料表清單無效或包含重複名稱。");
        return report;
    }
}
