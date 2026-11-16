using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DuplicateImageReference(string ImageId, string EntryId, string EntryTitle,
    string FilePath, string? Sha256, string? Caption);
public sealed record DuplicateImageGroup(string Sha256, int ReferenceCount,
    IReadOnlyList<DuplicateImageReference> References);
public sealed record DatabaseImageDuplicateReport(string Database, DateTimeOffset CreatedAtUtc,
    int DuplicateHashGroupCount, int DuplicateReferenceCount,
    IReadOnlyList<DuplicateImageGroup> Groups, string ReportPath);

/// <summary>Creates a read-only report of image records sharing the same recorded SHA-256.</summary>
public sealed class DatabaseImageDuplicateReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseImageDuplicateReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("圖片重複報告已存在；為避免覆蓋已停止輸出。");

        var references = new List<DuplicateImageReference>();
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT i.id, i.entry_id, e.title, i.file_path, i.sha256, i.caption
                FROM entry_images i
                INNER JOIN entries e ON e.id = i.entry_id
                WHERE i.sha256 IS NOT NULL AND length(trim(i.sha256)) > 0
                ORDER BY lower(i.sha256), e.title COLLATE NOCASE, i.id;
                """;
            using var reader = command.ExecuteReader();
            while (reader.Read())
                references.Add(new DuplicateImageReference(reader.GetString(0), reader.GetString(1),
                    reader.GetString(2), reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4),
                    reader.IsDBNull(5) ? null : reader.GetString(5)));
        }

        var groups = references
            .GroupBy(x => x.Sha256!, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .Select(g => new DuplicateImageGroup(g.Key, g.Count(), g.ToList()))
            .ToList();
        var result = new DatabaseImageDuplicateReport(database, DateTimeOffset.UtcNow,
            groups.Count, groups.Sum(x => x.ReferenceCount), groups, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
