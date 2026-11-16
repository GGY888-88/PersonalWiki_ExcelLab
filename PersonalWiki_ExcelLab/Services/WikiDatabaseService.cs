using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed class WikiDatabaseService
{
    public const int CurrentSchemaVersion = 1;

    public object Initialize(string databasePath)
    {
        var fullPath = Path.GetFullPath(databasePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false
        }.ToString());
        connection.Open();
        using (var pragmas = connection.CreateCommand())
        {
            pragmas.CommandText = "PRAGMA foreign_keys=ON; PRAGMA journal_mode=WAL; PRAGMA synchronous=FULL;";
            pragmas.ExecuteNonQuery();
        }

        using var transaction = connection.BeginTransaction();
        var existingVersion = ReadUserVersion(connection, transaction);
        if (existingVersion > CurrentSchemaVersion)
            throw new InvalidOperationException($"資料庫版本 {existingVersion} 高於程式支援版本 {CurrentSchemaVersion}，為避免破壞資料已停止。 ");
        if (existingVersion == 0)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = SchemaV1;
            command.ExecuteNonQuery();
        }
        transaction.Commit();
        return Inspect(databasePath);
    }

    public object Inspect(string databasePath)
    {
        var fullPath = Path.GetFullPath(databasePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("找不到資料庫，請先執行 db-init。", fullPath);
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString());
        connection.Open();
        var version = ReadUserVersion(connection, null);
        var tables = new List<object>();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.GetString(0);
            tables.Add(new { Name = name, RowCount = CountRows(connection, name) });
        }
        return new { Database = fullPath, SchemaVersion = version, SupportedSchemaVersion = CurrentSchemaVersion, Tables = tables };
    }

    private static int ReadUserVersion(SqliteConnection connection, SqliteTransaction? transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "PRAGMA user_version;";
        return Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static long CountRows(SqliteConnection connection, string tableName)
    {
        // Table names originate only from sqlite_master, then are quoted as identifiers.
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM \"{tableName.Replace("\"", "\"\"", StringComparison.Ordinal)}\";";
        return Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private const string SchemaV1 = @"
CREATE TABLE schema_migrations (
    version INTEGER PRIMARY KEY,
    applied_at TEXT NOT NULL,
    description TEXT NOT NULL
);
INSERT INTO schema_migrations(version, applied_at, description)
VALUES (1, strftime('%Y-%m-%dT%H:%M:%fZ','now'), 'Initial personal wiki data model');

CREATE TABLE import_batches (
    id TEXT PRIMARY KEY,
    source_file TEXT NOT NULL,
    source_sha256 TEXT NOT NULL,
    started_at TEXT NOT NULL,
    completed_at TEXT,
    status TEXT NOT NULL CHECK(status IN ('Planned','Running','Completed','Failed','Cancelled')),
    source_row_count INTEGER NOT NULL DEFAULT 0 CHECK(source_row_count >= 0),
    imported_row_count INTEGER NOT NULL DEFAULT 0 CHECK(imported_row_count >= 0),
    rejected_row_count INTEGER NOT NULL DEFAULT 0 CHECK(rejected_row_count >= 0),
    notes TEXT
);

CREATE TABLE entries (
    id TEXT PRIMARY KEY,
    title TEXT NOT NULL CHECK(length(trim(title)) > 0),
    normalized_title TEXT NOT NULL,
    entry_type TEXT NOT NULL DEFAULT 'General',
    summary TEXT,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    is_archived INTEGER NOT NULL DEFAULT 0 CHECK(is_archived IN (0,1)),
    UNIQUE(normalized_title, entry_type)
);
CREATE INDEX idx_entries_title ON entries(normalized_title);
CREATE INDEX idx_entries_type_updated ON entries(entry_type, updated_at DESC);

CREATE TABLE entry_fields (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    entry_id TEXT NOT NULL REFERENCES entries(id) ON DELETE CASCADE,
    field_key TEXT NOT NULL,
    field_label TEXT NOT NULL,
    field_value TEXT,
    value_type TEXT NOT NULL DEFAULT 'Text',
    sort_order INTEGER NOT NULL DEFAULT 0,
    source_batch_id TEXT REFERENCES import_batches(id) ON DELETE SET NULL,
    source_sheet TEXT,
    source_row INTEGER CHECK(source_row IS NULL OR source_row > 0),
    source_column TEXT,
    UNIQUE(entry_id, field_key)
);
CREATE INDEX idx_entry_fields_key ON entry_fields(field_key);
CREATE INDEX idx_entry_fields_source ON entry_fields(source_batch_id, source_sheet, source_row);

CREATE TABLE entry_notes (
    id TEXT PRIMARY KEY,
    entry_id TEXT NOT NULL REFERENCES entries(id) ON DELETE CASCADE,
    note_text TEXT NOT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);
CREATE INDEX idx_entry_notes_entry ON entry_notes(entry_id, updated_at DESC);

CREATE TABLE entry_images (
    id TEXT PRIMARY KEY,
    entry_id TEXT NOT NULL REFERENCES entries(id) ON DELETE CASCADE,
    file_path TEXT NOT NULL,
    original_name TEXT,
    mime_type TEXT,
    sha256 TEXT,
    caption TEXT,
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL,
    UNIQUE(entry_id, file_path)
);
CREATE INDEX idx_entry_images_entry ON entry_images(entry_id, sort_order);

CREATE TABLE entry_sources (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    entry_id TEXT NOT NULL REFERENCES entries(id) ON DELETE CASCADE,
    batch_id TEXT REFERENCES import_batches(id) ON DELETE SET NULL,
    source_file TEXT NOT NULL,
    source_sheet TEXT NOT NULL,
    source_row INTEGER NOT NULL CHECK(source_row > 0),
    source_table TEXT,
    imported_at TEXT NOT NULL,
    UNIQUE(entry_id, source_file, source_sheet, source_row)
);
CREATE INDEX idx_entry_sources_origin ON entry_sources(source_file, source_sheet, source_row);

CREATE TABLE entry_tags (
    entry_id TEXT NOT NULL REFERENCES entries(id) ON DELETE CASCADE,
    tag TEXT NOT NULL,
    normalized_tag TEXT NOT NULL,
    PRIMARY KEY(entry_id, normalized_tag)
);
CREATE INDEX idx_entry_tags_normalized ON entry_tags(normalized_tag);

CREATE VIRTUAL TABLE entries_fts USING fts5(entry_id UNINDEXED, title, summary, content='');
PRAGMA user_version=1;
";
}
