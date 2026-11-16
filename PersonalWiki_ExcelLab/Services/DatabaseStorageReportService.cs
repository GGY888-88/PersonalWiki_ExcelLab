using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseStorageReport(
    string Database,
    DateTimeOffset CreatedAt,
    long FileSizeBytes,
    long PageSizeBytes,
    long PageCount,
    long FreePageCount,
    long UsedPageCount,
    int UserTableCount,
    int IndexCount,
    IReadOnlyList<DatabaseTableInventory> Tables,
    string ReportPath);

/// <summary>Creates a read-only SQLite storage inventory report without changing the database.</summary>
public sealed class DatabaseStorageReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseStorageReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("儲存空間報告已存在；為避免覆蓋已停止輸出。");

        long pageSize;
        long pageCount;
        long freePageCount;
        int indexCount;
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            pageSize = ReadPragma(connection, "page_size");
            pageCount = ReadPragma(connection, "page_count");
            freePageCount = ReadPragma(connection, "freelist_count");
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name NOT LIKE 'sqlite_%';";
            indexCount = Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
        }

        var tables = new DatabaseTableInventoryService().GetCounts(database);
        var result = new DatabaseStorageReport(database, DateTimeOffset.UtcNow, new FileInfo(database).Length,
            pageSize, pageCount, freePageCount, Math.Max(0, pageCount - freePageCount), tables.Count,
            indexCount, tables, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }

    private static long ReadPragma(SqliteConnection connection, string pragma)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA {pragma};";
        return Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }
}
