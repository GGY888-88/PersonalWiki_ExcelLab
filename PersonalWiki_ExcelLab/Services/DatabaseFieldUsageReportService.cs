using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record FieldUsageBand(string UsageRange, int FieldKeyCount);
public sealed record FieldValueTypeCount(string ValueType, int FieldCount);
public sealed record DatabaseFieldUsageReport(
    string Database,
    DateTimeOffset CreatedAtUtc,
    int UniqueFieldKeyCount,
    int TotalFieldRecords,
    int EntriesWithFields,
    int BlankFieldKeyCount,
    IReadOnlyList<FieldUsageBand> UsageDistribution,
    IReadOnlyList<FieldValueTypeCount> ValueTypes,
    string ReportPath);

/// <summary>Creates a read-only aggregate field-usage report without exporting field names or values.</summary>
public sealed class DatabaseFieldUsageReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseFieldUsageReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("欄位使用統計報告已存在；為避免覆蓋已停止輸出。");

        var frequencies = new List<int>();
        var valueTypes = new List<FieldValueTypeCount>();
        int totalFields;
        int entriesWithFields;
        int blankKeys;
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*), COUNT(DISTINCT entry_id), SUM(CASE WHEN trim(field_key) = '' THEN 1 ELSE 0 END) FROM entry_fields;";
                using var reader = command.ExecuteReader();
                if (!reader.Read()) throw new InvalidDataException("無法讀取欄位統計資料。");
                totalFields = reader.GetInt32(0);
                entriesWithFields = reader.GetInt32(1);
                blankKeys = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT usage_count FROM (SELECT field_key, COUNT(DISTINCT entry_id) AS usage_count FROM entry_fields WHERE trim(field_key) <> '' GROUP BY field_key);";
                using var reader = command.ExecuteReader();
                while (reader.Read()) frequencies.Add(reader.GetInt32(0));
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT value_type, COUNT(*) FROM entry_fields GROUP BY value_type ORDER BY value_type COLLATE NOCASE;";
                using var reader = command.ExecuteReader();
                while (reader.Read()) valueTypes.Add(new FieldValueTypeCount(reader.IsDBNull(0) ? "(NULL)" : reader.GetString(0), reader.GetInt32(1)));
            }
        }

        var distribution = new[]
        {
            new FieldUsageBand("1 個條目", frequencies.Count(x => x == 1)),
            new FieldUsageBand("2–5 個條目", frequencies.Count(x => x is >= 2 and <= 5)),
            new FieldUsageBand("6–20 個條目", frequencies.Count(x => x is >= 6 and <= 20)),
            new FieldUsageBand("21 個以上條目", frequencies.Count(x => x >= 21))
        };
        var result = new DatabaseFieldUsageReport(database, DateTimeOffset.UtcNow, frequencies.Count,
            totalFields, entriesWithFields, blankKeys, distribution, valueTypes, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
