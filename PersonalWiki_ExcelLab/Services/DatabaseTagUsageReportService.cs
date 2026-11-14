using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record TagUsageBand(string UsageRange, int TagCount);

public sealed record DatabaseTagUsageReport(
    string Database,
    DateTimeOffset CreatedAtUtc,
    int TaggedEntryCount,
    int UniqueNormalizedTagCount,
    int TotalTagAssignments,
    int SingleUseTagCount,
    int MultiUseTagCount,
    IReadOnlyList<TagUsageBand> UsageDistribution,
    string ReportPath);

/// <summary>Creates a read-only aggregate report of tag usage without exporting tag text.</summary>
public sealed class DatabaseTagUsageReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseTagUsageReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("標籤使用統計報告已存在；為避免覆蓋已停止輸出。");

        var frequencies = new List<int>();
        int taggedEntries;
        int assignments;
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(DISTINCT entry_id), COUNT(*) FROM entry_tags;";
                using var reader = command.ExecuteReader();
                if (!reader.Read()) throw new InvalidDataException("無法讀取標籤統計資料。");
                taggedEntries = reader.GetInt32(0);
                assignments = reader.GetInt32(1);
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT usage_count FROM (SELECT normalized_tag, COUNT(DISTINCT entry_id) AS usage_count FROM entry_tags GROUP BY normalized_tag);";
                using var reader = command.ExecuteReader();
                while (reader.Read()) frequencies.Add(reader.GetInt32(0));
            }
        }

        var uniqueTags = frequencies.Count;
        var singleUse = frequencies.Count(x => x == 1);
        var multiUse = frequencies.Count(x => x > 1);
        var distribution = new[]
        {
            new TagUsageBand("1 個條目", singleUse),
            new TagUsageBand("2–5 個條目", frequencies.Count(x => x is >= 2 and <= 5)),
            new TagUsageBand("6–20 個條目", frequencies.Count(x => x is >= 6 and <= 20)),
            new TagUsageBand("21 個以上條目", frequencies.Count(x => x >= 21))
        };
        var result = new DatabaseTagUsageReport(database, DateTimeOffset.UtcNow, taggedEntries,
            uniqueTags, assignments, singleUse, multiUse, distribution, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
