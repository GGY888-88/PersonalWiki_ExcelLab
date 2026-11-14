using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ExcelImportBatchSummary(string Id, string SourceFile, string SourceSha256, string StartedAt, string? CompletedAt, string Status, long SourceRows, long ImportedRows, long RejectedRows, string? Notes);
public sealed record ExcelImportBatchReport(ExcelImportBatchSummary Batch, long EntryCount, long FieldCount, long NoteCount, long ImageCount, long TagCount, long SourceCount, long MissingFtsCount, long ForeignKeyViolations);
public sealed record ExcelImportBatchEntry(string EntryId, string Title, string EntryType, string? Summary, string SourceSheet, long SourceRow, string ImportedAt);
public sealed record EntrySourceTrace(long SourceId, string EntryId, string EntryTitle, string BatchId, string SourceFile, string SourceSheet, long SourceRow, string? SourceTable, string ImportedAt);

/// <summary>Read-only history and report queries for Excel import batches.</summary>
public sealed class ExcelImportBatchManagementService
{
    public IReadOnlyList<ExcelImportBatchSummary> List(string databasePath, int limit = 50)
    {
        if (limit is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(limit), "筆數必須介於 1 到 500。");
        using var connection = Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id,source_file,source_sha256,started_at,completed_at,status,source_row_count,imported_row_count,rejected_row_count,notes FROM import_batches ORDER BY started_at DESC LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", limit);
        using var reader = command.ExecuteReader();
        var result = new List<ExcelImportBatchSummary>();
        while (reader.Read()) result.Add(ReadBatch(reader));
        return result;
    }

    public IReadOnlyList<ExcelImportBatchEntry> ListEntries(string databasePath, string batchId, int page = 1, int pageSize = 50)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "頁碼必須大於 0。");
        if (pageSize is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(pageSize), "每頁筆數必須介於 1 到 200。");
        using var connection = Open(databasePath);
        using var exists = connection.CreateCommand();
        exists.CommandText = "SELECT 1 FROM import_batches WHERE id=$id;";
        exists.Parameters.AddWithValue("$id", batchId);
        if (exists.ExecuteScalar() is null) throw new InvalidDataException("找不到指定的匯入批次。");
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT e.id,e.title,e.entry_type,e.summary,s.source_sheet,s.source_row,s.imported_at
            FROM entry_sources s JOIN entries e ON e.id=s.entry_id
            WHERE s.batch_id=$id
            ORDER BY s.source_sheet,s.source_row,e.title
            LIMIT $limit OFFSET $offset;
            """;
        command.Parameters.AddWithValue("$id", batchId);
        command.Parameters.AddWithValue("$limit", pageSize);
        command.Parameters.AddWithValue("$offset", checked((page - 1) * pageSize));
        using var reader = command.ExecuteReader();
        var result = new List<ExcelImportBatchEntry>();
        while (reader.Read()) result.Add(new ExcelImportBatchEntry(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.GetInt64(5), reader.GetString(6)));
        return result;
    }

    public IReadOnlyList<EntrySourceTrace> ListEntrySources(string databasePath, string entryId)
    {
        using var connection = Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT s.id,s.entry_id,e.title,COALESCE(s.batch_id,''),s.source_file,s.source_sheet,s.source_row,s.source_table,s.imported_at
            FROM entry_sources s JOIN entries e ON e.id=s.entry_id
            WHERE s.entry_id=$id
            ORDER BY s.imported_at DESC,s.id DESC;
            """;
        command.Parameters.AddWithValue("$id", entryId);
        using var reader = command.ExecuteReader();
        var result = new List<EntrySourceTrace>();
        while (reader.Read()) result.Add(new EntrySourceTrace(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt64(6), reader.IsDBNull(7) ? null : reader.GetString(7), reader.GetString(8)));
        return result;
    }

    public ExcelImportBatchReport Report(string databasePath, string batchId)
    {
        using var connection = Open(databasePath);
        ExcelImportBatchSummary batch;
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT id,source_file,source_sha256,started_at,completed_at,status,source_row_count,imported_row_count,rejected_row_count,notes FROM import_batches WHERE id=$id;";
            command.Parameters.AddWithValue("$id", batchId);
            using var reader = command.ExecuteReader();
            if (!reader.Read()) throw new InvalidDataException("找不到指定的匯入批次。");
            batch = ReadBatch(reader);
        }
        long Count(string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("$id", batchId);
            return Convert.ToInt64(command.ExecuteScalar());
        }
        var entries = Count("SELECT COUNT(DISTINCT entry_id) FROM entry_sources WHERE batch_id=$id;");
        var fields = Count("SELECT COUNT(*) FROM entry_fields WHERE source_batch_id=$id;");
        var notes = Count("SELECT COUNT(*) FROM entry_notes n JOIN entry_sources s ON s.entry_id=n.entry_id WHERE s.batch_id=$id;");
        var images = Count("SELECT COUNT(*) FROM entry_images i JOIN entry_sources s ON s.entry_id=i.entry_id WHERE s.batch_id=$id;");
        var tags = Count("SELECT COUNT(*) FROM entry_tags t JOIN entry_sources s ON s.entry_id=t.entry_id WHERE s.batch_id=$id;");
        var sources = Count("SELECT COUNT(*) FROM entry_sources WHERE batch_id=$id;");
        var missingFts = Count("SELECT COUNT(*) FROM entry_sources s WHERE s.batch_id=$id AND NOT EXISTS(SELECT 1 FROM entries_fts f WHERE f.entry_id=s.entry_id);");
        long fk = 0;
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA foreign_key_check;";
            using var reader = command.ExecuteReader();
            while (reader.Read()) fk++;
        }
        return new ExcelImportBatchReport(batch, entries, fields, notes, images, tags, sources, missingFts, fk);
    }

    private static SqliteConnection Open(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("找不到知識庫。", path);
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path.GetFullPath(path), Mode = SqliteOpenMode.ReadOnly }.ToString());
        connection.Open();
        return connection;
    }

    private static ExcelImportBatchSummary ReadBatch(SqliteDataReader reader) => new(
        reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
        reader.IsDBNull(4) ? null : reader.GetString(4), reader.GetString(5),
        reader.GetInt64(6), reader.GetInt64(7), reader.GetInt64(8),
        reader.IsDBNull(9) ? null : reader.GetString(9));
}
