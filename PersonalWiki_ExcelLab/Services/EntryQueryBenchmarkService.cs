using System.Diagnostics;

namespace PersonalWiki.ExcelLab.Services;

public sealed record EntryQueryBenchmarkReport(
    string DatabasePath, bool ReadOnly, string? Keyword, string? EntryType, string? Tag,
    string ArchivedMode, string Sort, int Page, int PageSize,
    string? CreatedFrom, string? CreatedTo, string? UpdatedFrom, string? UpdatedTo, int Iterations,
    int TotalCount, int ReturnedCount, double MinimumMilliseconds,
    double MedianMilliseconds, double AverageMilliseconds, double MaximumMilliseconds,
    IReadOnlyList<double> SamplesMilliseconds, IReadOnlyList<string> Notes, IReadOnlyList<string> ResultIds);

/// <summary>Measures end-to-end read-only entry-query latency; it does not modify the database.</summary>
public sealed class EntryQueryBenchmarkService
{
    public EntryQueryBenchmarkReport Run(string databasePath, string? keyword = null,
        string? entryType = null, string? tag = null, string archivedMode = "exclude",
        int page = 1, int pageSize = 50, string sort = "updated-desc",
        string? createdFrom = null, string? createdTo = null,
        string? updatedFrom = null, string? updatedTo = null, int iterations = 5)
    {
        if (iterations is < 1 or > 20)
            throw new ArgumentOutOfRangeException(nameof(iterations), "iterations 必須介於 1 至 20。");

        var service = new EntryQueryService();
        // One unmeasured warm-up reduces first-use/JIT noise; all calls use the same read-only query path as CLI/Desktop.
        var warmup = service.Query(databasePath, keyword, entryType, tag, archivedMode, page, pageSize,
            sort, createdFrom, createdTo, updatedFrom, updatedTo);
        var samples = new List<double>(iterations);
        EntryQueryResult? last = null;
        for (var i = 0; i < iterations; i++)
        {
            var timer = Stopwatch.StartNew();
            last = service.Query(databasePath, keyword, entryType, tag, archivedMode, page, pageSize,
                sort, createdFrom, createdTo, updatedFrom, updatedTo);
            timer.Stop();
            samples.Add(timer.Elapsed.TotalMilliseconds);
            if (last.TotalCount != warmup.TotalCount || !last.Items.Select(x => x.Id).SequenceEqual(warmup.Items.Select(x => x.Id)))
                throw new InvalidOperationException("量測期間搜尋結果發生變化；資料庫可能正被其他程序修改，已停止產生基準報告。");
        }

        var ordered = samples.OrderBy(x => x).ToArray();
        var median = ordered.Length % 2 == 1
            ? ordered[ordered.Length / 2]
            : (ordered[ordered.Length / 2 - 1] + ordered[ordered.Length / 2]) / 2d;
        return new EntryQueryBenchmarkReport(Path.GetFullPath(databasePath), true, keyword, entryType,
            tag, archivedMode, sort, page, pageSize, createdFrom, createdTo, updatedFrom, updatedTo,
            iterations, last!.TotalCount, last.Items.Count,
            ordered[0], median, samples.Average(), ordered[^1], samples,
            new[]
            {
                "此報告量測 EntryQueryService 的端到端讀取耗時，包含連線開啟、COUNT、分頁結果及本頁標籤查詢。",
                "量測前執行一次未計時暖機；各次耗時會受作業系統快取、磁碟、背景負載及資料庫大小影響。",
                "此數值不是 SQLite EXPLAIN QUERY PLAN 的估算，也不是跨電腦效能保證；比較時請使用相同資料庫、篩選條件及環境。",
                "所有搜尋使用 SQLite ReadOnly 連線；本服務不寫入資料庫。量測期間若結果變化則拒絕產生報告。"
            }, last.Items.Select(x => x.Id).ToArray());
    }
}
