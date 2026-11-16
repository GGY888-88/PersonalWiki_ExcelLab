using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record NoteAuditFinding(string Category, string Severity, string NoteId,
    string EntryId, string EntryTitle, string Detail);

public sealed record DatabaseNoteAuditReport(string Database, DateTimeOffset CreatedAtUtc,
    int EntryCount, int NoteCount, int BlankNoteCount, int InvalidTimestampCount,
    int DuplicateNoteCandidateCount, IReadOnlyList<NoteAuditFinding> Findings, string ReportPath);

/// <summary>Creates a read-only quality audit of note records without exporting note text.</summary>
public sealed class DatabaseNoteAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseNoteAuditReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("筆記稽核報告已存在；為避免覆蓋已停止輸出。");

        var findings = new List<NoteAuditFinding>();
        var notes = new List<(string Id, string EntryId, string EntryTitle, string Text, string Created, string Updated)>();
        var entryIds = new HashSet<string>(StringComparer.Ordinal);
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT n.id, n.entry_id, e.title, n.note_text, n.created_at, n.updated_at
                FROM entry_notes n INNER JOIN entries e ON e.id = n.entry_id
                ORDER BY e.title COLLATE NOCASE, n.created_at, n.id;
                """;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var note = (reader.GetString(0), reader.GetString(1), reader.GetString(2),
                    reader.GetString(3), reader.GetString(4), reader.GetString(5));
                notes.Add(note);
                entryIds.Add(note.Item2);
                if (string.IsNullOrWhiteSpace(note.Item4))
                    findings.Add(new NoteAuditFinding("BlankNote", "Warning", note.Item1, note.Item2,
                        note.Item3, "筆記內容為空白或只有空白字元。"));
                if (!DateTimeOffset.TryParse(note.Item5, out _) || !DateTimeOffset.TryParse(note.Item6, out _))
                    findings.Add(new NoteAuditFinding("InvalidTimestamp", "Warning", note.Item1, note.Item2,
                        note.Item3, "建立或更新時間無法解析為日期時間。"));
            }
        }

        var duplicateCandidates = 0;
        foreach (var group in notes.Where(x => !string.IsNullOrWhiteSpace(x.Text))
                     .GroupBy(x => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(x.Text))), StringComparer.Ordinal)
                     .Where(g => g.Count() > 1))
        {
            var members = group.ToList();
            duplicateCandidates += members.Count;
            foreach (var note in members)
                findings.Add(new NoteAuditFinding("DuplicateNoteTextCandidate", "Review", note.Id,
                    note.EntryId, note.EntryTitle,
                    $"有 {members.Count} 筆筆記文字完全相同（以 SHA-256 分組）；僅供人工複核，不代表應刪除。"));
        }

        var result = new DatabaseNoteAuditReport(database, DateTimeOffset.UtcNow, entryIds.Count,
            notes.Count, findings.Count(x => x.Category == "BlankNote"),
            findings.Count(x => x.Category == "InvalidTimestamp"), duplicateCandidates, findings, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
