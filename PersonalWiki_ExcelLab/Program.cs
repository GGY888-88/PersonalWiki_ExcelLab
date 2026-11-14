using System.Text.Json;
using PersonalWiki.ExcelLab.Services;

namespace PersonalWiki.ExcelLab;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
            {
                PrintHelp();
                return 0;
            }

            switch (args[0].ToLowerInvariant())
            {
                case "db-batch-source-reconciliation":
                    RequireCount(args, 3);
                    var batchSource = new DatabaseBatchSourceReconciliationService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(batchSource, JsonOptions));
                    return 0;
                case "db-image-file-integrity-audit":
                    RequireCount(args, 3);
                    var imageIntegrity = new DatabaseImageFileIntegrityAuditService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(imageIntegrity, JsonOptions));
                    return 0;
                case "db-relationship-audit":
                    RequireCount(args, 3);
                    var relationshipAudit = new DatabaseRelationshipAuditService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(relationshipAudit, JsonOptions));
                    return 0;
                case "db-source-trace-audit":
                    RequireCount(args, 3);
                    var sourceTraceAudit = new DatabaseSourceTraceAuditService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(sourceTraceAudit, JsonOptions));
                    return 0;
                case "db-note-usage-report":
                    RequireCount(args, 3);
                    var noteUsage = new DatabaseNoteUsageReportService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(noteUsage, JsonOptions));
                    return 0;
                case "db-field-usage-report":
                    RequireCount(args, 3);
                    var fieldUsage = new DatabaseFieldUsageReportService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(fieldUsage, JsonOptions));
                    return 0;
                case "db-tag-usage-report":
                    RequireCount(args, 3);
                    var tagUsage = new DatabaseTagUsageReportService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(tagUsage, JsonOptions));
                    return 0;
                case "db-entry-inventory-report":
                    RequireCount(args, 3);
                    var entryInventory = new DatabaseEntryInventoryReportService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(entryInventory, JsonOptions));
                    return 0;
                case "db-import-batch-audit":
                    RequireCount(args, 3);
                    var batchAudit = new DatabaseImportBatchAuditService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(batchAudit, JsonOptions));
                    return 0;
                case "db-tag-audit":
                    RequireCount(args, 3);
                    var tagAudit = new DatabaseTagAuditService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(tagAudit, JsonOptions));
                    return 0;
                case "db-field-audit":
                    RequireCount(args, 3);
                    var fieldAudit = new DatabaseFieldAuditService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(fieldAudit, JsonOptions));
                    return 0;
                case "db-note-audit":
                    RequireCount(args, 3);
                    var noteAudit = new DatabaseNoteAuditService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(noteAudit, JsonOptions));
                    return 0;
                case "db-image-inventory-report":
                    RequireCount(args, 3);
                    var imageInventoryReport = new DatabaseImageInventoryReportService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(imageInventoryReport, JsonOptions));
                    return 0;
                case "db-image-duplicate-report":
                    RequireCount(args, 3);
                    var imageDuplicateReport = new DatabaseImageDuplicateReportService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(imageDuplicateReport, JsonOptions));
                    return 0;
                case "db-vacuum-copy":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new DatabaseVacuumCopyService().Create(args[1], args[2]), JsonOptions));
                    return 0;
                case "db-storage-report":
                    RequireCount(args, 3);
                    var storageReport = new DatabaseStorageReportService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(storageReport, JsonOptions));
                    return 0;
                case "db-diagnostic-report":
                    RequireCount(args, 3);
                    var diagnosticReport = new DatabaseDiagnosticReportService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(diagnosticReport, JsonOptions));
                    return diagnosticReport.IsHealthy ? 0 : 3;
                case "db-diagnostic-compare":
                    RequireCount(args, 4);
                    var diagnosticComparison = new DatabaseDiagnosticCompareService().Create(args[1], args[2], args[3]);
                    Console.WriteLine(JsonSerializer.Serialize(diagnosticComparison, JsonOptions));
                    return 0;
                case "db-entry-audit":
                    RequireCount(args, 3);
                    var entryAudit = new DatabaseEntryAuditService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(entryAudit, JsonOptions));
                    return 0;
                case "db-init":
                    RequireCount(args, 2);
                    Console.WriteLine(JsonSerializer.Serialize(new WikiDatabaseService().Initialize(args[1]), JsonOptions));
                    return 0;

                case "db-index-check":
                    RequireCount(args, 2);
                    var indexCheck = new SearchIndexMaintenanceService().Check(args[1]);
                    Console.WriteLine(JsonSerializer.Serialize(indexCheck, JsonOptions));
                    return indexCheck.IsConsistent ? 0 : 3;
                case "db-index-rebuild":
                    RequireCount(args, 2);
                    Console.WriteLine(JsonSerializer.Serialize(new SearchIndexMaintenanceService().Rebuild(args[1]), JsonOptions));
                    return 0;
                case "db-info":
                    RequireCount(args, 2);
                    Console.WriteLine(JsonSerializer.Serialize(new WikiDatabaseService().Inspect(args[1]), JsonOptions));
                    return 0;
                case "db-check":
                    RequireCount(args, 2);
                    var health = new DatabaseHealthService().Check(args[1]);
                    Console.WriteLine(JsonSerializer.Serialize(health, JsonOptions));
                    return health.IsHealthy ? 0 : 3;
                case "excel-import-db-safety":
                    RequireCount(args, 3);
                    var importSafety = new ExcelImportDatabaseSafetyService().Prepare(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(importSafety, JsonOptions));
                    return importSafety.ReadyForImport ? 0 : 3;
                case "excel-import-db-safety-check":
                    RequireCount(args, 2);
                    var safetyValidation = new ExcelImportDatabaseSafetyValidationService().Validate(args[1]);
                    Console.WriteLine(JsonSerializer.Serialize(safetyValidation, JsonOptions));
                    return safetyValidation.IsValid ? 0 : 3;
                case "excel-import-db-safety-json":
                    RequireCount(args, 4);
                    var importSafetyJson = new ExcelImportDatabaseSafetyService().Prepare(args[1], args[2]);
                    var safetyReportPath = Path.GetFullPath(args[3]);
                    if (File.Exists(safetyReportPath)) throw new IOException("安全檢查報告已存在，為避免覆蓋已停止輸出。");
                    var safetyReportDirectory = Path.GetDirectoryName(safetyReportPath);
                    if (!string.IsNullOrWhiteSpace(safetyReportDirectory)) Directory.CreateDirectory(safetyReportDirectory);
                    using (var safetyReportStream = new FileStream(safetyReportPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        JsonSerializer.Serialize(safetyReportStream, importSafetyJson, JsonOptions);
                    Console.WriteLine($"匯入前資料庫安全檢查 JSON 報告已輸出：{safetyReportPath}");
                    return importSafetyJson.ReadyForImport ? 0 : 3;
                case "db-backup":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new DatabaseBackupService().Backup(args[1], args[2]), JsonOptions));
                    return 0;
                case "db-restore":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new DatabaseBackupService().Restore(args[1], args[2]), JsonOptions));
                    return 0;
                case "db-recovery-restore-verify":
                    RequireCount(args, 3);
                    var recovery = new DatabaseRecoveryService().RestoreAndVerify(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(recovery, JsonOptions));
                    return recovery.IsVerified ? 0 : 3;
                case "db-schema-snapshot":
                    RequireCount(args, 3);
                    var schemaSnapshot = new DatabaseSchemaSnapshotService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(schemaSnapshot, JsonOptions));
                    return 0;
                case "db-schema-snapshot-check-json":
                    RequireCount(args, 4);
                    var schemaSnapshotReportPath = Path.GetFullPath(args[3]);
                    if (File.Exists(schemaSnapshotReportPath)) throw new IOException("Schema 快照驗證報告已存在，為避免覆蓋已停止輸出。");
                    var schemaSnapshotReportDirectory = Path.GetDirectoryName(schemaSnapshotReportPath);
                    if (!string.IsNullOrWhiteSpace(schemaSnapshotReportDirectory)) Directory.CreateDirectory(schemaSnapshotReportDirectory);
                    object schemaSnapshotReport;
                    var schemaSnapshotExitCode = 0;
                    try
                    {
                        var validation = new DatabaseSchemaSnapshotService().Validate(args[1], args[2]);
                        schemaSnapshotReport = new { Success = validation.IsValid, Result = validation, Error = (string?)null };
                        schemaSnapshotExitCode = validation.IsValid ? 0 : 3;
                    }
                    catch (Exception ex)
                    {
                        schemaSnapshotReport = new { Success = false, Result = (object?)null, Error = ex.Message, ErrorType = ex.GetType().FullName };
                        schemaSnapshotExitCode = 3;
                    }
                    using (var schemaSnapshotReportStream = new FileStream(schemaSnapshotReportPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        JsonSerializer.Serialize(schemaSnapshotReportStream, schemaSnapshotReport, JsonOptions);
                    Console.WriteLine($"Schema 快照驗證 JSON 報告已輸出：{schemaSnapshotReportPath}");
                    Console.WriteLine(JsonSerializer.Serialize(schemaSnapshotReport, JsonOptions));
                    return schemaSnapshotExitCode;
                case "db-schema-snapshot-check":
                    RequireCount(args, 3);
                    var schemaSnapshotCheck = new DatabaseSchemaSnapshotService().Validate(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(schemaSnapshotCheck, JsonOptions));
                    return schemaSnapshotCheck.IsValid ? 0 : 3;
                case "db-schema-compare":
                    RequireCount(args, 3);
                    var schemaComparison = new DatabaseSchemaComparisonService().Compare(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(schemaComparison, JsonOptions));
                    return schemaComparison.IsEqual ? 0 : 3;
                case "db-integrity-report-check":
                    RequireCount(args, 3);
                    var integrityReportCheck = new DatabaseIntegrityReportValidationService().Validate(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(integrityReportCheck, JsonOptions));
                    return integrityReportCheck.IsValid ? 0 : 3;
                case "db-integrity-report":
                    RequireCount(args, 3);
                    var integrityReport = new DatabaseIntegrityReportService().Create(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(integrityReport, JsonOptions));
                    return integrityReport.IsHealthy ? 0 : 3;
                case "db-content-compare":
                    RequireCount(args, 3);
                    var contentComparison = new DatabaseContentComparisonService().Compare(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(contentComparison, JsonOptions));
                    return contentComparison.IsEqual ? 0 : 3;
                case "db-recovery-restore-verify-json":
                    RequireCount(args, 4);
                    var recoveryReportPath = Path.GetFullPath(args[3]);
                    if (File.Exists(recoveryReportPath)) throw new IOException("復原驗證報告已存在；為避免覆蓋已停止執行。");
                    var recoveryReportDirectory = Path.GetDirectoryName(recoveryReportPath);
                    if (!string.IsNullOrWhiteSpace(recoveryReportDirectory)) Directory.CreateDirectory(recoveryReportDirectory);
                    object recoveryReport;
                    var recoveryExitCode = 0;
                    try
                    {
                        var recoveryJsonResult = new DatabaseRecoveryService().RestoreAndVerify(args[1], args[2]);
                        recoveryReport = new { Success = recoveryJsonResult.IsVerified, Result = recoveryJsonResult, Error = (string?)null };
                        recoveryExitCode = recoveryJsonResult.IsVerified ? 0 : 3;
                    }
                    catch (Exception ex)
                    {
                        recoveryReport = new { Success = false, Result = (object?)null, Error = ex.Message, ErrorType = ex.GetType().FullName };
                        recoveryExitCode = 3;
                    }
                    using (var recoveryReportStream = new FileStream(recoveryReportPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        JsonSerializer.Serialize(recoveryReportStream, recoveryReport, JsonOptions);
                    Console.WriteLine($"資料庫復原驗證 JSON 報告已輸出：{recoveryReportPath}");
                    Console.WriteLine(JsonSerializer.Serialize(recoveryReport, JsonOptions));
                    return recoveryExitCode;
                case "db-export-json":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new DatabaseExportService().ExportJson(args[1], args[2]), JsonOptions));
                    return 0;
                case "db-export-check":
                    RequireCount(args, 2);
                    var exportCheck = new DatabaseExportValidationService().Validate(args[1]);
                    Console.WriteLine(JsonSerializer.Serialize(exportCheck, JsonOptions));
                    return exportCheck.IsValid ? 0 : 3;
                case "db-import-preview":
                    RequireCount(args, 3);
                    var importPreview = new DatabaseImportPreviewService().Preview(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(importPreview, JsonOptions));
                    return importPreview.IsValid ? 0 : 3;
                case "db-import-json":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new DatabaseImportService().ImportIntoEmptyDatabase(args[1], args[2]), JsonOptions));
                    return 0;
                case "db-import-verify":
                    RequireCount(args, 3);
                    var importVerification = new DatabaseImportVerificationService().Verify(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(importVerification, JsonOptions));
                    return importVerification.IsVerified ? 0 : 3;
                case "excel-import-preflight-check":
                    RequireCount(args, 5);
                    var preflightValidation = new ExcelImportPreflightValidationService().Validate(args[1], args[2], args[3], args[4]);
                    Console.WriteLine(JsonSerializer.Serialize(preflightValidation, JsonOptions));
                    return preflightValidation.CanImport ? 0 : 3;

                case "excel-import-preflight":
                    RequireCount(args, 4);
                    var preflight = new ExcelImportPreflightService().Check(args[1], args[2], args[3]);
                    Console.WriteLine(JsonSerializer.Serialize(preflight, JsonOptions));
                    return preflight.CanImport ? 0 : 3;
                case "excel-import-preflight-json":
                    RequireCount(args, 5);
                    var preflightJson = new ExcelImportPreflightService().Check(args[1], args[2], args[3]);
                    var preflightPath = Path.GetFullPath(args[4]);
                    if (File.Exists(preflightPath)) throw new IOException("預檢報告檔已存在，為避免覆蓋已停止輸出。");
                    var preflightDirectory = Path.GetDirectoryName(preflightPath);
                    if (!string.IsNullOrWhiteSpace(preflightDirectory)) Directory.CreateDirectory(preflightDirectory);
                    using (var preflightStream = new FileStream(preflightPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        JsonSerializer.Serialize(preflightStream, preflightJson, JsonOptions);
                    Console.WriteLine($"Excel 匯入預檢 JSON 報告已輸出：{preflightPath}");
                    return preflightJson.CanImport ? 0 : 3;

                case "excel-import-db":
                    RequireCount(args, 5);
                    Console.WriteLine(JsonSerializer.Serialize(new ExcelDatabaseImportService().Import(args[1], args[2], args[3], args[4]), JsonOptions));
                    return 0;
                case "excel-import-db-safe":
                    RequireCount(args, 6);
                    var safeImport = new ExcelImportWorkflowService().Execute(args[1], args[2], args[3], args[4], args[5]);
                    Console.WriteLine(JsonSerializer.Serialize(safeImport, JsonOptions));
                    return safeImport.BatchVerified ? 0 : 3;
                case "excel-import-db-safe-json":
                    RequireCount(args, 7);
                    var safeReportPath = Path.GetFullPath(args[6]);
                    if (File.Exists(safeReportPath)) throw new IOException("匯入流程報告已存在，為避免覆蓋已停止執行。");
                    var safeReportDirectory = Path.GetDirectoryName(safeReportPath);
                    if (!string.IsNullOrWhiteSpace(safeReportDirectory)) Directory.CreateDirectory(safeReportDirectory);
                    var safeImportReport = new ExcelImportWorkflowService().Execute(args[1], args[2], args[3], args[4], args[5]);
                    using (var safeReportStream = new FileStream(safeReportPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        JsonSerializer.Serialize(safeReportStream, safeImportReport, JsonOptions);
                    Console.WriteLine(JsonSerializer.Serialize(new { Report = safeReportPath, Result = safeImportReport }, JsonOptions));
                    return safeImportReport.BatchVerified ? 0 : 3;
                case "excel-import-batch-diff-json":
                    RequireCount(args, 5);
                    var batchDiffJson = new ExcelImportBatchDiffService().Compare(args[1], args[2], args[3]);
                    var batchDiffJsonPath = Path.GetFullPath(args[4]);
                    if (File.Exists(batchDiffJsonPath)) throw new IOException("差異報告檔已存在，為避免覆蓋已停止輸出。");
                    var batchDiffDirectory = Path.GetDirectoryName(batchDiffJsonPath);
                    if (!string.IsNullOrWhiteSpace(batchDiffDirectory)) Directory.CreateDirectory(batchDiffDirectory);
                    using (var diffStream = new FileStream(batchDiffJsonPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        JsonSerializer.Serialize(diffStream, batchDiffJson, JsonOptions);
                    Console.WriteLine($"批次差異 JSON 報告已輸出：{batchDiffJsonPath}");
                    return 0;

                case "excel-import-batch-diff":
                    RequireCount(args, 4);
                    var batchDiff = new ExcelImportBatchDiffService().Compare(args[1], args[2], args[3]);
                    Console.WriteLine(JsonSerializer.Serialize(batchDiff, JsonOptions));
                    return 0;

                case "excel-import-verify":
                    RequireCount(args, 3);
                    var excelImportVerification = new ExcelImportBatchVerificationService().Verify(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(excelImportVerification, JsonOptions));
                    return excelImportVerification.IsVerified ? 0 : 3;
                case "excel-import-batches":
                    if (args.Length is < 2 or > 3) throw new ArgumentException("excel-import-batches <database.db> [limit]");
                    var batchLimit = args.Length == 3 ? int.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture) : 50;
                    Console.WriteLine(JsonSerializer.Serialize(new ExcelImportBatchManagementService().List(args[1], batchLimit), JsonOptions));
                    return 0;
                case "excel-import-batch-entries":
                    if (args.Length is < 3 or > 5) throw new ArgumentException("excel-import-batch-entries <database.db> <batch-id> [page] [page-size]");
                    var sourcePage = args.Length >= 4 ? int.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture) : 1;
                    var sourcePageSize = args.Length == 5 ? int.Parse(args[4], System.Globalization.CultureInfo.InvariantCulture) : 50;
                    var batchEntries = new ExcelImportBatchManagementService().ListEntries(args[1], args[2], sourcePage, sourcePageSize);
                    Console.WriteLine(JsonSerializer.Serialize(new { BatchId = args[2], Page = sourcePage, PageSize = sourcePageSize, Count = batchEntries.Count, Entries = batchEntries }, JsonOptions));
                    return 0;

                case "entry-source-trace":
                    RequireCount(args, 3);
                    var entrySources = new ExcelImportBatchManagementService().ListEntrySources(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(new { EntryId = args[2], Count = entrySources.Count, Sources = entrySources }, JsonOptions));
                    return 0;

                case "excel-import-report":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new ExcelImportBatchManagementService().Report(args[1], args[2]), JsonOptions));
                    return 0;
                case "excel-import-report-json":
                    RequireCount(args, 4);
                    var batchReport = new ExcelImportBatchManagementService().Report(args[1], args[2]);
                    var batchReportPath = Path.GetFullPath(args[3]);
                    if (File.Exists(batchReportPath)) throw new IOException("報告檔已存在，為避免覆蓋已停止輸出。");
                    var batchReportDirectory = Path.GetDirectoryName(batchReportPath);
                    if (!string.IsNullOrWhiteSpace(batchReportDirectory)) Directory.CreateDirectory(batchReportDirectory);
                    using (var reportStream = new FileStream(batchReportPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        JsonSerializer.Serialize(reportStream, batchReport, JsonOptions);
                    Console.WriteLine($"批次 JSON 報告已輸出：{batchReportPath}");
                    return 0;

                case "entry-add":
                    if (args.Length is < 4 or > 5) throw new ArgumentException("entry-add <database.db> <title> <type> [summary]");
                    var addedEntry = new EntryService().Create(args[1], args[2], args[3], args.Length == 5 ? args[4] : null);
                    Console.WriteLine(JsonSerializer.Serialize(addedEntry, JsonOptions));
                    return 0;

                case "entry-search":
                    if (args.Length is < 3 or > 4) throw new ArgumentException("entry-search <database.db> <keyword> [limit]");
                    var entryLimit = args.Length == 4 ? int.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture) : 50;
                    var entryResults = new EntryService().Search(args[1], args[2], entryLimit);
                    Console.WriteLine(JsonSerializer.Serialize(new { Count = entryResults.Count, Results = entryResults }, JsonOptions));
                    return 0;
                case "entry-query":
                    if (args.Length < 2) throw new ArgumentException("entry-query <database.db> [keyword] [--type value] [--tag value] [--archived exclude|include|only] [--sort updated-desc|title-asc|title-desc|created-desc|created-asc] [--created-from yyyy-MM-dd] [--created-to yyyy-MM-dd] [--updated-from yyyy-MM-dd] [--updated-to yyyy-MM-dd] [--page n] [--page-size n] [--output report.json]");
                    string? queryKeyword = null, queryType = null, queryTag = null;
                    string? queryCreatedFrom = null, queryCreatedTo = null, queryUpdatedFrom = null, queryUpdatedTo = null;
                    var querySort = "updated-desc";
                    var queryArchived = "exclude";
                    var queryPage = 1;
                    var queryPageSize = 50;
                    string? queryOutput = null;
                    var queryIndex = 2;
                    if (queryIndex < args.Length && !args[queryIndex].StartsWith("--", StringComparison.Ordinal)) queryKeyword = args[queryIndex++];
                    while (queryIndex < args.Length)
                    {
                        var option = args[queryIndex++];
                        if (queryIndex >= args.Length) throw new ArgumentException($"{option} 後面缺少值。");
                        var value = args[queryIndex++];
                        switch (option)
                        {
                            case "--type": queryType = value; break;
                            case "--tag": queryTag = value; break;
                            case "--archived": queryArchived = value; break;
                            case "--sort": querySort = value; break;
                            case "--created-from": queryCreatedFrom = value; break;
                            case "--created-to": queryCreatedTo = value; break;
                            case "--updated-from": queryUpdatedFrom = value; break;
                            case "--updated-to": queryUpdatedTo = value; break;
                            case "--page": queryPage = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                            case "--page-size": queryPageSize = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                            case "--output": queryOutput = value; break;
                            default: throw new ArgumentException($"未知搜尋選項：{option}");
                        }
                    }
                    var queryResult = new EntryQueryService().Query(args[1], queryKeyword, queryType, queryTag, queryArchived, queryPage, queryPageSize, querySort, queryCreatedFrom, queryCreatedTo, queryUpdatedFrom, queryUpdatedTo);
                    var queryJson = JsonSerializer.Serialize(queryResult, JsonOptions);
                    if (queryOutput is null) Console.WriteLine(queryJson);
                    else
                    {
                        SafeFileOutput.WriteAllText(queryOutput, queryJson + Environment.NewLine);
                        Console.WriteLine($"搜尋結果已安全匯出：{Path.GetFullPath(queryOutput)}（{queryResult.TotalCount} 筆符合，本頁 {queryResult.Items.Count} 筆）");
                    }
                    return 0;
                case "entry-query-benchmark":
                    if (args.Length < 2) throw new ArgumentException("entry-query-benchmark <database.db> [keyword] [--iterations 1-20] [--type value] [--tag value] [--archived exclude|include|only] [--sort updated-desc|title-asc|title-desc|created-desc|created-asc] [--created-from yyyy-MM-dd] [--created-to yyyy-MM-dd] [--updated-from yyyy-MM-dd] [--updated-to yyyy-MM-dd] [--page n] [--page-size n]");
                    string? benchmarkKeyword = null, benchmarkType = null, benchmarkTag = null;
                    string? benchmarkCreatedFrom = null, benchmarkCreatedTo = null, benchmarkUpdatedFrom = null, benchmarkUpdatedTo = null;
                    var benchmarkSort = "updated-desc";
                    var benchmarkArchived = "exclude";
                    var benchmarkPage = 1;
                    var benchmarkPageSize = 50;
                    var benchmarkIterations = 5;
                    var benchmarkIndex = 2;
                    if (benchmarkIndex < args.Length && !args[benchmarkIndex].StartsWith("--", StringComparison.Ordinal)) benchmarkKeyword = args[benchmarkIndex++];
                    while (benchmarkIndex < args.Length)
                    {
                        var option = args[benchmarkIndex++];
                        if (benchmarkIndex >= args.Length) throw new ArgumentException($"{option} 後面缺少值。");
                        var value = args[benchmarkIndex++];
                        switch (option)
                        {
                            case "--type": benchmarkType = value; break;
                            case "--tag": benchmarkTag = value; break;
                            case "--archived": benchmarkArchived = value; break;
                            case "--sort": benchmarkSort = value; break;
                            case "--created-from": benchmarkCreatedFrom = value; break;
                            case "--created-to": benchmarkCreatedTo = value; break;
                            case "--updated-from": benchmarkUpdatedFrom = value; break;
                            case "--updated-to": benchmarkUpdatedTo = value; break;
                            case "--page": benchmarkPage = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                            case "--page-size": benchmarkPageSize = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                            case "--iterations": benchmarkIterations = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                            default: throw new ArgumentException($"未知效能量測選項：{option}");
                        }
                    }
                    var benchmarkReport = new EntryQueryBenchmarkService().Run(args[1], benchmarkKeyword, benchmarkType, benchmarkTag, benchmarkArchived, benchmarkPage, benchmarkPageSize, benchmarkSort, benchmarkCreatedFrom, benchmarkCreatedTo, benchmarkUpdatedFrom, benchmarkUpdatedTo, benchmarkIterations);
                    Console.WriteLine(JsonSerializer.Serialize(benchmarkReport, JsonOptions));
                    return 0;

                case "entry-query-plan":
                    if (args.Length < 2) throw new ArgumentException("entry-query-plan <database.db> [keyword] [--type value] [--tag value] [--archived exclude|include|only] [--sort updated-desc|title-asc|title-desc|created-desc|created-asc] [--created-from yyyy-MM-dd] [--created-to yyyy-MM-dd] [--updated-from yyyy-MM-dd] [--updated-to yyyy-MM-dd]");
                    string? planType = null, planTag = null, planKeyword = null, planCreatedFrom = null, planCreatedTo = null, planUpdatedFrom = null, planUpdatedTo = null;
                    var planArchived = "exclude";
                    var planSort = "updated-desc";
                    var planIndexStart = 2;
                    if (planIndexStart < args.Length && !args[planIndexStart].StartsWith("--", StringComparison.Ordinal)) planKeyword = args[planIndexStart++];
                    for (var planIndex = planIndexStart; planIndex < args.Length; planIndex += 2)
                    {
                        if (planIndex + 1 >= args.Length) throw new ArgumentException($"{args[planIndex]} 後面缺少值。");
                        var planValue = args[planIndex + 1];
                        switch (args[planIndex])
                        {
                            case "--type": planType = planValue; break;
                            case "--tag": planTag = planValue; break;
                            case "--archived": planArchived = planValue; break;
                            case "--sort": planSort = planValue; break;
                            case "--created-from": planCreatedFrom = planValue; break;
                            case "--created-to": planCreatedTo = planValue; break;
                            case "--updated-from": planUpdatedFrom = planValue; break;
                            case "--updated-to": planUpdatedTo = planValue; break;
                            default: throw new ArgumentException($"未知查詢計畫選項：{args[planIndex]}");
                        }
                    }
                    var planReport = new EntryQueryPlanService().Explain(args[1], planType, planTag, planArchived, planSort, planKeyword, planCreatedFrom, planCreatedTo, planUpdatedFrom, planUpdatedTo);
                    Console.WriteLine(JsonSerializer.Serialize(planReport, JsonOptions));
                    return 0;

                case "entry-get":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryDetailService().Get(args[1], args[2]), JsonOptions));
                    return 0;
                case "entry-update":
                    if (args.Length is < 6 or > 6) throw new ArgumentException("entry-update <database.db> <id> <title> <type> <summary>");
                    Console.WriteLine(JsonSerializer.Serialize(new EntryDetailService().Update(args[1], args[2], args[3], args[4], args[5]), JsonOptions));
                    return 0;
                case "entry-archive":
                    RequireCount(args, 4);
                    if (!bool.TryParse(args[3], out var archived)) throw new ArgumentException("archived 必須是 true 或 false。");
                    Console.WriteLine(JsonSerializer.Serialize(new EntryDetailService().SetArchived(args[1], args[2], archived), JsonOptions));
                    return 0;
                case "field-set":
                    if (args.Length is < 6 or > 7) throw new ArgumentException("field-set <database.db> <entry-id> <key> <label> <value> [value-type]");
                    Console.WriteLine(JsonSerializer.Serialize(new EntryDetailService().SetField(args[1], args[2], args[3], args[4], args[5], args.Length==7?args[6]:"Text"), JsonOptions));
                    return 0;
                case "field-list":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryDetailService().ListFields(args[1], args[2]), JsonOptions));
                    return 0;
                case "note-add":
                    RequireCount(args, 4);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryContentService().AddNote(args[1], args[2], args[3]), JsonOptions));
                    return 0;
                case "note-list":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryContentService().ListNotes(args[1], args[2]), JsonOptions));
                    return 0;
                case "note-update":
                    RequireCount(args, 4);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryContentService().UpdateNote(args[1], args[2], args[3]), JsonOptions));
                    return 0;
                case "note-delete":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryContentService().DeleteNote(args[1], args[2]), JsonOptions));
                    return 0;
                case "image-add":
                    if (args.Length is < 4 or > 5) throw new ArgumentException("image-add <database.db> <entry-id> <image-path> [caption]");
                    Console.WriteLine(JsonSerializer.Serialize(new EntryContentService().AddImage(args[1], args[2], args[3], args.Length == 5 ? args[4] : null), JsonOptions));
                    return 0;
                case "image-list":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryContentService().ListImages(args[1], args[2]), JsonOptions));
                    return 0;
                case "image-remove":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryContentService().RemoveImage(args[1], args[2]), JsonOptions));
                    return 0;
                case "source-list":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryContentService().ListSources(args[1], args[2]), JsonOptions));
                    return 0;
                case "tag-add":
                    RequireCount(args, 4);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryTagService().Add(args[1], args[2], args[3]), JsonOptions));
                    return 0;
                case "tag-remove":
                    RequireCount(args, 4);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryTagService().Remove(args[1], args[2], args[3]), JsonOptions));
                    return 0;
                case "tag-list":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryTagService().List(args[1], args[2]), JsonOptions));
                    return 0;
                case "tag-search":
                    RequireCount(args, 3);
                    Console.WriteLine(JsonSerializer.Serialize(new EntryTagService().Search(args[1], args[2]), JsonOptions));
                    return 0;
                case "fixture":
                    RequireCount(args, 2);
                    var fixture = Path.GetFullPath(args[1]);
                    WorkbookFixtureBuilder.CreateBasicFixture(fixture);
                    Console.WriteLine($"測試活頁簿已建立：{fixture}");
                    return 0;

                case "fixture-large":
                    RequireCount(args, 3);
                    var largeFixture = Path.GetFullPath(args[1]);
                    var largeRows = int.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture);
                    WorkbookFixtureBuilder.CreateLargeFixture(largeFixture, largeRows);
                    Console.WriteLine($"大型測試活頁簿已建立：{largeFixture}；資料筆數：{largeRows:N0}");
                    return 0;

                case "inspect":
                    RequireCount(args, 2);
                    Console.WriteLine(JsonSerializer.Serialize(WorkbookReader.Inspect(args[1]), JsonOptions));
                    return 0;

                case "import-preview":
                    if (args.Length < 2)
                        throw new ArgumentException("import-preview 參數不足。");
                    var previewLimit = 10;
                    string? reportPath = null;
                    for (var i = 2; i < args.Length; i++)
                    {
                        if (args[i] == "--json")
                        {
                            if (++i >= args.Length)
                                throw new ArgumentException("--json 後面必須指定報告檔路徑。");
                            reportPath = args[i];
                        }
                        else if (int.TryParse(args[i], System.Globalization.NumberStyles.Integer,
                                     System.Globalization.CultureInfo.InvariantCulture, out var parsedRows))
                        {
                            previewLimit = parsedRows;
                        }
                        else
                        {
                            throw new ArgumentException($"無法辨識 import-preview 參數：{args[i]}");
                        }
                    }
                    var excelImportPreview = ImportPreviewService.Analyze(args[1], previewLimit);
                    PrintImportPreviewSummary(excelImportPreview);
                    if (!string.IsNullOrWhiteSpace(reportPath))
                    {
                        var fullReportPath = Path.GetFullPath(reportPath);
                        var reportDirectory = Path.GetDirectoryName(fullReportPath);
                        if (!string.IsNullOrWhiteSpace(reportDirectory))
                            Directory.CreateDirectory(reportDirectory);
                        SafeFileOutput.WriteAllText(fullReportPath, JsonSerializer.Serialize(excelImportPreview, JsonOptions));
                        Console.WriteLine($"完整預覽報告：{fullReportPath}");
                    }
                    return 0;

                case "import-quality":
                    if (args.Length is < 2 or > 3)
                        throw new ArgumentException("import-quality 參數數量錯誤。");
                    var qualityReport = ImportQualityService.Analyze(args[1]);
                    var qualityJson = JsonSerializer.Serialize(qualityReport, JsonOptions);
                    if (args.Length == 3)
                    {
                        var qualityPath = Path.GetFullPath(args[2]);
                        var qualityDirectory = Path.GetDirectoryName(qualityPath);
                        if (!string.IsNullOrWhiteSpace(qualityDirectory))
                            Directory.CreateDirectory(qualityDirectory);
                        SafeFileOutput.WriteAllText(qualityPath, qualityJson);
                        Console.WriteLine($"品質分析報告已儲存：{qualityPath}");
                    }
                    else
                    {
                        foreach (var sheet in qualityReport.Sheets)
                        {
                            Console.WriteLine($"[{sheet.State}] {sheet.Name}：資料 {sheet.DataRowCount:N0} 列，空白列 {sheet.BlankRowCount:N0} 列");
                            var warningCount = sheet.Columns.Sum(column => column.Warnings.Count);
                            Console.WriteLine($"  欄位 {sheet.Columns.Count}；需留意欄位 {warningCount}");
                            foreach (var column in sheet.Columns.Where(column => column.Warnings.Count > 0).Take(8))
                                Console.WriteLine($"  - {column.Name}：{string.Join("、", column.Warnings)}");
                        }
                        Console.WriteLine("此為唯讀分析；完整欄位統計請指定 JSON 報告路徑。");
                    }
                    return 0;

                case "mapping-template":
                    RequireCount(args, 3);
                    ImportMappingService.SaveTemplate(args[1], args[2]);
                    Console.WriteLine($"欄位對應範本已建立：{Path.GetFullPath(args[2])}");
                    Console.WriteLine("範本不會匯入資料；請編輯 JSON 後再執行 mapping-check。");
                    return 0;

                case "clean-rules-template":
                    RequireCount(args, 4);
                    ImportCleaningRulesService.SaveTemplate(args[1], args[2], args[3]);
                    Console.WriteLine($"清理規則範本已建立：{Path.GetFullPath(args[3])}");
                    Console.WriteLine("規則預設為 Review；此範本不會修改 Excel 或資料庫。");
                    return 0;

                case "import-dry-run":
                    RequireCount(args, 5);
                    var dryRun = ImportDryRunService.Analyze(args[1], args[2], args[3]);
                    var dryRunJson = JsonSerializer.Serialize(dryRun, JsonOptions);
                    var dryRunPath = Path.GetFullPath(args[4]);
                    var dryRunDirectory = Path.GetDirectoryName(dryRunPath);
                    if (!string.IsNullOrWhiteSpace(dryRunDirectory)) Directory.CreateDirectory(dryRunDirectory);
                    SafeFileOutput.WriteAllText(dryRunPath, dryRunJson);
                    Console.WriteLine($"Dry-run 報告已儲存：{dryRunPath}");
                    Console.WriteLine($"候選資料列：{dryRun.CandidateRows:N0}；需人工檢查：{dryRun.RowsRequiringReview:N0}");
                    Console.WriteLine("未修改 Excel，未寫入資料庫。詳細規則統計請查看 JSON 報告。");
                    return 0;

                case "clean-rules-check":
                    RequireCount(args, 4);
                    var rulesCheck = ImportCleaningRulesService.Validate(args[1], args[2], args[3]);
                    Console.WriteLine(JsonSerializer.Serialize(rulesCheck, JsonOptions));
                    return rulesCheck.IsValid ? 0 : 2;

                case "import-clean-check":
                    if (args.Length is < 3 or > 4)
                        throw new ArgumentException("import-clean-check 參數數量錯誤。");
                    var cleaningReport = ImportCleaningService.Analyze(args[1], args[2]);
                    var cleaningJson = JsonSerializer.Serialize(cleaningReport, JsonOptions);
                    if (args.Length == 4)
                    {
                        var cleaningPath = Path.GetFullPath(args[3]);
                        var cleaningDirectory = Path.GetDirectoryName(cleaningPath);
                        if (!string.IsNullOrWhiteSpace(cleaningDirectory))
                            Directory.CreateDirectory(cleaningDirectory);
                        SafeFileOutput.WriteAllText(cleaningPath, cleaningJson);
                        Console.WriteLine($"資料清理檢查報告已儲存：{cleaningPath}");
                    }
                    else
                    {
                        Console.WriteLine("匯入前資料清理檢查（唯讀）");
                        foreach (var sheet in cleaningReport.Sheets)
                        {
                            Console.WriteLine($"{sheet.SheetName}：掃描 {sheet.RowsScanned:N0} 列；缺少標題 {sheet.MissingTitleRows:N0}；重複標題 {sheet.DuplicateTitleRows:N0}；前後空白 {sheet.CellsWithEdgeWhitespace:N0}；控制字元 {sheet.CellsWithControlCharacters:N0}；錯誤值 {sheet.CellsWithErrorValues:N0}");
                        }
                        Console.WriteLine("未修改來源 Excel；完整問題列號請加上報告路徑輸出 JSON。");
                    }
                    return 0;

                case "mapping-check":
                    RequireCount(args, 3);
                    var mappingCheck = ImportMappingService.Validate(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(mappingCheck, JsonOptions));
                    return mappingCheck.IsValid ? 0 : 2;

                case "validate":
                    if (args.Length is < 2 or > 3)
                        throw new ArgumentException("validate 參數數量錯誤。");
                    var maxErrors = args.Length == 3 ? int.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture) : 100;
                    Console.WriteLine(JsonSerializer.Serialize(WorkbookValidator.Validate(args[1], maxErrors), JsonOptions));
                    return 0;

                case "write-copy":
                    RequireCount(args, 6);
                    var written = WorkbookWriter.WriteTextToCopy(args[1], args[2], args[3], args[4], args[5]);
                    Console.WriteLine($"已在副本寫入文字：{written}");
                    Console.WriteLine("來源檔未修改；請使用 compare 檢查套件差異。");
                    return 0;

                case "snapshot":
                    RequireCount(args, 3);
                    PackageIntegrity.SaveManifest(args[1], args[2]);
                    Console.WriteLine($"套件清單已儲存：{Path.GetFullPath(args[2])}");
                    return 0;

                case "compare":
                    RequireCount(args, 3);
                    var differences = PackageIntegrity.Compare(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(new
                    {
                        Before = Path.GetFullPath(args[1]),
                        After = Path.GetFullPath(args[2]),
                        DifferenceCount = differences.Count,
                        Differences = differences,
                        Note = "此比較以 ZIP 內各 Open XML 部件的解壓內容 SHA-256 為準；不是 Excel 語意等價性證明。"
                    }, JsonOptions));
                    return differences.Count == 0 ? 0 : 2;

                case "index":
                    RequireCount(args, 3);
                    var indexResult = new SearchIndexer().Rebuild(args[1], args[2]);
                    Console.WriteLine(JsonSerializer.Serialize(new
                    {
                        IndexedRows = indexResult.IndexedRows,
                        Tokenizer = indexResult.Tokenizer,
                        Database = Path.GetFullPath(args[2])
                    }, JsonOptions));
                    return 0;

                case "search":
                    if (args.Length is < 3 or > 4)
                        throw new ArgumentException("search 參數數量錯誤。");
                    var limit = args.Length == 4 ? int.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture) : 50;
                    var results = new SearchIndexer().Search(args[1], args[2], limit);
                    Console.WriteLine(JsonSerializer.Serialize(new { Count = results.Count, Results = results }, JsonOptions));
                    return 0;

                case "self-test":
                    RequireCount(args, 2);
                    return RunSelfTest(Path.GetFullPath(args[1]));

                default:
                    throw new ArgumentException($"未知命令：{args[0]}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"錯誤：{ex.Message}");
            Console.Error.WriteLine("使用 help 查看命令。");
            return 1;
        }
    }

    private static void PrintImportPreviewSummary(ImportWorkbookPreview preview)
    {
        Console.WriteLine("Excel 匯入分析摘要");
        Console.WriteLine($"檔案：{Path.GetFileName(preview.File)} ({preview.Extension}, {preview.FileSize:N0} bytes)");
        Console.WriteLine($"工作表：{preview.Sheets.Count}；每張預覽列數：{preview.PreviewRowLimit}");
        Console.WriteLine();

        foreach (var sheet in preview.Sheets)
        {
            Console.WriteLine($"[{sheet.State}] {sheet.Name}");
            Console.WriteLine($"  標題候選列：{(sheet.HeaderRow == 0 ? "未判定" : sheet.HeaderRow)}；欄位：{sheet.ColumnCount}；資料列：{sheet.DataRowCount:N0}；空白列：{sheet.BlankRowCount:N0}");
            if (sheet.Columns.Count > 0)
            {
                const int maxShownColumns = 12;
                var names = sheet.Columns.Take(maxShownColumns).Select(column => column.Name);
                var suffix = sheet.Columns.Count > maxShownColumns ? $" 等（共 {sheet.Columns.Count} 欄）" : string.Empty;
                Console.WriteLine($"  欄位：{string.Join("、", names)}{suffix}");
            }
            if (sheet.Tables.Count > 0)
                Console.WriteLine($"  Excel 表格：{string.Join("；", sheet.Tables.Select(table => $"{table.Name} {table.Range}"))}");
            if (sheet.PreviewRows.Count > 0)
                Console.WriteLine($"  已擷取預覽：{sheet.PreviewRows.Count} 列");
            else
                Console.WriteLine("  沒有可預覽的資料列");
        }

        Console.WriteLine();
        Console.WriteLine("來源檔僅供讀取，未修改 Excel，也未匯入資料庫。");
        Console.WriteLine("需要完整欄位與預覽內容時，請加上 --json <報告檔路徑> 輸出完整報告。");
    }

    private static int RunSelfTest(string root)
    {
        Directory.CreateDirectory(root);
        var originalFolder = Path.Combine(root, "Original");
        var candidateFolder = Path.Combine(root, "Candidate");
        var indexFolder = Path.Combine(root, "Index");
        var indexSourceFolder = Path.Combine(indexFolder, "Source");
        Directory.CreateDirectory(originalFolder);
        Directory.CreateDirectory(candidateFolder);
        Directory.CreateDirectory(indexFolder);
        Directory.CreateDirectory(indexSourceFolder);

        var original = Path.Combine(originalFolder, "01_基本測試.xlsx");
        var candidate = Path.Combine(candidateFolder, "01_基本測試_副本.xlsx");
        var database = Path.Combine(indexFolder, "test-index.db");
        var reportPath = Path.Combine(root, "SelfTestReport.json");

        WorkbookFixtureBuilder.CreateBasicFixture(original);
        File.Copy(original, candidate, overwrite: false);
        var beforeInfo = WorkbookReader.Inspect(original);
        var beforeValidation = WorkbookValidator.Validate(original);
        WorkbookWriter.WriteTextToCopy(candidate, candidate + ".written.xlsx", "基本資料", "H2", "寫入驗證成功");
        var writtenCandidate = candidate + ".written.xlsx";
        var differences = PackageIntegrity.Compare(candidate, writtenCandidate);
        var originalCandidateDifferences = PackageIntegrity.Compare(original, candidate);
        File.Copy(writtenCandidate, Path.Combine(indexSourceFolder, Path.GetFileName(writtenCandidate)), overwrite: false);
        var indexResult = new SearchIndexer().Rebuild(indexSourceFolder, database);
        var searchResults = new SearchIndexer().Search(database, "測試人物", 20);
        var afterInfo = WorkbookReader.Inspect(writtenCandidate);
        var afterValidation = WorkbookValidator.Validate(writtenCandidate);
        var writtenValuePresent = WorkbookReader.ReadRows(writtenCandidate)
            .Any(row => row.Content.Contains("寫入驗證成功", StringComparison.Ordinal));
        var searchContainsExpectedName = searchResults.Any(x => x.Content.Contains("測試人物", StringComparison.Ordinal));
        var snapshotSafety = RunSnapshotSafetySelfTest(Path.Combine(root, "SnapshotSafety"));
        var safeOutputSafety = RunSafeFileOutputSelfTest(Path.Combine(root, "SafeFileOutputSafety"));
        var selfTestPassed = differences.Count > 0
            && originalCandidateDifferences.Count == 0
            && indexResult.IndexedRows >= 5
            && searchContainsExpectedName
            && writtenValuePresent
            && snapshotSafety.Passed
            && safeOutputSafety.Passed;

        var report = new
        {
            Version = "3.0.0",
            CreatedAt = DateTimeOffset.Now,
            SourceFile = original,
            CandidateFile = writtenCandidate,
            OriginalFileWasCopied = File.Exists(original),
            WriteWasPerformedOnCopy = true,
            PackageDifferenceCount = differences.Count,
            PackageDifferences = differences,
            OriginalAndCandidateDifferenceCount = originalCandidateDifferences.Count,
            IndexedRows = indexResult.IndexedRows,
            Tokenizer = indexResult.Tokenizer,
            SearchResultCount = searchResults.Count,
            SearchContainsExpectedName = searchContainsExpectedName,
            WrittenValuePresent = writtenValuePresent,
            SnapshotSafety = snapshotSafety,
            SafeFileOutputSafety = safeOutputSafety,
            SelfTestPassed = selfTestPassed,
            BeforeOverview = beforeInfo,
            AfterOverview = afterInfo,
            BeforeOpenXmlValidation = beforeValidation,
            AfterOpenXmlValidation = afterValidation,
            Limitations = new[]
            {
                "此自我測試只測試基本 XLSX、既有儲存格純文字寫入、ZIP 部件比對及 SQLite FTS5。",
                "尚未實測 XLSM/VBA、Excel COM、Power Query、外部連線、內嵌儲存格圖片、密碼保護工作表及百萬筆效能。",
                "IndexedRows 是原型逐列索引數，並非正式系統穩定資料 ID。"
            }
        };

        SafeFileOutput.WriteAllText(reportPath, JsonSerializer.Serialize(report, JsonOptions));
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            Report = reportPath,
            Original = original,
            ModifiedCopy = writtenCandidate,
            PackageDifferenceCount = differences.Count,
            OriginalAndCandidateDifferenceCount = originalCandidateDifferences.Count,
            IndexedRows = indexResult.IndexedRows,
            SearchResultCount = searchResults.Count,
            WrittenValuePresent = writtenValuePresent,
            SnapshotSafety = snapshotSafety,
            SafeFileOutputSafety = safeOutputSafety,
            SelfTestPassed = selfTestPassed,
            Note = "此結果只代表本版基本自我測試條件；不是完整 Excel 相容性驗收。"
        }, JsonOptions));
        return selfTestPassed ? 0 : 3;
    }


    private static SafeFileOutputSafetySelfTestResult RunSafeFileOutputSelfTest(string root)
    {
        Directory.CreateDirectory(root);
        var existingPath = Path.Combine(root, "protected-output.txt");
        File.WriteAllText(existingPath, "KEEP-ORIGINAL");
        var beforeHash = ExcelImportPreflightService.HashFile(existingPath);
        var overwriteRejected = false;
        try { SafeFileOutput.WriteAllText(existingPath, "MUST-NOT-REPLACE"); }
        catch (IOException) { overwriteRejected = true; }
        var originalPreserved = ExcelImportPreflightService.HashFile(existingPath) == beforeHash;

        var replacePath = Path.Combine(root, "settings.json");
        File.WriteAllText(replacePath, "{\"old\":true}");
        SafeFileOutput.WriteAllTextAtomicallyReplacing(replacePath, "{\"new\":true}");
        var atomicReplaceCompleted = File.ReadAllText(replacePath) == "{\"new\":true}";

        var blockedTarget = Path.Combine(root, "blocked-target");
        Directory.CreateDirectory(blockedTarget);
        var failedReplaceRejected = false;
        try { SafeFileOutput.WriteAllTextAtomicallyReplacing(blockedTarget, "not-written"); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { failedReplaceRejected = true; }
        var temporaryFilesCleaned = !Directory.EnumerateFiles(root, "*.tmp", SearchOption.TopDirectoryOnly).Any();
        var passed = overwriteRejected && originalPreserved && atomicReplaceCompleted
            && failedReplaceRejected && temporaryFilesCleaned;
        return new SafeFileOutputSafetySelfTestResult(
            passed,
            overwriteRejected,
            originalPreserved,
            atomicReplaceCompleted,
            failedReplaceRejected,
            temporaryFilesCleaned);
    }

    private static SnapshotSafetySelfTestResult RunSnapshotSafetySelfTest(string root)
    {
        Directory.CreateDirectory(root);
        var workbook = Path.Combine(root, "snapshot-source.xlsm");
        var mapping = Path.Combine(root, "mapping.json");
        var rules = Path.Combine(root, "rules.json");
        using (var workbookStream = new FileStream(workbook, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            var buffer = new byte[1024 * 128];
            for (var block = 0; block < 64; block++)
            {
                for (var i = 0; i < buffer.Length; i++) buffer[i] = (byte)((i + block) % 251);
                workbookStream.Write(buffer);
            }
            workbookStream.Flush(flushToDisk: true);
        }
        File.WriteAllText(mapping, "{\"mapping\":true}");
        File.WriteAllText(rules, "{\"rules\":true}");
        var workbookHash = ExcelImportPreflightService.HashFile(workbook);
        var mappingHash = ExcelImportPreflightService.HashFile(mapping);
        var rulesHash = ExcelImportPreflightService.HashFile(rules);
        var originalHashBefore = workbookHash;
        var snapshotDirectory = string.Empty;
        var readOnlyFiles = false;
        var extensionPreserved = false;
        var cleanupSucceeded = false;
        using (var snapshot = ExcelImportInputSnapshot.Create(workbook, mapping, rules, workbookHash, mappingHash, rulesHash))
        {
            snapshotDirectory = snapshot.SnapshotDirectory;
            readOnlyFiles = new[] { snapshot.WorkbookPath, snapshot.MappingPath, snapshot.RulesPath }
                .All(path => (File.GetAttributes(path) & FileAttributes.ReadOnly) != 0);
            extensionPreserved = string.Equals(Path.GetExtension(snapshot.WorkbookPath), ".xlsm", StringComparison.OrdinalIgnoreCase);
            if (ExcelImportPreflightService.HashFile(snapshot.WorkbookPath) != workbookHash
                || ExcelImportPreflightService.HashFile(snapshot.MappingPath) != mappingHash
                || ExcelImportPreflightService.HashFile(snapshot.RulesPath) != rulesHash)
                throw new InvalidDataException("快照自我測試內容雜湊不一致。");
        }
        cleanupSucceeded = !Directory.Exists(snapshotDirectory);

        var mismatchRejected = false;
        try
        {
            _ = ExcelImportInputSnapshot.Create(workbook, mapping, rules, new string('0', 64), mappingHash, rulesHash);
        }
        catch (IOException) { mismatchRejected = true; }

        var originalUnchanged = ExcelImportPreflightService.HashFile(workbook) == originalHashBefore;
        var passed = readOnlyFiles && extensionPreserved && cleanupSucceeded && mismatchRejected && originalUnchanged;
        return new SnapshotSafetySelfTestResult(
            passed,
            readOnlyFiles,
            extensionPreserved,
            cleanupSucceeded,
            mismatchRejected,
            originalUnchanged,
            new FileInfo(workbook).Length);
    }

    private sealed record SafeFileOutputSafetySelfTestResult(
        bool Passed,
        bool ExistingOutputOverwriteRejected,
        bool ExistingOutputContentPreserved,
        bool AtomicSettingsReplacementCompleted,
        bool InvalidReplacementTargetRejected,
        bool TemporaryFilesCleanedAfterFailure);

    private sealed record SnapshotSafetySelfTestResult(
        bool Passed,
        bool ReadOnlySnapshotFiles,
        bool WorkbookExtensionPreserved,
        bool SnapshotCleanedAfterDispose,
        bool HashMismatchRejected,
        bool OriginalWorkbookUnchanged,
        long LargeFileBytes);

    private static void RequireCount(string[] args, int expected)
    {
        if (args.Length != expected)
            throw new ArgumentException("命令參數數量錯誤。請使用 help 查看格式。");
    }

    private static void PrintHelp()
    {
        Console.WriteLine("PersonalWiki ExcelLab 3.0.0");
        Console.WriteLine();
        Console.WriteLine("命令：");
        Console.WriteLine("  entry-add <database.db> <title> <type> [summary]  新增一筆知識條目");
        Console.WriteLine("  entry-search <database.db> <keyword> [limit]     搜尋知識條目全文索引");
        Console.WriteLine("  entry-query-benchmark <database.db> [keyword] [--iterations 1-20] [--type value] [--tag value] [--archived exclude|include|only] [--sort ...] [--created-from yyyy-MM-dd] [--created-to yyyy-MM-dd] [--updated-from yyyy-MM-dd] [--updated-to yyyy-MM-dd] [--page n] [--page-size n]  唯讀量測搜尋實際耗時\n  entry-query-plan <database.db> [keyword] [--type value] [--tag value] [--archived exclude|include|only] [--sort ...] [--created-from yyyy-MM-dd] [--created-to yyyy-MM-dd] [--updated-from yyyy-MM-dd] [--updated-to yyyy-MM-dd]  唯讀檢查完整搜尋查詢計畫\n  entry-query <database.db> [keyword] [--type value] [--tag value] [--archived exclude|include|only] [--sort updated-desc|title-asc|title-desc|created-desc|created-asc] [--created-from yyyy-MM-dd] [--created-to yyyy-MM-dd] [--updated-from yyyy-MM-dd] [--updated-to yyyy-MM-dd] [--page n] [--page-size n]  多條件分頁搜尋");
        Console.WriteLine("  entry-get <database.db> <id>                     讀取條目詳細資料");
        Console.WriteLine("  entry-update <database.db> <id> <title> <type> <summary>  更新條目並同步全文索引");
        Console.WriteLine("  entry-archive <database.db> <id> <true|false>   封存或還原條目（不實體刪除）");
        Console.WriteLine("  field-set <database.db> <entry-id> <key> <label> <value> [type]  新增或更新自訂欄位");
        Console.WriteLine("  field-list <database.db> <entry-id>             讀取條目自訂欄位");
        Console.WriteLine("  note-add <database.db> <entry-id> <text>        新增條目筆記");
        Console.WriteLine("  note-list <database.db> <entry-id>              列出條目筆記");
        Console.WriteLine("  note-update <database.db> <note-id> <text>      修改筆記");
        Console.WriteLine("  note-delete <database.db> <note-id>             刪除筆記");
        Console.WriteLine("  image-add <database.db> <entry-id> <path> [caption]  建立圖片關聯並計算 SHA-256");
        Console.WriteLine("  image-list <database.db> <entry-id>             列出圖片關聯及檔案狀態");
        Console.WriteLine("  image-remove <database.db> <image-id>           移除關聯，不刪除硬碟圖片");
        Console.WriteLine("  source-list <database.db> <entry-id>             查看 Excel 匯入來源追溯");
        Console.WriteLine("  tag-add <database.db> <entry-id> <tag>          為條目新增標籤");
        Console.WriteLine("  tag-remove <database.db> <entry-id> <tag>       移除條目標籤");
        Console.WriteLine("  tag-list <database.db> <entry-id>               列出條目標籤");
        Console.WriteLine("  tag-search <database.db> <tag>                  依標籤尋找條目");
        Console.WriteLine("  db-init <database.db>                       建立或升級個人知識庫 SQLite 結構（不匯入資料）");
        Console.WriteLine("  db-info <database.db>                       檢查資料庫版本、資料表及筆數");
        Console.WriteLine("  db-check <database.db>                      唯讀檢查 SQLite 完整性及外鍵關係");
        Console.WriteLine("  db-index-check <database.db>                唯讀檢查全文索引與條目一致性");
        Console.WriteLine("  db-index-rebuild <database.db>              交易式重建全文索引（不修改條目資料）");
        Console.WriteLine("  db-backup <database.db> <backup.db>          建立一致性資料庫備份（不覆蓋既有檔案）");
        Console.WriteLine("  excel-import-db-safety <database.db> <backup.db> 匯入前檢查資料庫並建立、驗證備份（不執行匯入）");
        Console.WriteLine("  excel-import-db-safety-json <database.db> <backup.db> <report.json> 建立備份並輸出安全檢查 JSON 報告（不覆蓋）");
        Console.WriteLine("  excel-import-db-safety-check <report.json> 唯讀驗證安全報告及備份檔雜湊、完整性與 Schema");
        Console.WriteLine("  db-export-json <database.db> <export.json>    匯出知識庫資料為 JSON（不覆蓋既有檔案）");
        Console.WriteLine("  db-export-check <export.json>                 唯讀驗證 JSON 匯出檔結構與必要欄位");
        Console.WriteLine("  db-import-preview <database.db> <import.json> 唯讀預覽匯入相容性、關聯及重複衝突");
        Console.WriteLine("  db-import-json <database.db> <import.json>    匯入 JSON（僅限空白資料庫，交易式寫入）");
        Console.WriteLine("  db-import-verify <database.db> <import.json> 匯入後唯讀比對筆數、外鍵及搜尋索引");
        Console.WriteLine("  excel-import-db <database.db> <file.xlsx|file.xlsm> <mapping.json> <rules.json> 依欄位對應匯入 Excel（交易式寫入）");
        Console.WriteLine("  excel-import-db-safe <database.db> <file.xlsx|file.xlsm> <mapping.json> <rules.json> <backup.db> 預檢、備份驗證、正式匯入及匯入後驗證整合流程");
        Console.WriteLine("  excel-import-db-safe-json <database.db> <file.xlsx|file.xlsm> <mapping.json> <rules.json> <backup.db> <report.json> 執行安全匯入並輸出完整 JSON 結果（不覆蓋）");
        Console.WriteLine("  excel-import-batch-diff-json <database.db> <earlier-batch-id> <later-batch-id> <report.json> 輸出差異 JSON 報告（不覆蓋）");
        Console.WriteLine("  excel-import-batch-diff <database.db> <earlier-batch-id> <later-batch-id> 唯讀比較兩次 Excel 匯入的條目及自訂欄位新增、刪除與變更，附欄位彙總");
        Console.WriteLine("  excel-import-verify <database.db> <batch-id> 唯讀驗證 Excel 匯入批次、來源、索引及外鍵");
        Console.WriteLine("  excel-import-batches <database.db> [limit]    唯讀列出匯入批次歷史（最多 500 筆）");
        Console.WriteLine("  excel-import-batch-entries <database.db> <batch-id> [page] [page-size] 唯讀列出批次來源條目及 Excel 列位置");
        Console.WriteLine("  entry-source-trace <database.db> <entry-id> 唯讀查詢條目的所有 Excel 來源追溯紀錄");
        Console.WriteLine("  excel-import-report <database.db> <batch-id> 唯讀輸出指定批次的完整統計報告");
        Console.WriteLine("  excel-import-report-json <database.db> <batch-id> <report.json> 輸出批次 JSON 報告（不覆蓋）");
        Console.WriteLine("  db-restore <backup.db> <new-database.db>     還原至新路徑並執行完整性檢查（不覆蓋既有檔案）");
        Console.WriteLine("  db-recovery-restore-verify <backup.db> <new-database.db> 還原備份至新路徑，驗證健康、Schema 與各資料表筆數（不覆蓋）");
        Console.WriteLine("  db-recovery-restore-verify-json <backup.db> <new-database.db> <report.json> 執行復原驗證並輸出成功/失敗 JSON 報告（不覆蓋）");
        Console.WriteLine("  db-content-compare <left.db> <right.db> 唯讀逐資料表比較 Schema、筆數與資料內容雜湊");
        Console.WriteLine("  db-integrity-report <database.db> <report.json> 輸出資料庫完整性、全文索引及逐表筆數報告（唯讀、不覆蓋）");
        Console.WriteLine("  db-integrity-report-check <database.db> <report.json> 唯讀驗證完整性報告與目前資料庫是否一致");
        Console.WriteLine("  db-schema-compare <left.db> <right.db> 唯讀比較 SQLite 資料表、索引、檢視表及觸發器定義");
        Console.WriteLine("  db-schema-snapshot <database.db> <snapshot.json> 唯讀建立資料庫 Schema 快照（不覆蓋）");
        Console.WriteLine("  db-schema-snapshot-check <database.db> <snapshot.json> 唯讀驗證資料庫是否符合 Schema 快照");
        Console.WriteLine("  db-schema-snapshot-check-json <database.db> <snapshot.json> <report.json> 驗證 Schema 快照並輸出成功/失敗 JSON 報告（不覆蓋）");
        Console.WriteLine("  db-tag-usage-report <database.db> <report.json> 唯讀彙總標籤使用頻率分布（不輸出標籤文字、不覆蓋）");
        Console.WriteLine("  db-field-usage-report <database.db> <report.json> 唯讀彙總欄位使用頻率及值類型（不輸出欄位名稱或值、不覆蓋）");
        Console.WriteLine("  db-note-usage-report <database.db> <report.json> 唯讀彙總筆記數量及長度分布（不輸出筆記正文、不覆蓋）");
        Console.WriteLine("  db-batch-source-reconciliation <database.db> <report.json> 唯讀核對匯入批次筆數與來源追溯紀錄，不輸出識別資料、不覆蓋");
        Console.WriteLine("  db-image-file-integrity-audit <database.db> <report.json> 唯讀驗證圖片檔案存在狀態及已記錄 SHA256（僅輸出統計、不輸出路徑、不覆蓋）");
        Console.WriteLine("  db-relationship-audit <database.db> <report.json> 唯讀檢查 SQLite 外鍵關聯完整性（只輸出表級統計、不輸出資料列識別碼、不覆蓋）");
        Console.WriteLine("  db-source-trace-audit <database.db> <report.json> 唯讀稽核 Excel 來源追溯完整性（只輸出統計、不輸出來源路徑、不覆蓋）");
        Console.WriteLine("  db-entry-inventory-report <database.db> <report.json> 唯讀彙總條目類型、封存狀態及欄位/筆記/圖片/標籤數量（不輸出內容、不覆蓋）");
        Console.WriteLine("  db-import-batch-audit <database.db> <report.json> 唯讀檢查匯入批次狀態、筆數及時間欄位（不輸出來源路徑、不覆蓋）");
        Console.WriteLine("  db-tag-audit <database.db> <report.json> 唯讀檢查空白標籤、標準化不一致及重複標籤候選（不輸出標籤文字、不覆蓋）");
        Console.WriteLine("  db-field-audit <database.db> <report.json> 唯讀檢查空白欄位及同條目重複欄位名稱候選（不輸出欄位值、不覆蓋）");
        Console.WriteLine("  db-note-audit <database.db> <report.json> 唯讀檢查空白筆記、無效時間及完全相同筆記候選（不輸出筆記內容、不覆蓋）");
        Console.WriteLine("  db-storage-report <database.db> <report.json> 唯讀產生資料庫檔案、頁面、索引及資料表筆數報告（不覆蓋）");
        Console.WriteLine("  db-diagnostic-report <database.db> <report.json> 整合唯讀健康、全文索引、儲存空間及資料表統計；異常回傳代碼 3（不覆蓋）");
        Console.WriteLine("  db-diagnostic-compare <baseline.json> <current.json> <report.json> 比較兩份診斷快照，輸出健康、Schema、索引、容量及資料表筆數差異（不覆蓋）");
        Console.WriteLine("  db-vacuum-copy <source.db> <optimized-copy.db> 建立 VACUUM 最佳化副本；來源唯讀、不覆蓋目的檔");
        Console.WriteLine("  db-image-duplicate-report <database.db> <report.json> 唯讀依圖片 SHA-256 找出重複圖片關聯（不刪檔、不覆蓋）");
        Console.WriteLine("  db-image-inventory-report <database.db> <report.json> 唯讀列出圖片關聯、檔案存在狀態及大小（不改檔、不覆蓋）");
        Console.WriteLine("  db-entry-audit <database.db> <report.json> 唯讀檢查標題標準化、跨類型同名候選及遺失圖片檔案（不覆蓋）");
        Console.WriteLine("  fixture <output.xlsx>                         建立基本測試活頁簿");
        Console.WriteLine("  fixture-large <output.xlsx> <data-rows>       串流建立大型資料測試活頁簿（最多 1,048,575 筆）");
        Console.WriteLine("  inspect <file.xlsx|file.xlsm>                 檢查活頁簿與工作表結構");
        Console.WriteLine("  import-preview <file.xlsx|file.xlsm> [rows] [--json report.json]  摘要分析；可另存完整報告");
        Console.WriteLine("  import-quality <file.xlsx|file.xlsm> [report.json]  唯讀檢查空白欄位、重複標題及錯誤值");
        Console.WriteLine("  mapping-template <file.xlsx|file.xlsm> <mapping.json>  建立欄位對應範本");
        Console.WriteLine("  mapping-check <file.xlsx|file.xlsm> <mapping.json>  檢查欄位對應設定");
        Console.WriteLine("  import-clean-check <file.xlsx|file.xlsm> <mapping.json> [report.json]  唯讀檢查匯入資料問題");
        Console.WriteLine("  clean-rules-template <file.xlsx|file.xlsm> <mapping.json> <rules.json>  建立清理規則範本");
        Console.WriteLine("  clean-rules-check <file.xlsx|file.xlsm> <mapping.json> <rules.json>  驗證清理規則設定");
        Console.WriteLine("  import-dry-run <file.xlsx|file.xlsm> <mapping.json> <rules.json> <report.json>  唯讀模擬匯入並輸出問題摘要");
        Console.WriteLine("  validate <file.xlsx|file.xlsm> [max-errors]   執行 Open XML 結構驗證");
        Console.WriteLine("  write-copy <input> <output> <sheet> <cell> <value>  複製後修改既有儲存格");
        Console.WriteLine("  snapshot <file.xlsx> <manifest.json>          儲存套件部件 SHA-256 清單");
        Console.WriteLine("  compare <before.xlsx> <after.xlsx>            比較 Open XML 套件部件");
        Console.WriteLine("  index <folder> <index.db>                     建立/重建 SQLite FTS5 索引");
        Console.WriteLine("  search <index.db> <keyword> [limit]           搜尋索引");
        Console.WriteLine("  self-test <output-folder>                     執行完整基本自我測試流程");
        Console.WriteLine("  版本：3.9.0；SQLite Schema user_version=1");
        Console.WriteLine();
        Console.WriteLine("安全限制：write-copy 永遠修改輸出副本，不提供原地覆寫原始 Excel 的命令。");
    }
}
