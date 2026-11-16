using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

/// <summary>Creates and restores SQLite backups through same-directory staging files.
/// Existing destination files are never overwritten; incomplete staging files are cleaned up.</summary>
public sealed class DatabaseBackupService
{
    public object Backup(string sourcePath, string backupPath)
    {
        var source = Path.GetFullPath(sourcePath);
        var target = Path.GetFullPath(backupPath);
        if (!File.Exists(source)) throw new FileNotFoundException("找不到來源資料庫。", source);
        if (PathsEqual(source, target)) throw new ArgumentException("備份目的地不可與來源資料庫相同。");
        if (HasSqliteFiles(target)) throw new IOException("備份目的檔或 SQLite 暫存檔已存在；為避免覆蓋，請指定新的檔名。");

        var directory = Path.GetDirectoryName(target);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        var staging = CreateStagingPath(target);
        try
        {
            using (var src = Open(source, SqliteOpenMode.ReadOnly))
            using (var dst = Open(staging, SqliteOpenMode.ReadWriteCreate))
                src.BackupDatabase(dst);

            ValidateDatabase(staging);
            // Recheck the complete SQLite file set immediately before no-overwrite commit.
            if (HasSqliteFiles(target)) throw new IOException("備份目的檔或 SQLite 暫存檔已存在；未覆蓋任何檔案。");
            File.Move(staging, target);
            return new
            {
                Source = source,
                Backup = target,
                SizeBytes = new FileInfo(target).Length,
                CreatedAt = DateTimeOffset.UtcNow.ToString("O"),
                IntegrityCheck = "ok",
                AtomicCommit = true
            };
        }
        finally
        {
            CleanupSqliteFiles(staging);
        }
    }

    public object Restore(string backupPath, string targetPath)
    {
        var source = Path.GetFullPath(backupPath);
        var target = Path.GetFullPath(targetPath);
        if (!File.Exists(source)) throw new FileNotFoundException("找不到備份資料庫。", source);
        if (PathsEqual(source, target)) throw new ArgumentException("還原目的地不可與備份來源相同。");
        if (HasSqliteFiles(target)) throw new IOException("還原目的資料庫或 SQLite 暫存檔已存在；本命令不覆蓋既有資料，請選擇新路徑。");

        var directory = Path.GetDirectoryName(target);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        var staging = CreateStagingPath(target);
        try
        {
            using (var src = Open(source, SqliteOpenMode.ReadOnly))
            using (var dst = Open(staging, SqliteOpenMode.ReadWriteCreate))
                src.BackupDatabase(dst);

            ValidateDatabase(staging);
            if (HasSqliteFiles(target)) throw new IOException("還原目的資料庫或 SQLite 暫存檔已存在；未覆蓋任何檔案。");
            File.Move(staging, target);
            return new
            {
                Backup = source,
                RestoredDatabase = target,
                IntegrityCheck = "ok",
                SizeBytes = new FileInfo(target).Length,
                AtomicCommit = true
            };
        }
        finally
        {
            CleanupSqliteFiles(staging);
        }
    }

    private static void ValidateDatabase(string path)
    {
        using var connection = Open(path, SqliteOpenMode.ReadOnly);
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA integrity_check;";
            var result = Convert.ToString(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
            if (!string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"資料庫完整性檢查失敗：{result}");
        }
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA foreign_key_check;";
            using var reader = command.ExecuteReader();
            if (reader.Read())
                throw new InvalidDataException($"資料庫外鍵檢查失敗：資料表 {reader.GetString(0)} 存在關聯錯誤。");
        }
    }

    private static string CreateStagingPath(string target)
    {
        var directory = Path.GetDirectoryName(target) ?? Directory.GetCurrentDirectory();
        var name = Path.GetFileName(target);
        return Path.Combine(directory, $".{name}.{Guid.NewGuid():N}.staging");
    }

    private static bool HasSqliteFiles(string path) =>
        new[] { path, path + "-wal", path + "-shm", path + "-journal" }.Any(File.Exists);

    private static void CleanupSqliteFiles(string path)
    {
        foreach (var candidate in new[] { path, path + "-wal", path + "-shm", path + "-journal" })
        {
            try { if (File.Exists(candidate)) File.Delete(candidate); }
            catch { /* Best-effort cleanup; never mask the original backup/restore exception. */ }
        }
    }

    private static bool PathsEqual(string left, string right) =>
        string.Equals(left.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            right.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private static SqliteConnection Open(string path, SqliteOpenMode mode)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = path, Mode = mode, Pooling = false }.ToString());
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys=ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }
}
