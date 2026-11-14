using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseSchemaSnapshotObject(string Type, string Name, string? Sql, string DefinitionSha256);
public sealed record DatabaseSchemaSnapshot(string Format, int SchemaVersion, int UserVersion,
    IReadOnlyList<DatabaseSchemaSnapshotObject> Objects, string SnapshotSha256, bool ReadOnly);
public sealed record DatabaseSchemaSnapshotValidationResult(string Database, string SnapshotFile,
    bool IsValid, int ExpectedObjects, int ActualObjects, IReadOnlyList<string> Differences, bool ReadOnly);

/// <summary>Creates and validates portable, read-only SQLite schema snapshots.</summary>
public sealed class DatabaseSchemaSnapshotService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseSchemaSnapshot Create(string databasePath, string outputPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(outputPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到資料庫。", database);
        if (File.Exists(output)) throw new IOException("Schema 快照檔已存在，為避免覆蓋已停止輸出。");
        using var connection = Open(database);
        var userVersion = ScalarInt(connection, "PRAGMA user_version;");
        var schemaVersion = ReadSchemaVersion(connection);
        var objects = ReadObjects(connection).Select(x => new DatabaseSchemaSnapshotObject(
            x.Type, x.Name, x.Sql, Hash(x.Sql ?? ""))).ToList();
        var canonical = Canonical(userVersion, schemaVersion, objects);
        var snapshot = new DatabaseSchemaSnapshot("personalwiki-schema-snapshot-v1", schemaVersion,
            userVersion, objects, Hash(canonical), true);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, snapshot, JsonOptions);
        return snapshot;
    }

    public DatabaseSchemaSnapshotValidationResult Validate(string databasePath, string snapshotPath)
    {
        var database = Path.GetFullPath(databasePath); var snapshotFile = Path.GetFullPath(snapshotPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到資料庫。", database);
        if (!File.Exists(snapshotFile)) throw new FileNotFoundException("找不到 Schema 快照。", snapshotFile);
        var expected = JsonSerializer.Deserialize<DatabaseSchemaSnapshot>(File.ReadAllText(snapshotFile))
            ?? throw new InvalidDataException("Schema 快照內容為空或格式錯誤。");
        if (expected.Format != "personalwiki-schema-snapshot-v1") throw new InvalidDataException("不支援的 Schema 快照格式。");
        var expectedHash = Hash(Canonical(expected.UserVersion, expected.SchemaVersion, expected.Objects));
        var differences = new List<string>();
        if (!string.Equals(expectedHash, expected.SnapshotSha256, StringComparison.OrdinalIgnoreCase)) differences.Add("快照自身 SHA-256 驗證失敗。");
        using var connection = Open(database);
        var actualUserVersion = ScalarInt(connection, "PRAGMA user_version;");
        var actualSchemaVersion = ReadSchemaVersion(connection);
        if (expected.UserVersion != actualUserVersion) differences.Add($"user_version 不同：預期 {expected.UserVersion}，實際 {actualUserVersion}。");
        if (expected.SchemaVersion != actualSchemaVersion) differences.Add($"Schema version 不同：預期 {expected.SchemaVersion}，實際 {actualSchemaVersion}。");
        var actual = ReadObjects(connection).Select(x => new DatabaseSchemaSnapshotObject(x.Type, x.Name, x.Sql, Hash(x.Sql ?? ""))).ToList();
        var expectedMap = expected.Objects.ToDictionary(x => (x.Type, x.Name));
        var actualMap = actual.ToDictionary(x => (x.Type, x.Name));
        foreach (var key in expectedMap.Keys.Union(actualMap.Keys).OrderBy(x => x.Type).ThenBy(x => x.Name))
        {
            expectedMap.TryGetValue(key, out var e); actualMap.TryGetValue(key, out var a);
            if (e is null) differences.Add($"新增資料庫物件：{key.Type}/{key.Name}");
            else if (a is null) differences.Add($"缺少資料庫物件：{key.Type}/{key.Name}");
            else if (!string.Equals(e.DefinitionSha256, a.DefinitionSha256, StringComparison.OrdinalIgnoreCase)) differences.Add($"物件定義不同：{key.Type}/{key.Name}");
        }
        return new DatabaseSchemaSnapshotValidationResult(database, snapshotFile, differences.Count == 0,
            expected.Objects.Count, actual.Count, differences, true);
    }

    private static SqliteConnection Open(string path)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open(); return connection;
    }
    private static int ScalarInt(SqliteConnection c, string sql)
    { using var cmd = c.CreateCommand(); cmd.CommandText = sql; return Convert.ToInt32(cmd.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture); }
    private static int ReadSchemaVersion(SqliteConnection c) => ScalarInt(c, "PRAGMA user_version;");
    private static List<(string Type, string Name, string? Sql)> ReadObjects(SqliteConnection c)
    {
        using var cmd = c.CreateCommand(); cmd.CommandText = "SELECT type,name,sql FROM sqlite_master WHERE type IN ('table','index','view','trigger') AND name NOT LIKE 'sqlite_%' ORDER BY type,name;";
        using var reader = cmd.ExecuteReader(); var result = new List<(string, string, string?)>();
        while (reader.Read()) result.Add((reader.GetString(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2)));
        return result;
    }
    private static string Canonical(int userVersion, int schemaVersion, IReadOnlyList<DatabaseSchemaSnapshotObject> objects)
        => JsonSerializer.Serialize(new { Format = "personalwiki-schema-snapshot-v1", SchemaVersion = schemaVersion, UserVersion = userVersion,
            Objects = objects.OrderBy(x => x.Type, StringComparer.Ordinal).ThenBy(x => x.Name, StringComparer.Ordinal).ToArray() });
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
