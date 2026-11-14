using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record RelatedEntryItem(string Id, string Title, string EntryType, string? Summary,
    string UpdatedAt, int SharedTagCount, bool SameType, int Score, string SharedTags);

/// <summary>Read-only related-entry suggestions ranked by shared tags and matching entry type.</summary>
public sealed class RelatedEntryService
{
    public IReadOnlyList<RelatedEntryItem> Get(string databasePath, string entryId, int limit = 20)
    {
        if (limit is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(limit), "limit 必須介於 1 至 100。");
        var full = Path.GetFullPath(databasePath);
        if (!File.Exists(full)) throw new FileNotFoundException("找不到知識庫。", full);
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = full, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
SELECT e.id,e.title,e.entry_type,e.summary,e.updated_at,
       COUNT(DISTINCT shared.normalized_tag) AS shared_tag_count,
       CASE WHEN e.entry_type=source.entry_type THEN 1 ELSE 0 END AS same_type,
       COALESCE(GROUP_CONCAT(DISTINCT shared.tag), '') AS shared_tags
FROM entries source
JOIN entries e ON e.id<>source.id AND e.is_archived=0
LEFT JOIN entry_tags shared ON shared.entry_id=e.id
 AND shared.normalized_tag IN (SELECT normalized_tag FROM entry_tags WHERE entry_id=$entry)
WHERE source.id=$entry
GROUP BY e.id
HAVING shared_tag_count>0 OR same_type=1
ORDER BY (same_type*2 + shared_tag_count) DESC,e.updated_at DESC,e.title COLLATE NOCASE ASC
LIMIT $limit;
""";
        command.Parameters.AddWithValue("$entry", entryId);
        command.Parameters.AddWithValue("$limit", limit);
        var result = new List<RelatedEntryItem>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var shared = reader.GetInt32(5);
            var sameType = reader.GetInt64(6) != 0;
            result.Add(new RelatedEntryItem(reader.GetString(0), reader.GetString(1), reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), shared, sameType,
                shared + (sameType ? 2 : 0), reader.GetString(7)));
        }
        return result;
    }
}
