using Microsoft.Data.Sqlite;
using System.Security.Cryptography;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseRecoveryResult(
    string BackupDatabase, string RestoredDatabase, bool BackupHealthy, bool RestoredHealthy,
    int BackupSchemaVersion, int RestoredSchemaVersion,
    IReadOnlyDictionary<string, long> BackupTableCounts,
    IReadOnlyDictionary<string, long> RestoredTableCounts,
    bool TableCountsMatch, bool IsVerified, string BackupSha256, string RestoredSha256);

/// <summary>Restores a backup to a new path and verifies logical table counts and database health.</summary>
public sealed class DatabaseRecoveryService
{
    public DatabaseRecoveryResult RestoreAndVerify(string backupPath, string targetPath)
    {
        var backup = Path.GetFullPath(backupPath);
        var target = Path.GetFullPath(targetPath);
        if (!File.Exists(backup)) throw new FileNotFoundException("找不到備份資料庫。", backup);
        if (new[] { target, target + "-wal", target + "-shm", target + "-journal" }.Any(File.Exists)) throw new IOException("還原目的資料庫或 SQLite 暫存檔已存在；為避免覆蓋，請指定新路徑。");

        var healthService = new DatabaseHealthService();
        var backupHealth = healthService.Check(backup);
        if (!backupHealth.IsHealthy)
            throw new InvalidDataException("備份資料庫健康檢查失敗，未執行還原。請先執行 db-check 查看詳細問題。");

        var backupCounts = GetTableCounts(backup);
        var targetCreatedByThisOperation = false;
        new DatabaseBackupService().Restore(backup, target);
        targetCreatedByThisOperation = true;

        try
        {
            var restoredHealth = healthService.Check(target);
            var restoredCounts = GetTableCounts(target);
            var countsMatch = DictionaryEqual(backupCounts, restoredCounts);
            var schemaMatch = backupHealth.SchemaVersion == restoredHealth.SchemaVersion;
            var verified = restoredHealth.IsHealthy && countsMatch && schemaMatch;
            if (!verified)
                throw new InvalidDataException("還原後驗證未通過：健康狀態、Schema 或資料表筆數與備份不一致；已清理由本次流程建立的還原副本。");
            return new DatabaseRecoveryResult(backup, target, backupHealth.IsHealthy, restoredHealth.IsHealthy,
                backupHealth.SchemaVersion, restoredHealth.SchemaVersion, backupCounts, restoredCounts,
                countsMatch, true, Sha256(backup), Sha256(target));
        }
        catch
        {
            // Never remove a path unless this invocation successfully committed that exact target.
            if (targetCreatedByThisOperation)
            {
                foreach (var candidate in new[] { target, target + "-wal", target + "-shm", target + "-journal" })
                {
                    try { if (File.Exists(candidate)) File.Delete(candidate); }
                    catch { /* Preserve the original verification exception. */ }
                }
            }
            throw;
        }
    }

    private static Dictionary<string, long> GetTableCounts(string path)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = path, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open();
        var tables = new List<string>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
            using var reader = command.ExecuteReader();
            while (reader.Read()) tables.Add(reader.GetString(0));
        }
        var result = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var table in tables)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {QuoteIdentifier(table)};";
            result[table] = Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
        }
        return result;
    }

    private static bool DictionaryEqual(IReadOnlyDictionary<string, long> left, IReadOnlyDictionary<string, long> right) =>
        left.Count == right.Count && left.All(pair => right.TryGetValue(pair.Key, out var value) && value == pair.Value);

    private static string QuoteIdentifier(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

    private static string Sha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
