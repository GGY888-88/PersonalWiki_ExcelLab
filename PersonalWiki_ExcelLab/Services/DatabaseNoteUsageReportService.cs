using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record NoteUsageBand(string UsageRange, int EntryCount);
public sealed record NoteLengthBand(string LengthRange, int NoteCount);
public sealed record DatabaseNoteUsageReport(
    string Database,
    DateTimeOffset CreatedAtUtc,
    int TotalNotes,
    int EntriesWithNotes,
    int EntriesWithoutNotes,
    int BlankNotes,
    int DistinctNoteTextCount,
    IReadOnlyList<NoteUsageBand> NotesPerEntry,
    IReadOnlyList<NoteLengthBand> NoteLengthDistribution,
    string ReportPath);

/// <summary>Creates a read-only aggregate note-usage report without exporting note text.</summary>
public sealed class DatabaseNoteUsageReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseNoteUsageReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase))
            throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("筆記使用統計報告已存在；為避免覆蓋已停止輸出。");

        int totalNotes;
        int entriesWithNotes;
        int blankNotes;
        int distinctText;
        int totalEntries;
        var notesPerEntry = new List<int>();
        var lengths = new List<int>();
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*), COUNT(DISTINCT entry_id), SUM(CASE WHEN trim(note_text) = '' THEN 1 ELSE 0 END), COUNT(DISTINCT note_text) FROM entry_notes;";
                using var reader = command.ExecuteReader();
                if (!reader.Read()) throw new InvalidDataException("無法讀取筆記統計資料。");
                totalNotes = reader.GetInt32(0);
                entriesWithNotes = reader.GetInt32(1);
                blankNotes = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                distinctText = reader.GetInt32(3);
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM entries;";
                totalEntries = Convert.ToInt32(command.ExecuteScalar());
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT note_count FROM (SELECT entry_id, COUNT(*) AS note_count FROM entry_notes GROUP BY entry_id);";
                using var reader = command.ExecuteReader();
                while (reader.Read()) notesPerEntry.Add(reader.GetInt32(0));
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT length(note_text) FROM entry_notes;";
                using var reader = command.ExecuteReader();
                while (reader.Read()) lengths.Add(reader.GetInt32(0));
            }
        }

        var usage = new[]
        {
            new NoteUsageBand("1 則筆記", notesPerEntry.Count(x => x == 1)),
            new NoteUsageBand("2–5 則筆記", notesPerEntry.Count(x => x is >= 2 and <= 5)),
            new NoteUsageBand("6–20 則筆記", notesPerEntry.Count(x => x is >= 6 and <= 20)),
            new NoteUsageBand("21 則以上筆記", notesPerEntry.Count(x => x >= 21))
        };
        var lengthDistribution = new[]
        {
            new NoteLengthBand("0 字", lengths.Count(x => x == 0)),
            new NoteLengthBand("1–100 字", lengths.Count(x => x is >= 1 and <= 100)),
            new NoteLengthBand("101–500 字", lengths.Count(x => x is >= 101 and <= 500)),
            new NoteLengthBand("501–2000 字", lengths.Count(x => x is >= 501 and <= 2000)),
            new NoteLengthBand("2001 字以上", lengths.Count(x => x >= 2001))
        };
        var result = new DatabaseNoteUsageReport(database, DateTimeOffset.UtcNow, totalNotes,
            entriesWithNotes, Math.Max(0, totalEntries - entriesWithNotes), blankNotes, distinctText,
            usage, lengthDistribution, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
