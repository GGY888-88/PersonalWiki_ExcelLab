namespace PersonalWiki.ExcelLab.Services;

public sealed record ExcelImportWorkflowResult(bool PreflightPassed, bool BackupVerified, bool ImportCompleted, bool BatchVerified, string BackupDatabase, string BackupSha256, string BatchId, long ImportedRows, long ImportedFields, IReadOnlyList<string> Stages, IReadOnlyList<string> SafetyNotes);

/// <summary>Runs preflight, verified backup, transactional import, and post-import verification in order.</summary>
public sealed class ExcelImportWorkflowService
{
    public ExcelImportWorkflowResult Execute(string databasePath, string workbookPath, string mappingPath, string rulesPath, string backupPath)
    {
        var stages = new List<string>();
        var preflight = new ExcelImportPreflightService().Check(workbookPath, mappingPath, rulesPath);
        if (!preflight.CanImport) throw new InvalidDataException("Excel 匯入預檢未通過；未建立備份，也未寫入資料庫。原因：" + string.Join("；", preflight.BlockingReasons));
        stages.Add("Excel 欄位對應、清理規則及 Dry-run 預檢通過。");

        var safety = new ExcelImportDatabaseSafetyService().Prepare(databasePath, backupPath);
        if (!safety.ReadyForImport || !safety.BackupHealthy) throw new InvalidDataException("資料庫安全檢查或備份驗證未通過；未執行匯入。");
        stages.Add("目標資料庫健康檢查通過，且匯入前備份已建立並驗證。");

        var current = new ExcelImportPreflightService().Check(workbookPath, mappingPath, rulesPath);
        if (!current.CanImport || preflight.SourceSha256 != current.SourceSha256 || preflight.MappingSha256 != current.MappingSha256 || preflight.CleaningRulesSha256 != current.CleaningRulesSha256)
            throw new InvalidDataException("預檢後來源 Excel 或設定檔發生變動，已停止匯入；匯入前備份保留。");
        stages.Add("備份後再次預檢，確認 Excel 與設定檔雜湊一致。");

        // Import only from a private snapshot. This closes the remaining time-of-check /
        // time-of-use gap between the final hash check and the importer reading the files.
        using var snapshot = ExcelImportInputSnapshot.Create(
            workbookPath, mappingPath, rulesPath,
            current.SourceSha256, current.MappingSha256, current.CleaningRulesSha256);
        var snapshotPreflight = new ExcelImportPreflightService().Check(snapshot.WorkbookPath, snapshot.MappingPath, snapshot.RulesPath);
        if (!snapshotPreflight.CanImport
            || !string.Equals(current.SourceSha256, snapshotPreflight.SourceSha256, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(current.MappingSha256, snapshotPreflight.MappingSha256, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(current.CleaningRulesSha256, snapshotPreflight.CleaningRulesSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("匯入快照未通過一致性預檢，已停止匯入；匯入前備份保留。");
        stages.Add("已建立唯讀來源快照，並以快照再次完成預檢；正式匯入不再讀取可變動的原始檔案。");

        var imported = new ExcelDatabaseImportService().Import(databasePath, snapshot.WorkbookPath, snapshot.MappingPath, snapshot.RulesPath, workbookPath);
        stages.Add("Excel 資料已透過單一 SQLite 交易匯入；提交前已通過 quick_check(1) 與 foreign_key_check。");
        var verification = new ExcelImportBatchVerificationService().Verify(databasePath, imported.BatchId);
        if (!verification.IsVerified) throw new InvalidDataException("匯入交易已完成，但匯入後驗證未通過。批次 ID：" + imported.BatchId + "；問題：" + string.Join("；", verification.Issues) + "。匯入前備份保留，請勿重複匯入。");
        stages.Add("匯入批次、來源追溯、全文索引及外鍵驗證通過。");
        return new ExcelImportWorkflowResult(true, true, true, true, safety.BackupDatabase, safety.BackupSha256, imported.BatchId, imported.ImportedRows, imported.ImportedFields, stages, new[] { "來源 Excel、mapping 與 rules 以唯讀方式複製至唯一暫存快照，匯入只讀取快照，不會修改原始檔案。", "正式寫入前先建立並驗證不覆蓋既有檔案的資料庫備份。", "匯入採單一 SQLite 交易；匯入失敗時交易回滾，備份保留。", "圖片檔案不會複製或修改。", "若匯入後驗證失敗，資料已提交但不會自動刪除；請使用匯入前備份還原至新路徑並人工確認。" });
    }
}
