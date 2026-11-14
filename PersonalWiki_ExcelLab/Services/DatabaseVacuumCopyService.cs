using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseVacuumCopyResult(
    string SourceDatabase,
    string OptimizedCopy,
    long SourceSizeBytes,
    long OptimizedSizeBytes,
    long SavedBytes,
    int SchemaVersion,
    string IntegrityCheck,
    int ForeignKeyIssueCount,
    DateTimeOffset CreatedAtUtc);

/// <summary>Creates a compacted copy without changing the source database.</summary>
public sealed class DatabaseVacuumCopyService
{
    public DatabaseVacuumCopyResult Create(string sourcePath, string destinationPath)
    {
        var source = Path.GetFullPath(sourcePath);
        var destination = Path.GetFullPath(destinationPath);
        if (!File.Exists(source)) throw new FileNotFoundException("找不到來源資料庫。", source);
        if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("最佳化副本不可與來源資料庫相同。", nameof(destinationPath));
        if (File.Exists(destination)) throw new IOException("目的資料庫已存在；為避免覆蓋，請指定新檔名。");

        var directory = Path.GetDirectoryName(destination);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        var temporary = destination + ".vacuum-tmp-" + Guid.NewGuid().ToString("N");
        var sourceSize = new FileInfo(source).Length;
        var schemaVersion = 0;
        string integrity;
        var foreignKeyIssues = 0;

        try
        {
            using (var sourceConnection = Open(source, SqliteOpenMode.ReadOnly))
            using (var copyConnection = Open(temporary, SqliteOpenMode.ReadWriteCreate))
            {
                sourceConnection.BackupDatabase(copyConnection);
                schemaVersion = ReadInt(copyConnection, "PRAGMA user_version;");
            }

            using (var maintenanceConnection = Open(temporary, SqliteOpenMode.ReadWrite))
            {
                using (var vacuum = maintenanceConnection.CreateCommand())
                {
                    vacuum.CommandText = "VACUUM;";
                    vacuum.ExecuteNonQuery();
                }
                integrity = ReadText(maintenanceConnection, "PRAGMA integrity_check;");
                using var foreignKeys = maintenanceConnection.CreateCommand();
                foreignKeys.CommandText = "PRAGMA foreign_key_check;";
                using var reader = foreignKeys.ExecuteReader();
                while (reader.Read()) foreignKeyIssues++;
                var copiedSchemaVersion = ReadInt(maintenanceConnection, "PRAGMA user_version;");
                if (copiedSchemaVersion != schemaVersion)
                    throw new InvalidDataException("最佳化副本的 Schema 版本與來源不一致。");
            }

            if (!string.Equals(integrity, "ok", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"最佳化副本完整性檢查失敗：{integrity}");
            if (foreignKeyIssues != 0)
                throw new InvalidDataException($"最佳化副本有 {foreignKeyIssues} 筆外鍵問題，未交付副本。");
            if (File.Exists(destination)) throw new IOException("目的資料庫在處理期間已建立；未覆蓋該檔案。");

            File.Move(temporary, destination);
            var optimizedSize = new FileInfo(destination).Length;
            return new DatabaseVacuumCopyResult(source, destination, sourceSize, optimizedSize,
                sourceSize - optimizedSize, schemaVersion, integrity, foreignKeyIssues, DateTimeOffset.UtcNow);
        }
        catch
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            throw;
        }
    }

    private static SqliteConnection Open(string path, SqliteOpenMode mode)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path, Mode = mode, Pooling = false
        }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys=ON;";
        command.ExecuteNonQuery();
        return connection;
    }

    private static int ReadInt(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string ReadText(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToString(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
