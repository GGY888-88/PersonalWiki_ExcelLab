using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ExcelDatabaseImportResult(string BatchId, string SourceFile, long SourceRows, long ImportedRows, long ImportedFields, string Status, IReadOnlyList<string> Notes);

/// <summary>Imports mapped Excel rows into the wiki database in one transaction.</summary>
public sealed class ExcelDatabaseImportService
{
    public ExcelDatabaseImportResult Import(string databasePath, string workbookPath, string mappingPath, string rulesPath, string? sourceTracePath = null)
    {
        var fullDb = Path.GetFullPath(databasePath);
        var fullWorkbook = Path.GetFullPath(workbookPath);
        var sourceFile = Path.GetFullPath(sourceTracePath ?? workbookPath);
        if (!File.Exists(fullDb)) throw new FileNotFoundException("找不到知識庫，請先執行 db-init。", fullDb);
        if (!File.Exists(fullWorkbook)) throw new FileNotFoundException("找不到 Excel 檔案。", fullWorkbook);

        var mappingCheck = ImportMappingService.Validate(fullWorkbook, mappingPath);
        if (!mappingCheck.IsValid) throw new InvalidDataException("欄位對應未通過驗證，已停止匯入。");
        var rulesCheck = ImportCleaningRulesService.Validate(fullWorkbook, mappingPath, rulesPath);
        if (!rulesCheck.IsValid) throw new InvalidDataException("清理規則未通過驗證，已停止匯入。");
        var dryRun = ImportDryRunService.Analyze(fullWorkbook, mappingPath, rulesPath);
        if (dryRun.RowsRequiringReview > 0)
            throw new InvalidDataException($"有 {dryRun.RowsRequiringReview} 列需要人工檢查。為避免匯入不完整資料，本次未寫入任何內容；請先處理問題並重新執行 dry-run。");

        var mapping = JsonSerializer.Deserialize<ImportMappingProfile>(File.ReadAllText(mappingPath)) ?? throw new InvalidDataException("mapping JSON 無法解析。");
        var batchId = Guid.NewGuid().ToString("N");
        var now = DateTimeOffset.UtcNow.ToString("O");
        var hash = ExcelImportPreflightService.HashFile(fullWorkbook);
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = fullDb, Mode = SqliteOpenMode.ReadWrite, Pooling = false }.ToString());
        connection.Open();
        // SQLite foreign-key enforcement is connection-local and must be enabled before
        // the transaction begins; the pre-commit guard then checks the whole pending write.
        using (var foreignKeys = connection.CreateCommand())
        {
            foreignKeys.CommandText = "PRAGMA foreign_keys=ON;";
            foreignKeys.ExecuteNonQuery();
        }
        using var tx = connection.BeginTransaction();
        EnsureSchema(connection, tx);
        using (var batch = connection.CreateCommand())
        {
            batch.Transaction = tx;
            batch.CommandText = "INSERT INTO import_batches(id,source_file,source_sha256,started_at,completed_at,status,source_row_count,imported_row_count,rejected_row_count,notes) VALUES($id,$file,$hash,$now,NULL,'Running',0,0,0,'Excel mapping import');";
            batch.Parameters.AddWithValue("$id", batchId); batch.Parameters.AddWithValue("$file", sourceFile); batch.Parameters.AddWithValue("$hash", hash); batch.Parameters.AddWithValue("$now", now); batch.ExecuteNonQuery();
        }

        long rows = 0, fields = 0;
        try
        {
            using var document = SpreadsheetDocument.Open(fullWorkbook, false);
            var wb = document.WorkbookPart ?? throw new InvalidDataException("找不到 WorkbookPart。");
            var shared = WorkbookReader.ReadSharedStrings(wb);
            var sheetParts = WorkbookReader.GetSheets(wb)
                .ToDictionary(s => s.Name?.Value ?? throw new InvalidDataException("來源活頁簿含有未命名工作表。"), StringComparer.Ordinal);
            foreach (var ms in mapping.Sheets)
            {
                if (ms.Columns.Count > 0 && ms.Columns.All(c => c.Mode.Equals("Ignore", StringComparison.OrdinalIgnoreCase)))
                    continue;
                var titleColumn = ms.Columns.SingleOrDefault(c => c.Mode.Equals("Title", StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException($"工作表「{ms.SheetName}」沒有 Title 欄位。");
                var contentColumns = ms.Columns.Where(c => c.Mode.Equals("Content", StringComparison.OrdinalIgnoreCase)).ToList();
                var attrColumns = ms.Columns.Where(c => c.Mode.Equals("Attribute", StringComparison.OrdinalIgnoreCase)).ToList();
                if (!sheetParts.TryGetValue(ms.SheetName, out var sourceSheet))
                    throw new InvalidDataException($"來源活頁簿找不到工作表「{ms.SheetName}」。");
                var part = WorkbookReader.GetWorksheetPart(wb, sourceSheet);
                using var reader = OpenXmlReader.Create(part);
                while (reader.Read())
                {
                    if (reader.ElementType != typeof(Row)) continue;
                    var row = reader.LoadCurrentElement() as Row
                        ?? throw new InvalidDataException("Open XML reader 回傳的資料列型別不正確。");
                    var rowNo = row.RowIndex?.Value ?? 0;
                    if (rowNo < ms.FirstDataRow) continue;
                    var values = new Dictionary<int, string>();
                    foreach (var cell in row.Elements<Cell>()) values[ColumnIndex(cell.CellReference?.Value)] = WorkbookReader.GetCellText(cell, shared);
                    if (values.Count == 0 || values.Values.All(string.IsNullOrWhiteSpace)) continue;
                    rows++;
                    var title = values.GetValueOrDefault(titleColumn.SourceIndex, string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(title)) throw new InvalidDataException($"{ms.SheetName}!第 {rowNo} 列標題空白；交易已回滾。");
                    var content = string.Join("\n", contentColumns.Select(c => values.GetValueOrDefault(c.SourceIndex, "").Trim()).Where(v => v.Length > 0));
                    var entryId = Guid.NewGuid().ToString("N");
                    using (var insert = connection.CreateCommand())
                    {
                        insert.Transaction = tx;
                        insert.CommandText = "INSERT INTO entries(id,title,normalized_title,entry_type,summary,created_at,updated_at,is_archived) VALUES($id,$title,$norm,$type,$summary,$now,$now,0);";
                        insert.Parameters.AddWithValue("$id", entryId); insert.Parameters.AddWithValue("$title", title); insert.Parameters.AddWithValue("$norm", title.ToUpperInvariant()); insert.Parameters.AddWithValue("$type", ms.SheetName.Trim()); insert.Parameters.AddWithValue("$summary", (object?)NullIfEmpty(content) ?? DBNull.Value); insert.Parameters.AddWithValue("$now", now); insert.ExecuteNonQuery();
                    }
                    foreach (var col in attrColumns)
                    {
                        var value = values.GetValueOrDefault(col.SourceIndex, string.Empty);
                        if (string.IsNullOrWhiteSpace(value)) continue;
                        using var field = connection.CreateCommand(); field.Transaction = tx;
                        field.CommandText = "INSERT INTO entry_fields(entry_id,field_key,field_label,field_value,value_type,sort_order,source_batch_id,source_sheet,source_row,source_column) VALUES($entry,$key,$label,$value,'Text',$order,$batch,$sheet,$row,$column);";
                        field.Parameters.AddWithValue("$entry", entryId); field.Parameters.AddWithValue("$key", col.TargetField!); field.Parameters.AddWithValue("$label", col.SourceName); field.Parameters.AddWithValue("$value", value); field.Parameters.AddWithValue("$order", fields); field.Parameters.AddWithValue("$batch", batchId); field.Parameters.AddWithValue("$sheet", ms.SheetName); field.Parameters.AddWithValue("$row", (long)rowNo); field.Parameters.AddWithValue("$column", ColumnName(col.SourceIndex)); field.ExecuteNonQuery(); fields++;
                    }
                    using (var source = connection.CreateCommand())
                    {
                        source.Transaction = tx; source.CommandText = "INSERT INTO entry_sources(entry_id,batch_id,source_file,source_sheet,source_row,source_table,imported_at) VALUES($entry,$batch,$file,$sheet,$row,NULL,$now);";
                        source.Parameters.AddWithValue("$entry", entryId); source.Parameters.AddWithValue("$batch", batchId); source.Parameters.AddWithValue("$file", sourceFile); source.Parameters.AddWithValue("$sheet", ms.SheetName); source.Parameters.AddWithValue("$row", (long)rowNo); source.Parameters.AddWithValue("$now", now); source.ExecuteNonQuery();
                    }
                    using (var fts = connection.CreateCommand())
                    {
                        fts.Transaction = tx; fts.CommandText = "INSERT INTO entries_fts(entry_id,title,summary) VALUES($id,$title,$summary);";
                        fts.Parameters.AddWithValue("$id", entryId); fts.Parameters.AddWithValue("$title", title); fts.Parameters.AddWithValue("$summary", content); fts.ExecuteNonQuery();
                    }
                }
            }
            using (var complete = connection.CreateCommand())
            {
                complete.Transaction = tx; complete.CommandText = "UPDATE import_batches SET completed_at=$now,status='Completed',source_row_count=$rows,imported_row_count=$rows,rejected_row_count=0 WHERE id=$id;";
                complete.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O")); complete.Parameters.AddWithValue("$rows", rows); complete.Parameters.AddWithValue("$id", batchId); complete.ExecuteNonQuery();
            }
            // Never commit an import until SQLite validates the pending transaction.
            new ImportTransactionGuard().ValidateBeforeCommit(connection, tx);
            tx.Commit();
        }
        catch
        {
            try { tx.Rollback(); } catch { /* Preserve the original import/validation exception. */ }
            throw;
        }
        return new ExcelDatabaseImportResult(batchId, sourceFile, rows, rows, fields, "Completed", new[] { "來源 Excel 以唯讀方式開啟。", "整批資料以單一 SQLite 交易寫入；任何錯誤均回滾。", "提交前必須通過 SQLite quick_check(1) 與 foreign_key_check。", "有啟用清理規則命中時，必須先人工處理，不會自動修正或略過資料。", "不匯入圖片檔案；不修改來源 Excel。" });
    }

    private static void EnsureSchema(SqliteConnection c, SqliteTransaction tx)
    {
        using var cmd = c.CreateCommand(); cmd.Transaction = tx; cmd.CommandText = "PRAGMA user_version;";
        var version = Convert.ToInt32(cmd.ExecuteScalar());
        if (version != WikiDatabaseService.CurrentSchemaVersion) throw new InvalidDataException($"資料庫 Schema 不相容：{version}。");
        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('import_batches','entries','entry_fields','entry_sources','entries_fts');";
        if (Convert.ToInt32(cmd.ExecuteScalar()) != 5) throw new InvalidDataException("知識庫缺少必要資料表，請先執行 db-init。");
    }
    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;
    private static int ColumnIndex(string? r) { if (string.IsNullOrWhiteSpace(r)) return 0; var n=0; foreach(var ch in r){if(!char.IsLetter(ch))break;n=n*26+char.ToUpperInvariant(ch)-'A'+1;} return n; }
    private static string ColumnName(int n) { var s=""; while(n>0){n--;s=(char)('A'+n%26)+s;n/=26;} return s; }
}
