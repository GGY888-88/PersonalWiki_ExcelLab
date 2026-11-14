using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseTableInventory(string Table, long Rows);
public sealed record DatabaseIntegrityReportResult(string Database, string DatabaseSha256, long DatabaseBytes,
    int SchemaVersion, DatabaseHealthResult Health, SearchIndexCheckResult SearchIndex,
    IReadOnlyList<DatabaseTableInventory> Tables, bool IsHealthy, string ReportPath);

/// <summary>Creates a read-only integrity inventory and writes it to a new JSON file.</summary>
public sealed class DatabaseIntegrityReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseIntegrityReportResult Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("完整性報告已存在；為避免覆蓋已停止輸出。");

        var health = new DatabaseHealthService().Check(database);
        SearchIndexCheckResult index;
        try { index = new SearchIndexMaintenanceService().Check(database); }
        catch (Exception ex)
        {
            index = new SearchIndexCheckResult(database, -1, -1, -1, -1, false, true);
            _ = ex;
        }

        var tables = new List<DatabaseTableInventory>();
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
            using var reader = command.ExecuteReader();
            var names = new List<string>();
            while (reader.Read()) names.Add(reader.GetString(0));
            reader.Close();
            foreach (var name in names)
            {
                using var count = connection.CreateCommand();
                count.CommandText = $"SELECT COUNT(*) FROM {Quote(name)};";
                tables.Add(new DatabaseTableInventory(name, Convert.ToInt64(count.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture)));
            }
        }

        var info = new FileInfo(database);
        using var databaseStream = new FileStream(database, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var databaseHash = Convert.ToHexString(SHA256.HashData(databaseStream)).ToLowerInvariant();
        var result = new DatabaseIntegrityReportResult(database, databaseHash,
            info.Length, health.SchemaVersion, health, index, tables, health.IsHealthy && index.IsConsistent, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }

    private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
}
