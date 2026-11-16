using Microsoft.Data.Sqlite;
using System.Text;

namespace PersonalWiki.ExcelLab.Services;

/// <summary>唯讀檢查名稱完全相同（忽略前後空白、大小寫及 Unicode 相容字元）的條目。</summary>
public sealed class DuplicateEntryReportService
{
    public IReadOnlyList<DuplicateEntryGroup> Find(string databasePath)
    {
        var fullPath = Path.GetFullPath(databasePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("找不到知識庫。", fullPath);
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = fullPath, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id,title,entry_type,is_archived,updated_at FROM entries ORDER BY title COLLATE NOCASE,updated_at DESC;";
        var rows = new List<DuplicateEntryRow>();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var title = reader.GetString(1);
                var key = title.Trim().Normalize(NormalizationForm.FormKC).ToUpperInvariant();
                if (key.Length == 0) continue;
                rows.Add(new DuplicateEntryRow(reader.GetString(0), title, reader.GetString(2), reader.GetInt64(3) != 0, reader.GetString(4), key));
            }
        }
        return rows.GroupBy(x => x.NormalizedTitle, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateEntryGroup(g.First().Title, g.Count(), g.Count(x => x.IsArchived),
                g.Select(x => x with { NormalizedTitle = string.Empty }).ToArray()))
            .OrderByDescending(x => x.Count).ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase).ToArray();
    }
}

public sealed record DuplicateEntryRow(string Id, string Title, string EntryType, bool IsArchived, string UpdatedAt, string NormalizedTitle);
public sealed record DuplicateEntryGroup(string Title, int Count, int ArchivedCount, IReadOnlyList<DuplicateEntryRow> Entries);
