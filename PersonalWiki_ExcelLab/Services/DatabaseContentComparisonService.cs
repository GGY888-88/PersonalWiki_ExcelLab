using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseTableComparison(string Table, long LeftRows, long RightRows,
    string LeftSha256, string RightSha256, bool IsEqual);
public sealed record DatabaseContentComparisonResult(string LeftDatabase, string RightDatabase,
    int LeftSchemaVersion, int RightSchemaVersion, IReadOnlyList<DatabaseTableComparison> Tables,
    bool SchemaMatches, bool IsEqual);

/// <summary>Read-only, deterministic logical comparison of user tables across two SQLite databases.</summary>
public sealed class DatabaseContentComparisonService
{
    public DatabaseContentComparisonResult Compare(string leftPath, string rightPath)
    {
        var left = Path.GetFullPath(leftPath);
        var right = Path.GetFullPath(rightPath);
        if (!File.Exists(left)) throw new FileNotFoundException("找不到左側資料庫。", left);
        if (!File.Exists(right)) throw new FileNotFoundException("找不到右側資料庫。", right);
        using var a = Open(left);
        using var b = Open(right);
        var leftVersion = GetSchemaVersion(a);
        var rightVersion = GetSchemaVersion(b);
        var leftTables = GetTables(a);
        var rightTables = GetTables(b);
        var names = leftTables.Union(rightTables, StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
        var comparisons = new List<DatabaseTableComparison>();
        foreach (var table in names)
        {
            if (!leftTables.Contains(table, StringComparer.Ordinal) || !rightTables.Contains(table, StringComparer.Ordinal))
            {
                comparisons.Add(new DatabaseTableComparison(table, -1, -1, "", "", false));
                continue;
            }
            var leftResult = HashTable(a, table);
            var rightResult = HashTable(b, table);
            comparisons.Add(new DatabaseTableComparison(table, leftResult.Rows, rightResult.Rows,
                leftResult.Hash, rightResult.Hash, leftResult.Rows == rightResult.Rows && leftResult.Hash == rightResult.Hash));
        }
        var schemaMatches = leftVersion == rightVersion && leftTables.SequenceEqual(rightTables, StringComparer.Ordinal);
        return new DatabaseContentComparisonResult(left, right, leftVersion, rightVersion, comparisons,
            schemaMatches, schemaMatches && comparisons.All(x => x.IsEqual));
    }

    private static SqliteConnection Open(string path)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = path, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open();
        return connection;
    }

    private static int GetSchemaVersion(SqliteConnection connection)
    {
        using var command = connection.CreateCommand(); command.CommandText = "PRAGMA user_version;";
        return Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static List<string> GetTables(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
        using var reader = command.ExecuteReader(); var names = new List<string>();
        while (reader.Read()) names.Add(reader.GetString(0));
        return names;
    }

    private static (long Rows, string Hash) HashTable(SqliteConnection connection, string table)
    {
        var columns = new List<string>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"PRAGMA table_info({Quote(table)});";
            using var reader = command.ExecuteReader(); while (reader.Read()) columns.Add(reader.GetString(1));
        }
        if (columns.Count == 0) return (0, "");
        var sql = $"SELECT {string.Join(",", columns.Select(Quote))} FROM {Quote(table)} ORDER BY {string.Join(",", columns.Select(Quote))};";
        using var query = connection.CreateCommand(); query.CommandText = sql;
        using var rows = query.ExecuteReader();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        long count = 0;
        while (rows.Read())
        {
            for (var i = 0; i < columns.Count; i++)
            {
                var type = rows.GetFieldType(i).FullName ?? "";
                var value = rows.IsDBNull(i) ? "<NULL>" : Convert.ToString(rows.GetValue(i), System.Globalization.CultureInfo.InvariantCulture) ?? "";
                var bytes = Encoding.UTF8.GetBytes(type + ":" + value.Length + ":" + value + "\n");
                hash.AppendData(bytes);
            }
            hash.AppendData(new byte[] { 0xff }); count++;
        }
        return (count, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
    }

    private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
}
