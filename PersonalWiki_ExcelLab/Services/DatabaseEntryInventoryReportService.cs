using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record EntryTypeInventory(string EntryType, int TotalEntries, int ActiveEntries,
    int ArchivedEntries, int EntriesWithFields, int EntriesWithNotes, int EntriesWithImages,
    int EntriesWithTags, int TotalFields, int TotalNotes, int TotalImages, int TotalTags);

public sealed record DatabaseEntryInventoryReport(string Database, DateTimeOffset CreatedAtUtc,
    int TotalEntries, int ActiveEntries, int ArchivedEntries, int TotalFields, int TotalNotes,
    int TotalImages, int TotalTags, IReadOnlyList<EntryTypeInventory> ByEntryType, string ReportPath);

/// <summary>Creates a read-only aggregate inventory of entries and their related records.</summary>
public sealed class DatabaseEntryInventoryReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseEntryInventoryReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("條目清冊報告已存在；為避免覆蓋已停止輸出。");

        var groups = new List<EntryTypeInventory>();
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT e.entry_type,
                       COUNT(*) AS total_entries,
                       SUM(CASE WHEN e.is_archived = 0 THEN 1 ELSE 0 END) AS active_entries,
                       SUM(CASE WHEN e.is_archived = 1 THEN 1 ELSE 0 END) AS archived_entries,
                       SUM(CASE WHEN (SELECT COUNT(*) FROM entry_fields f WHERE f.entry_id=e.id) > 0 THEN 1 ELSE 0 END) AS entries_with_fields,
                       SUM(CASE WHEN (SELECT COUNT(*) FROM entry_notes n WHERE n.entry_id=e.id) > 0 THEN 1 ELSE 0 END) AS entries_with_notes,
                       SUM(CASE WHEN (SELECT COUNT(*) FROM entry_images i WHERE i.entry_id=e.id) > 0 THEN 1 ELSE 0 END) AS entries_with_images,
                       SUM(CASE WHEN (SELECT COUNT(*) FROM entry_tags t WHERE t.entry_id=e.id) > 0 THEN 1 ELSE 0 END) AS entries_with_tags,
                       SUM((SELECT COUNT(*) FROM entry_fields f WHERE f.entry_id=e.id)) AS total_fields,
                       SUM((SELECT COUNT(*) FROM entry_notes n WHERE n.entry_id=e.id)) AS total_notes,
                       SUM((SELECT COUNT(*) FROM entry_images i WHERE i.entry_id=e.id)) AS total_images,
                       SUM((SELECT COUNT(*) FROM entry_tags t WHERE t.entry_id=e.id)) AS total_tags
                FROM entries e
                GROUP BY e.entry_type
                ORDER BY e.entry_type COLLATE NOCASE;
                """;
            using var reader = command.ExecuteReader();
            while (reader.Read())
                groups.Add(new EntryTypeInventory(reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2),
                    reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5), reader.GetInt32(6),
                    reader.GetInt32(7), reader.GetInt32(8), reader.GetInt32(9), reader.GetInt32(10),
                    reader.GetInt32(11)));
        }

        var result = new DatabaseEntryInventoryReport(database, DateTimeOffset.UtcNow,
            groups.Sum(x => x.TotalEntries), groups.Sum(x => x.ActiveEntries), groups.Sum(x => x.ArchivedEntries),
            groups.Sum(x => x.TotalFields), groups.Sum(x => x.TotalNotes), groups.Sum(x => x.TotalImages),
            groups.Sum(x => x.TotalTags), groups, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
