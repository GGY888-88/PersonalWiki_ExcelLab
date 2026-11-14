using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed class DatabaseTableInventoryService
{
    public IReadOnlyList<DatabaseTableInventory> GetCounts(string databasePath)
    {
        var path = Path.GetFullPath(databasePath);
        if (!File.Exists(path)) throw new FileNotFoundException("找不到知識庫。", path);
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = path, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
        using var reader = command.ExecuteReader();
        var names = new List<string>();
        while (reader.Read()) names.Add(reader.GetString(0));
        reader.Close();
        var result = new List<DatabaseTableInventory>();
        foreach (var name in names)
        {
            using var count = connection.CreateCommand();
            count.CommandText = $"SELECT COUNT(*) FROM \"{name.Replace("\"", "\"\"")}\";";
            result.Add(new DatabaseTableInventory(name, Convert.ToInt64(count.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture)));
        }
        return result;
    }
}
