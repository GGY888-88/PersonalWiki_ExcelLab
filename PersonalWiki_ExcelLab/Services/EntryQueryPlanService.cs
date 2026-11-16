using Microsoft.Data.Sqlite;
using System.Globalization;

namespace PersonalWiki.ExcelLab.Services;

public sealed record EntryQueryPlanStep(int Id, int Parent, string Detail);
public sealed record EntryQueryPlanReport(string DatabasePath, string SqliteVersion, bool ReadOnly,
    string ArchivedMode, string Sort, string? Keyword, string? CreatedFrom, string? CreatedTo,
    string? UpdatedFrom, string? UpdatedTo, IReadOnlyList<EntryQueryPlanStep> CountPlan,
    IReadOnlyList<EntryQueryPlanStep> ResultPlan, IReadOnlyList<string> Notes);

/// <summary>Read-only SQLite query-plan inspection for the entry search predicates.</summary>
public sealed class EntryQueryPlanService
{
    private static readonly IReadOnlyDictionary<string, string> SortSql = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["updated-desc"] = "e.updated_at DESC,e.title COLLATE NOCASE ASC,e.id ASC",
        ["title-asc"] = "e.title COLLATE NOCASE ASC,e.id ASC",
        ["title-desc"] = "e.title COLLATE NOCASE DESC,e.id ASC",
        ["created-desc"] = "e.created_at DESC,e.title COLLATE NOCASE ASC,e.id ASC",
        ["created-asc"] = "e.created_at ASC,e.title COLLATE NOCASE ASC,e.id ASC"
    };

    public EntryQueryPlanReport Explain(string databasePath, string? entryType = null,
        string? tag = null, string archivedMode = "exclude", string sort = "updated-desc",
        string? keyword = null, string? createdFrom = null, string? createdTo = null,
        string? updatedFrom = null, string? updatedTo = null)
    {
        archivedMode = archivedMode.Trim().ToLowerInvariant();
        if (archivedMode is not ("exclude" or "include" or "only"))
            throw new ArgumentException("archived 必須是 exclude、include 或 only。");
        var normalizedSort = (sort ?? string.Empty).Trim();
        if (!SortSql.TryGetValue(normalizedSort, out var orderBy))
            throw new ArgumentException("不支援的搜尋排序方式。", nameof(sort));
        sort = normalizedSort.ToLowerInvariant();
        entryType = string.IsNullOrWhiteSpace(entryType) ? null : entryType.Trim();
        keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        createdFrom = NormalizeDate(createdFrom, nameof(createdFrom));
        createdTo = NormalizeDate(createdTo, nameof(createdTo));
        updatedFrom = NormalizeDate(updatedFrom, nameof(updatedFrom));
        updatedTo = NormalizeDate(updatedTo, nameof(updatedTo));
        ValidateRange(createdFrom, createdTo, "建立日期");
        ValidateRange(updatedFrom, updatedTo, "更新日期");
        tag = string.IsNullOrWhiteSpace(tag) ? null : tag.Trim().Normalize(System.Text.NormalizationForm.FormKC).ToUpperInvariant();

        var fullPath = Path.GetFullPath(databasePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("找不到知識庫。", fullPath);
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = fullPath, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open();

        var predicates = new List<string>();
        if (keyword is not null) predicates.Add("(e.id IN (SELECT entry_id FROM entries_fts WHERE entries_fts MATCH $keyword) OR e.title LIKE $like ESCAPE '\\' OR COALESCE(e.summary,'') LIKE $like ESCAPE '\\' OR EXISTS (SELECT 1 FROM entry_fields f WHERE f.entry_id=e.id AND (COALESCE(f.field_label,'') LIKE $like ESCAPE '\\' OR COALESCE(f.field_value,'') LIKE $like ESCAPE '\\')) OR EXISTS (SELECT 1 FROM entry_notes n WHERE n.entry_id=e.id AND n.note_text LIKE $like ESCAPE '\\') OR EXISTS (SELECT 1 FROM entry_tags t WHERE t.entry_id=e.id AND t.tag LIKE $like ESCAPE '\\') OR EXISTS (SELECT 1 FROM entry_images i WHERE i.entry_id=e.id AND (COALESCE(i.caption,'') LIKE $like ESCAPE '\\' OR COALESCE(i.original_name,'') LIKE $like ESCAPE '\\')))" );
        if (entryType is not null) predicates.Add("e.entry_type=$type");
        if (tag is not null) predicates.Add("EXISTS (SELECT 1 FROM entry_tags t WHERE t.entry_id=e.id AND t.normalized_tag=$tag)");
        if (archivedMode == "exclude") predicates.Add("e.is_archived=0");
        else if (archivedMode == "only") predicates.Add("e.is_archived=1");
        AddDatePredicate(predicates, "e.created_at", "$createdFrom", "$createdTo", createdFrom, createdTo);
        AddDatePredicate(predicates, "e.updated_at", "$updatedFrom", "$updatedTo", updatedFrom, updatedTo);
        var where = predicates.Count == 0 ? "" : " WHERE " + string.Join(" AND ", predicates);

        List<EntryQueryPlanStep> ReadPlan(string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "EXPLAIN QUERY PLAN " + sql;
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
            using var reader = command.ExecuteReader();
            var steps = new List<EntryQueryPlanStep>();
            while (reader.Read()) steps.Add(new EntryQueryPlanStep(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(3)));
            return steps;
        }

        var countPlan = ReadPlan("SELECT COUNT(*) FROM entries e" + where + ";");
        var resultPlan = ReadPlan("SELECT e.id,e.title,e.entry_type,e.summary,e.created_at,e.updated_at,e.is_archived FROM entries e" + where + " ORDER BY " + orderBy + " LIMIT 50 OFFSET 0;");
        var notes = new List<string>
        {
            "此報告只顯示 SQLite 查詢規劃器的預估步驟，不執行搜尋、不量測實際耗時，也不修改資料庫。",
            "SCAN 表示掃描；SEARCH 表示索引或條件查找；USE TEMP B-TREE 通常表示排序或分組需要暫存結構。",
            "關鍵字診斷會同時納入 FTS 與 LIKE/關聯資料表條件；實際使用路徑仍由 SQLite 規劃器決定。",
            "日期範圍使用起日含、迄日含（SQL 以次日零時作為排他上界），與 EntryQueryService 一致。"
        };
        return new EntryQueryPlanReport(fullPath, connection.ServerVersion, true, archivedMode, sort, keyword, createdFrom, createdTo, updatedFrom, updatedTo, countPlan, resultPlan, notes);
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

    private static void AddDatePredicate(List<string> predicates, string column, string fromParameter, string toParameter, string? from, string? to)
    {
        if (from is not null) predicates.Add($"{column} >= {fromParameter}");
        if (to is not null) predicates.Add($"{column} < {toParameter}");
    }

    private static string EndExclusive(string date)
    {
        var value = DateOnly.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return value == DateOnly.MaxValue ? "9999-12-32T00:00:00" : value.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "T00:00:00";
    }
}
