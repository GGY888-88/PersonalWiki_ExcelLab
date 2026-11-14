using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ImportBatchAuditFinding(string Category, string Severity, string BatchId, string Detail);
public sealed record ImportBatchAuditReport(string Database, DateTimeOffset CreatedAtUtc,
    int BatchCount, int CompletedCount, int FailedCount, int RunningCount, int CancelledCount,
    int PlannedCount, int CounterMismatchCount, int TimestampIssueCount,
    IReadOnlyList<ImportBatchAuditFinding> Findings, string ReportPath);

/// <summary>Creates a read-only audit of import batch metadata without exposing source paths or notes.</summary>
public sealed class DatabaseImportBatchAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ImportBatchAuditReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("匯入批次稽核報告已存在；為避免覆蓋已停止輸出。");

        var findings = new List<ImportBatchAuditFinding>();
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        { ["Completed"] = 0, ["Failed"] = 0, ["Running"] = 0, ["Cancelled"] = 0, ["Planned"] = 0 };
        var counterIssues = 0;
        var timestampIssues = 0;

        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT id, status, source_row_count, imported_row_count, rejected_row_count,
                       started_at, completed_at
                FROM import_batches ORDER BY started_at, id;
                """;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetString(0);
                var status = reader.GetString(1);
                var sourceRows = reader.GetInt32(2);
                var importedRows = reader.GetInt32(3);
                var rejectedRows = reader.GetInt32(4);
                var startedAt = reader.GetString(5);
                var completedAt = reader.IsDBNull(6) ? null : reader.GetString(6);
                if (counts.ContainsKey(status)) counts[status]++;
                else findings.Add(new ImportBatchAuditFinding("UnknownStatus", "Warning", id,
                    "批次狀態不屬於目前支援的狀態集合。"));

                if (sourceRows < 0 || importedRows < 0 || rejectedRows < 0 ||
                    importedRows + rejectedRows > sourceRows)
                {
                    counterIssues++;
                    findings.Add(new ImportBatchAuditFinding("CounterMismatch", "Warning", id,
                        "來源、匯入及拒絕筆數不符合合理範圍，請人工核對。"));
                }

                var startedValid = DateTimeOffset.TryParse(startedAt, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out _);
                var completedValid = completedAt is null || DateTimeOffset.TryParse(completedAt,
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out _);
                if (!startedValid || !completedValid)
                {
                    timestampIssues++;
                    findings.Add(new ImportBatchAuditFinding("InvalidTimestamp", "Warning", id,
                        "批次開始或完成時間無法解析。"));
                }
                if (status == "Completed" && completedAt is null)
                    findings.Add(new ImportBatchAuditFinding("MissingCompletionTime", "Review", id,
                        "已完成批次缺少完成時間。"));
                if (status is "Planned" or "Running" && completedAt is not null)
                    findings.Add(new ImportBatchAuditFinding("UnexpectedCompletionTime", "Review", id,
                        "尚未完成的批次卻已有完成時間。"));
            }
        }

        var result = new ImportBatchAuditReport(database, DateTimeOffset.UtcNow, counts.Values.Sum(),
            counts["Completed"], counts["Failed"], counts["Running"], counts["Cancelled"], counts["Planned"],
            counterIssues, timestampIssues, findings, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
