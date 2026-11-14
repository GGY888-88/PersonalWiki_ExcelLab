using Microsoft.Data.Sqlite;
using System.Globalization;

namespace PersonalWiki.ExcelLab.Services;

public sealed record EntryQueryItem(string Id, string Title, string EntryType, string? Summary,
    string CreatedAt, string UpdatedAt, bool IsArchived, IReadOnlyList<string> Tags);

public sealed record EntryQueryResult(string? Keyword, string? EntryType, string? Tag,
    string ArchivedMode, int Page, int PageSize, int TotalCount, int TotalPages,
    IReadOnlyList<EntryQueryItem> Items, string Sort = "updated-desc",
    string? CreatedFrom = null, string? CreatedTo = null,
    string? UpdatedFrom = null, string? UpdatedTo = null);

/// <summary>Read-only, parameterized, bounded and paged knowledge-entry search.</summary>
public sealed class EntryQueryService
{
    private static readonly IReadOnlyDictionary<string, string> SortSql = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["updated-desc"] = "e.updated_at DESC,e.title COLLATE NOCASE ASC,e.id ASC",
        ["title-asc"] = "e.title COLLATE NOCASE ASC,e.id ASC",
        ["title-desc"] = "e.title COLLATE NOCASE DESC,e.id ASC",
        ["created-desc"] = "e.created_at DESC,e.title COLLATE NOCASE ASC,e.id ASC",
        ["created-asc"] = "e.created_at ASC,e.title COLLATE NOCASE ASC,e.id ASC"
    };

    public EntryQueryResult Query(string databasePath, string? keyword = null, string? entryType = null,
        string? tag = null, string archivedMode = "exclude", int page = 1, int pageSize = 50,
        string sort = "updated-desc", string? createdFrom = null, string? createdTo = null,
        string? updatedFrom = null, string? updatedTo = null)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "page 必須大於 0。");
        if (pageSize is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(pageSize), "page-size 必須介於 1 至 200。");
        archivedMode = archivedMode.Trim().ToLowerInvariant();
        if (archivedMode is not ("exclude" or "include" or "only"))
            throw new ArgumentException("archived 必須是 exclude、include 或 only。");
        var normalizedSort = (sort ?? string.Empty).Trim();
        if (!SortSql.TryGetValue(normalizedSort, out var orderBy))
            throw new ArgumentException("sort 必須是 updated-desc、title-asc、title-desc、created-desc 或 created-asc。", nameof(sort));
        sort = normalizedSort.ToLowerInvariant();
        createdFrom = NormalizeDate(createdFrom, nameof(createdFrom));
        createdTo = NormalizeDate(createdTo, nameof(createdTo));
        updatedFrom = NormalizeDate(updatedFrom, nameof(updatedFrom));
        updatedTo = NormalizeDate(updatedTo, nameof(updatedTo));
        ValidateRange(createdFrom, createdTo, "建立日期");
        ValidateRange(updatedFrom, updatedTo, "更新日期");

        keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        entryType = string.IsNullOrWhiteSpace(entryType) ? null : entryType.Trim();
        tag = string.IsNullOrWhiteSpace(tag) ? null : tag.Trim().Normalize(System.Text.NormalizationForm.FormKC).ToUpperInvariant();

        var fullPath = Path.GetFullPath(databasePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("找不到知識庫，請先執行 db-init。", fullPath);
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = fullPath, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open();

        var predicates = new List<string>();
        if (keyword is not null) predicates.Add("(e.id IN (SELECT entry_id FROM entries_fts WHERE entries_fts MATCH $keyword) OR e.title LIKE $like ESCAPE '\\' OR COALESCE(e.summary,'') LIKE $like ESCAPE '\\' OR EXISTS (SELECT 1 FROM entry_fields f WHERE f.entry_id=e.id AND (COALESCE(f.field_label,'') LIKE $like ESCAPE '\\' OR COALESCE(f.field_value,'') LIKE $like ESCAPE '\\')) OR EXISTS (SELECT 1 FROM entry_notes n WHERE n.entry_id=e.id AND n.note_text LIKE $like ESCAPE '\\') OR EXISTS (SELECT 1 FROM entry_tags t WHERE t.entry_id=e.id AND t.tag LIKE $like ESCAPE '\\') OR EXISTS (SELECT 1 FROM entry_images i WHERE i.entry_id=e.id AND (COALESCE(i.caption,'') LIKE $like ESCAPE '\\' OR COALESCE(i.original_name,'') LIKE $like ESCAPE '\\')))");
        if (entryType is not null) predicates.Add("e.entry_type = $type");
        if (tag is not null) predicates.Add("EXISTS (SELECT 1 FROM entry_tags t WHERE t.entry_id=e.id AND t.normalized_tag=$tag)");
        if (archivedMode == "exclude") predicates.Add("e.is_archived=0");
        else if (archivedMode == "only") predicates.Add("e.is_archived=1");
        AddDatePredicate(predicates, "e.created_at", "$createdFrom", "$createdTo", createdFrom, createdTo);
        AddDatePredicate(predicates, "e.updated_at", "$updatedFrom", "$updatedTo", updatedFrom, updatedTo);
        var where = predicates.Count == 0 ? string.Empty : " WHERE " + string.Join(" AND ", predicates);

        using var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM entries e" + where + ";";
        AddFilters(count, keyword, entryType, tag, createdFrom, createdTo, updatedFrom, updatedTo);
        var total = Convert.ToInt32(count.ExecuteScalar(), CultureInfo.InvariantCulture);
        var offset = checked((page - 1) * pageSize);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT e.id,e.title,e.entry_type,e.summary,e.created_at,e.updated_at,e.is_archived FROM entries e" + where + " ORDER BY " + orderBy + " LIMIT $limit OFFSET $offset;";
        AddFilters(command, keyword, entryType, tag, createdFrom, createdTo, updatedFrom, updatedTo);
        command.Parameters.AddWithValue("$limit", pageSize);
        command.Parameters.AddWithValue("$offset", offset);
        var items = new List<EntryQueryItem>();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
                items.Add(new EntryQueryItem(reader.GetString(0), reader.GetString(1), reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.GetString(5),
                    reader.GetInt64(6) != 0, Array.Empty<string>()));
        }

        // Fetch tags for the current page in one query, never one query per result row.
        if (items.Count > 0)
        {
            using var tagsCommand = connection.CreateCommand();
            var placeholders = new List<string>();
            for (var i = 0; i < items.Count; i++)
            {
                var name = "$id" + i;
                placeholders.Add(name);
                tagsCommand.Parameters.AddWithValue(name, items[i].Id);
            }
            tagsCommand.CommandText = $"SELECT entry_id,tag FROM entry_tags WHERE entry_id IN ({string.Join(",", placeholders)}) ORDER BY normalized_tag;";
            var tagsByEntry = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            using var tagReader = tagsCommand.ExecuteReader();
            while (tagReader.Read())
            {
                var id = tagReader.GetString(0);
                if (!tagsByEntry.TryGetValue(id, out var list)) tagsByEntry[id] = list = new List<string>();
                list.Add(tagReader.GetString(1));
            }
            for (var i = 0; i < items.Count; i++)
                items[i] = items[i] with { Tags = (IReadOnlyList<string>?)tagsByEntry.GetValueOrDefault(items[i].Id) ?? Array.Empty<string>() };
        }

        return new EntryQueryResult(keyword, entryType, tag, archivedMode, page, pageSize, total,
            total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize), items,
            sort, createdFrom, createdTo, updatedFrom, updatedTo);
    }

    private static string? NormalizeDate(string? value, string parameter)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (!DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            throw new ArgumentException($"{parameter} 日期格式必須是 yyyy-MM-dd。", parameter);
        return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static void ValidateRange(string? from, string? to, string label)
    {
        if (from is not null && to is not null && string.CompareOrdinal(from, to) > 0)
            throw new ArgumentException($"{label}起日不可晚於迄日。");
    }

    private static void AddDatePredicate(List<string> predicates, string column, string fromParameter,
        string toParameter, string? from, string? to)
    {
        if (from is not null) predicates.Add($"{column} >= {fromParameter}");
        if (to is not null) predicates.Add($"{column} < {toParameter}");
    }

    private static string EndExclusive(string date)
    {
        var value = DateOnly.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return value == DateOnly.MaxValue ? "9999-12-32T00:00:00" : value.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "T00:00:00";
    }

    private static void AddFilters(SqliteCommand command, string? keyword, string? entryType, string? tag,
        string? createdFrom, string? createdTo, string? updatedFrom, string? updatedTo)
    {
        if (keyword is not null)
        {
            command.Parameters.AddWithValue("$keyword", "\"" + keyword.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"");
            var escapedLike = keyword.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal);
            command.Parameters.AddWithValue("$like", "%" + escapedLike + "%");
        }
        if (entryType is not null) command.Parameters.AddWithValue("$type", entryType);
        if (tag is not null) command.Parameters.AddWithValue("$tag", tag);
        if (createdFrom is not null) command.Parameters.AddWithValue("$createdFrom", createdFrom + "T00:00:00");
        if (createdTo is not null) command.Parameters.AddWithValue("$createdTo", EndExclusive(createdTo));
        if (updatedFrom is not null) command.Parameters.AddWithValue("$updatedFrom", updatedFrom + "T00:00:00");
        if (updatedTo is not null) command.Parameters.AddWithValue("$updatedTo", EndExclusive(updatedTo));
    }
}
