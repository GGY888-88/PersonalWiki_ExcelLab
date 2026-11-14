using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record WikiEntry(string Id, string Title, string EntryType, string? Summary, string CreatedAt, string UpdatedAt);

public sealed class EntryService
{
    public WikiEntry Create(string databasePath, string title, string entryType, string? summary)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("條目名稱不可空白。", nameof(title));
        if (string.IsNullOrWhiteSpace(entryType)) entryType = "General";
        var fullPath = Path.GetFullPath(databasePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("找不到知識庫，請先執行 db-init。", fullPath);
        using var connection = Open(fullPath);
        var now = DateTimeOffset.UtcNow.ToString("O");
        var id = Guid.NewGuid().ToString("N");
        var normalized = title.Trim().Normalize(System.Text.NormalizationForm.FormKC).ToUpperInvariant();
        using var transaction = connection.BeginTransaction();
        using (var insert = connection.CreateCommand())
        {
            insert.Transaction = transaction;
            insert.CommandText = "INSERT INTO entries(id,title,normalized_title,entry_type,summary,created_at,updated_at) VALUES($id,$title,$normalized,$type,$summary,$now,$now);";
            insert.Parameters.AddWithValue("$id", id);
            insert.Parameters.AddWithValue("$title", title.Trim());
            insert.Parameters.AddWithValue("$normalized", normalized);
            insert.Parameters.AddWithValue("$type", entryType.Trim());
            insert.Parameters.AddWithValue("$summary", (object?)summary ?? DBNull.Value);
            insert.Parameters.AddWithValue("$now", now);
            insert.ExecuteNonQuery();
        }
        using (var fts = connection.CreateCommand())
        {
            fts.Transaction = transaction;
            fts.CommandText = "INSERT INTO entries_fts(entry_id,title,summary) VALUES($id,$title,$summary);";
            fts.Parameters.AddWithValue("$id", id);
            fts.Parameters.AddWithValue("$title", title.Trim());
            fts.Parameters.AddWithValue("$summary", summary ?? string.Empty);
            fts.ExecuteNonQuery();
        }
        transaction.Commit();
        return new WikiEntry(id, title.Trim(), entryType.Trim(), summary, now, now);
    }

    public IReadOnlyList<WikiEntry> Search(string databasePath, string keyword, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return Array.Empty<WikiEntry>();
        if (limit is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(limit), "limit 必須介於 1 至 500。");
        var fullPath = Path.GetFullPath(databasePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("找不到知識庫。", fullPath);
        using var connection = Open(fullPath, readOnly: true);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT e.id,e.title,e.entry_type,e.summary,e.created_at,e.updated_at FROM entries_fts f JOIN entries e ON e.id=f.entry_id WHERE entries_fts MATCH $query ORDER BY e.updated_at DESC LIMIT $limit;";
        command.Parameters.AddWithValue("$query", "\"" + keyword.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"");
        command.Parameters.AddWithValue("$limit", limit);
        var rows = new List<WikiEntry>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            rows.Add(new WikiEntry(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.GetString(5)));
        return rows;
    }

    private static SqliteConnection Open(string path, bool readOnly = false)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite,
            Pooling = false
        }.ToString());
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys=ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }
}
