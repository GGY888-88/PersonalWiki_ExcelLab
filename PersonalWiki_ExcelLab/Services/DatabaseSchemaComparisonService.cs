using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseSchemaObjectComparison(string Type, string Name, string? LeftSql, string? RightSql, bool IsEqual);
public sealed record DatabaseSchemaComparisonResult(string LeftDatabase, string RightDatabase, int LeftVersion,
    int RightVersion, IReadOnlyList<DatabaseSchemaObjectComparison> Objects, bool IsEqual, bool ReadOnly);

/// <summary>Compares SQLite schema definitions without changing either database.</summary>
public sealed class DatabaseSchemaComparisonService
{
    public DatabaseSchemaComparisonResult Compare(string leftPath, string rightPath)
    {
        var left = Path.GetFullPath(leftPath); var right = Path.GetFullPath(rightPath);
        if (!File.Exists(left)) throw new FileNotFoundException("找不到左側資料庫。", left);
        if (!File.Exists(right)) throw new FileNotFoundException("找不到右側資料庫。", right);
        using var a = Open(left); using var b = Open(right);
        var leftVersion = Version(a); var rightVersion = Version(b);
        var x = Objects(a); var y = Objects(b);
        var keys = x.Keys.Union(y.Keys).OrderBy(k => k.Type, StringComparer.Ordinal).ThenBy(k => k.Name, StringComparer.Ordinal).ToList();
        var differences = keys.Select(key =>
        {
            x.TryGetValue(key, out var l); y.TryGetValue(key, out var r);
            return new DatabaseSchemaObjectComparison(key.Type, key.Name, l, r,
                string.Equals(Normalize(l), Normalize(r), StringComparison.Ordinal));
        }).ToList();
        return new DatabaseSchemaComparisonResult(left, right, leftVersion, rightVersion, differences,
            leftVersion == rightVersion && differences.All(d => d.IsEqual), true);
    }

    private static SqliteConnection Open(string path)
    {
        var c = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        c.Open(); return c;
    }
    private static int Version(SqliteConnection c)
    { using var cmd = c.CreateCommand(); cmd.CommandText = "PRAGMA user_version;"; return Convert.ToInt32(cmd.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture); }
    private static Dictionary<(string Type, string Name), string?> Objects(SqliteConnection c)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT type,name,sql FROM sqlite_master WHERE type IN ('table','index','view','trigger') AND name NOT LIKE 'sqlite_%' ORDER BY type,name;";
        using var reader = cmd.ExecuteReader(); var result = new Dictionary<(string, string), string?>();
        while (reader.Read()) result[(reader.GetString(0), reader.GetString(1))] = reader.IsDBNull(2) ? null : reader.GetString(2);
        return result;
    }
    private static string? Normalize(string? sql) => sql is null ? null : string.Join(' ', sql.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
