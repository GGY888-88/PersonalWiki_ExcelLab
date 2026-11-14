using Microsoft.Data.Sqlite;
using System.Text;

namespace PersonalWiki.ExcelLab.Services;

/// <summary>標籤管理。所有破壞性操作均要求明確呼叫，且以交易執行。</summary>
public sealed class TagManagementService
{
    public sealed record TagSummary(string Tag, int EntryCount);

    public IReadOnlyList<TagSummary> List(string databasePath)
    {
        using var connection = Open(databasePath, readOnly: true);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT MIN(tag) AS tag, COUNT(DISTINCT entry_id) AS entry_count FROM entry_tags GROUP BY normalized_tag ORDER BY normalized_tag;";
        using var reader = command.ExecuteReader();
        var result = new List<TagSummary>();
        while (reader.Read()) result.Add(new TagSummary(reader.GetString(0), reader.GetInt32(1)));
        return result;
    }

    public int Rename(string databasePath, string oldTag, string newTag)
    {
        var oldNormalized = Normalize(oldTag);
        var cleanNew = Clean(newTag);
        var newNormalized = Normalize(cleanNew);
        if (oldNormalized == newNormalized) return 0;

        using var connection = Open(databasePath);
        using var transaction = connection.BeginTransaction();
        int changed;
        using (var count = connection.CreateCommand())
        {
            count.Transaction = transaction;
            count.CommandText = "SELECT COUNT(*) FROM entry_tags WHERE normalized_tag=$old;";
            count.Parameters.AddWithValue("$old", oldNormalized);
            changed = Convert.ToInt32(count.ExecuteScalar());
        }
        // INSERT OR IGNORE 會安全合併目標標籤已存在的條目，再移除舊標籤。
        using (var merge = connection.CreateCommand())
        {
            merge.Transaction = transaction;
            merge.CommandText = "INSERT OR IGNORE INTO entry_tags(entry_id, tag, normalized_tag) SELECT entry_id, $tag, $normalized FROM entry_tags WHERE normalized_tag=$old;";
            merge.Parameters.AddWithValue("$tag", cleanNew);
            merge.Parameters.AddWithValue("$normalized", newNormalized);
            merge.Parameters.AddWithValue("$old", oldNormalized);
            merge.ExecuteNonQuery();
        }
        using (var remove = connection.CreateCommand())
        {
            remove.Transaction = transaction;
            remove.CommandText = "DELETE FROM entry_tags WHERE normalized_tag=$old;";
            remove.Parameters.AddWithValue("$old", oldNormalized);
            remove.ExecuteNonQuery();
        }
        transaction.Commit();
        return changed;
    }

    public int Delete(string databasePath, string tag)
    {
        var normalized = Normalize(tag);
        using var connection = Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM entry_tags WHERE normalized_tag=$tag; SELECT changes();";
        command.Parameters.AddWithValue("$tag", normalized);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public IReadOnlyList<string> ExportCsv(string databasePath)
    {
        return List(databasePath).Select(x => $"\"{x.Tag.Replace("\"", "\"\"")}\",{x.EntryCount}").Prepend("標籤,使用條目數").ToArray();
    }

    private static string Clean(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("標籤不可空白。");
        var clean = value.Trim();
        if (clean.Length > 100) throw new ArgumentException("標籤不可超過 100 個字元。");
        return clean;
    }
    private static string Normalize(string value) => Clean(value).Normalize(NormalizationForm.FormKC).ToUpperInvariant();
    private static SqliteConnection Open(string path, bool readOnly = false)
    {
        var full = Path.GetFullPath(path);
        if (!File.Exists(full)) throw new FileNotFoundException("找不到知識庫資料庫。", full);
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = full, Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite, Pooling = false }.ToString());
        connection.Open();
        using var pragma = connection.CreateCommand(); pragma.CommandText = "PRAGMA foreign_keys=ON;"; pragma.ExecuteNonQuery();
        return connection;
    }
}
