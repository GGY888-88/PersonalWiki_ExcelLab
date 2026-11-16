
namespace PersonalWiki.ExcelLab.Services;

public sealed record ExcelImportDatabaseSafetyResult(
    string SourceDatabase,
    string BackupDatabase,
    int ExpectedSchemaVersion,
    int ActualSchemaVersion,
    bool SourceHealthy,
    bool BackupHealthy,
    string BackupSha256,
    long BackupSizeBytes,
    bool ReadyForImport,
    IReadOnlyList<string> Checks,
    IReadOnlyList<string> SafetyNotes);

/// <summary>Checks the destination database and creates a verified, non-overwriting backup before Excel import.</summary>
public sealed class ExcelImportDatabaseSafetyService
{
    public ExcelImportDatabaseSafetyResult Prepare(string databasePath, string backupPath)
    {
        var source = Path.GetFullPath(databasePath);
        var backup = Path.GetFullPath(backupPath);
        if (!File.Exists(source)) throw new FileNotFoundException("找不到目標知識庫。", source);
        if (string.Equals(source, backup, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("備份路徑不可與目標資料庫相同。");
        if (File.Exists(backup)) throw new IOException("備份目的檔已存在；為避免覆蓋，請指定新檔名。");

        var sourceHealth = new DatabaseHealthService().Check(source);
        if (!sourceHealth.IsHealthy)
            throw new InvalidDataException("目標資料庫完整性或外鍵檢查未通過；已停止備份及匯入準備。請先修復或還原資料庫。");
        if (sourceHealth.SchemaVersion != WikiDatabaseService.CurrentSchemaVersion)
            throw new InvalidDataException($"資料庫 Schema 不相容：目前 {sourceHealth.SchemaVersion}，要求 {WikiDatabaseService.CurrentSchemaVersion}。");

        var directory = Path.GetDirectoryName(backup);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        new DatabaseBackupService().Backup(source, backup);

        try
        {
            var backupHealth = new DatabaseHealthService().Check(backup);
            if (!backupHealth.IsHealthy || backupHealth.SchemaVersion != sourceHealth.SchemaVersion)
                throw new InvalidDataException("備份檔驗證失敗；無法確認備份完整性或 Schema 一致。");
            var hash = ExcelImportPreflightService.HashFile(backup);
            return new ExcelImportDatabaseSafetyResult(source, backup, WikiDatabaseService.CurrentSchemaVersion,
                sourceHealth.SchemaVersion, true, true, hash, new FileInfo(backup).Length, true,
                new[] { "來源資料庫 quick_check 通過。", "來源資料庫 foreign_key_check 無錯誤。", "來源 Schema 版本相容。", "已建立 SQLite 一致性備份。", "備份檔 quick_check 與 foreign_key_check 通過。", "備份 Schema 與來源一致。" },
                new[] { "本命令不執行 Excel 匯入。", "不修改目標資料庫、來源 Excel 或圖片。", "備份檔使用新路徑建立，不覆蓋既有檔案。", "正式匯入仍須另行執行 excel-import-db。" });
        }
        catch
        {
            if (File.Exists(backup)) File.Delete(backup);
            throw;
        }
    }
}
