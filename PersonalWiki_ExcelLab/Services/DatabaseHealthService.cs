using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseHealthResult(string Database, int SchemaVersion, IReadOnlyList<string> QuickCheck,
    bool QuickCheckPassed, int ForeignKeyIssueCount, IReadOnlyList<object> ForeignKeyIssues, bool IsHealthy, bool ReadOnly);

public sealed class DatabaseHealthService
{
    public DatabaseHealthResult Check(string databasePath)
    {
        var fullPath = Path.GetFullPath(databasePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("找不到知識庫。", fullPath);
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = fullPath, Mode = SqliteOpenMode.ReadOnly, Pooling = false
        }.ToString());
        connection.Open();

        var quickCheck = new List<string>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA quick_check;";
            using var reader = command.ExecuteReader();
            while (reader.Read()) quickCheck.Add(reader.GetString(0));
        }

        var foreignKeyIssues = new List<object>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA foreign_key_check;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
                foreignKeyIssues.Add(new
                {
                    Table = reader.GetString(0),
                    RowId = reader.IsDBNull(1) ? (long?)null : reader.GetInt64(1),
                    ParentTable = reader.GetString(2),
                    ForeignKeyIndex = reader.GetInt64(3)
                });
        }

        int schemaVersion;
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA user_version;";
            schemaVersion = Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
        }
        var quickCheckPassed = quickCheck.Count == 1 && quickCheck[0].Equals("ok", StringComparison.OrdinalIgnoreCase);
        return new DatabaseHealthResult(fullPath, schemaVersion, quickCheck, quickCheckPassed,
            foreignKeyIssues.Count, foreignKeyIssues, quickCheckPassed && foreignKeyIssues.Count == 0, true);
    }
}
