using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ImportTransactionValidationResult(
    bool QuickCheckPassed,
    int ForeignKeyIssueCount,
    IReadOnlyList<string> QuickCheckResults,
    IReadOnlyList<string> Issues);

/// <summary>Validates the uncommitted import transaction before it is allowed to commit.</summary>
public sealed class ImportTransactionGuard
{
    public ImportTransactionValidationResult ValidateBeforeCommit(SqliteConnection connection, SqliteTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);

        var quickCheck = new List<string>();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "PRAGMA quick_check(1);";
            using var reader = command.ExecuteReader();
            while (reader.Read()) quickCheck.Add(reader.GetString(0));
        }

        var foreignKeyIssues = new List<string>();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "PRAGMA foreign_key_check;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var table = reader.IsDBNull(0) ? "?" : reader.GetString(0);
                var rowId = reader.IsDBNull(1) ? "NULL" : reader.GetValue(1).ToString();
                var parent = reader.IsDBNull(2) ? "?" : reader.GetString(2);
                foreignKeyIssues.Add($"{table}:{rowId}->{parent}");
            }
        }

        var quickCheckPassed = quickCheck.Count == 1
            && string.Equals(quickCheck[0], "ok", StringComparison.OrdinalIgnoreCase);
        var issues = new List<string>();
        if (!quickCheckPassed) issues.Add("SQLite quick_check(1) 未通過：" + string.Join("；", quickCheck));
        if (foreignKeyIssues.Count > 0) issues.Add($"SQLite foreign_key_check 發現 {foreignKeyIssues.Count} 筆違規。");

        var result = new ImportTransactionValidationResult(
            quickCheckPassed, foreignKeyIssues.Count, quickCheck, issues);
        if (issues.Count > 0)
            throw new InvalidDataException("匯入交易提交前驗證失敗，交易將回滾。" + string.Join(" ", issues));
        return result;
    }
}
