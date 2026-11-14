using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseImageInventoryItem(
    string ImageId, string EntryId, string EntryTitle, string StoredPath,
    string? ResolvedPath, string FileStatus, long? FileSizeBytes,
    string? MimeType, string? RecordedSha256, string? Caption, int SortOrder,
    string? InspectionError);

public sealed record DatabaseImageInventoryReport(
    string Database, DateTimeOffset CreatedAtUtc, int TotalImageReferences,
    int ExistingFiles, int MissingFiles, int InvalidPaths, long TotalExistingFileBytes,
    IReadOnlyList<DatabaseImageInventoryItem> Items, string ReportPath);

/// <summary>Creates a read-only inventory of image references and their on-disk availability.</summary>
public sealed class DatabaseImageInventoryReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseImageInventoryReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("圖片清冊報告已存在；為避免覆蓋已停止輸出。");

        var items = new List<DatabaseImageInventoryItem>();
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT i.id, i.entry_id, e.title, i.file_path, i.mime_type,
                       i.sha256, i.caption, i.sort_order
                FROM entry_images i
                INNER JOIN entries e ON e.id = i.entry_id
                ORDER BY e.title COLLATE NOCASE, i.sort_order, i.id;
                """;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var storedPath = reader.GetString(3);
                var status = "Missing";
                string? resolvedPath = null;
                long? size = null;
                string? error = null;
                try
                {
                    resolvedPath = Path.GetFullPath(storedPath);
                    var info = new FileInfo(resolvedPath);
                    if (info.Exists)
                    {
                        status = "Exists";
                        size = info.Length;
                    }
                }
                catch (Exception ex) when (ex is ArgumentException or NotSupportedException or IOException or UnauthorizedAccessException)
                {
                    status = "InvalidPath";
                    error = ex.Message;
                }

                items.Add(new DatabaseImageInventoryItem(
                    reader.GetString(0), reader.GetString(1), reader.GetString(2), storedPath,
                    resolvedPath, status, size,
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    reader.IsDBNull(5) ? null : reader.GetString(5),
                    reader.IsDBNull(6) ? null : reader.GetString(6), reader.GetInt32(7), error));
            }
        }

        var existing = items.Where(x => x.FileStatus == "Exists").ToList();
        var result = new DatabaseImageInventoryReport(database, DateTimeOffset.UtcNow,
            items.Count, existing.Count, items.Count(x => x.FileStatus == "Missing"),
            items.Count(x => x.FileStatus == "InvalidPath"), existing.Sum(x => x.FileSizeBytes ?? 0),
            items, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
