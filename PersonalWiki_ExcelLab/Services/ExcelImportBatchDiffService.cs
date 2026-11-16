using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed record BatchFieldSnapshot(string FieldKey, string FieldLabel, string? Value, string ValueType, string? SourceSheet, long? SourceRow, string? SourceColumn);
public sealed record BatchFieldChange(string FieldKey, string ChangeType, BatchFieldSnapshot? Before, BatchFieldSnapshot? After);
public sealed record BatchEntrySnapshot(string EntryId, string Title, string EntryType, string? Summary, string SourceSheet, long SourceRow, IReadOnlyList<BatchFieldSnapshot> Fields);
public sealed record BatchEntryChange(string Key, string ChangeType, BatchEntrySnapshot? Before, BatchEntrySnapshot? After, IReadOnlyList<string> ChangedFields, IReadOnlyList<BatchFieldChange> FieldChanges);
public sealed record BatchFieldDiffSummary(string FieldKey, string FieldLabel, long AddedCount, long RemovedCount, long ChangedCount);
public sealed record ExcelImportBatchDiffResult(string EarlierBatchId, string LaterBatchId, long EarlierCount, long LaterCount, long AddedCount, long RemovedCount, long ChangedCount, long AddedFieldCount, long RemovedFieldCount, long ChangedFieldCount, IReadOnlyList<BatchFieldDiffSummary> FieldSummaries, IReadOnlyList<BatchEntryChange> Changes);

/// <summary>Read-only comparison of two Excel import batches, including mapped custom fields.</summary>
public sealed class ExcelImportBatchDiffService
{
    public ExcelImportBatchDiffResult Compare(string databasePath, string earlierBatchId, string laterBatchId)
    {
        if (string.Equals(earlierBatchId, laterBatchId, StringComparison.Ordinal))
            throw new ArgumentException("比較批次不可相同。");
        if (!File.Exists(databasePath)) throw new FileNotFoundException("找不到知識庫。", databasePath);
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path.GetFullPath(databasePath), Mode = SqliteOpenMode.ReadOnly }.ToString());
        connection.Open();
        EnsureBatch(connection, earlierBatchId);
        EnsureBatch(connection, laterBatchId);
        var before = Load(connection, earlierBatchId);
        var after = Load(connection, laterBatchId);
        var changes = new List<BatchEntryChange>();
        foreach (var key in before.Keys.Union(after.Keys).OrderBy(x => x, StringComparer.Ordinal))
        {
            before.TryGetValue(key, out var oldItem);
            after.TryGetValue(key, out var newItem);
            if (oldItem is null)
                changes.Add(new BatchEntryChange(key, "Added", null, newItem, Array.Empty<string>(), Array.Empty<BatchFieldChange>()));
            else if (newItem is null)
                changes.Add(new BatchEntryChange(key, "Removed", oldItem, null, Array.Empty<string>(), Array.Empty<BatchFieldChange>()));
            else
            {
                var changed = new List<string>();
                if (!string.Equals(oldItem.Title, newItem.Title, StringComparison.Ordinal)) changed.Add("Title");
                if (!string.Equals(oldItem.EntryType, newItem.EntryType, StringComparison.Ordinal)) changed.Add("EntryType");
                if (!string.Equals(oldItem.Summary, newItem.Summary, StringComparison.Ordinal)) changed.Add("Summary");
                var fieldChanges = CompareFields(oldItem.Fields, newItem.Fields);
                if (fieldChanges.Count > 0) changed.Add("Fields");
                if (changed.Count > 0) changes.Add(new BatchEntryChange(key, "Changed", oldItem, newItem, changed, fieldChanges));
            }
        }
        var allFieldChanges = changes.SelectMany(x => x.FieldChanges).ToList();
        var fieldSummaries = allFieldChanges.GroupBy(x => x.FieldKey, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new BatchFieldDiffSummary(
                g.Key,
                g.Select(x => x.After?.FieldLabel ?? x.Before?.FieldLabel ?? g.Key).First(),
                g.LongCount(x => x.ChangeType == "Added"),
                g.LongCount(x => x.ChangeType == "Removed"),
                g.LongCount(x => x.ChangeType == "Changed")))
            .ToList();
        return new ExcelImportBatchDiffResult(earlierBatchId, laterBatchId, before.Count, after.Count,
            changes.LongCount(x => x.ChangeType == "Added"), changes.LongCount(x => x.ChangeType == "Removed"),
            changes.LongCount(x => x.ChangeType == "Changed"), allFieldChanges.LongCount(x => x.ChangeType == "Added"),
            allFieldChanges.LongCount(x => x.ChangeType == "Removed"), allFieldChanges.LongCount(x => x.ChangeType == "Changed"), fieldSummaries, changes);
    }

    private static List<BatchFieldChange> CompareFields(IReadOnlyList<BatchFieldSnapshot> before, IReadOnlyList<BatchFieldSnapshot> after)
    {
        var oldFields = before.ToDictionary(x => x.FieldKey, StringComparer.Ordinal);
        var newFields = after.ToDictionary(x => x.FieldKey, StringComparer.Ordinal);
        var result = new List<BatchFieldChange>();
        foreach (var key in oldFields.Keys.Union(newFields.Keys).OrderBy(x => x, StringComparer.Ordinal))
        {
            oldFields.TryGetValue(key, out var oldField);
            newFields.TryGetValue(key, out var newField);
            if (oldField is null) result.Add(new BatchFieldChange(key, "Added", null, newField));
            else if (newField is null) result.Add(new BatchFieldChange(key, "Removed", oldField, null));
            else if (!string.Equals(oldField.FieldLabel, newField.FieldLabel, StringComparison.Ordinal)
                || !string.Equals(oldField.Value, newField.Value, StringComparison.Ordinal)
                || !string.Equals(oldField.ValueType, newField.ValueType, StringComparison.Ordinal))
                result.Add(new BatchFieldChange(key, "Changed", oldField, newField));
        }
        return result;
    }

    private static void EnsureBatch(SqliteConnection connection, string id)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM import_batches WHERE id=$id;";
        command.Parameters.AddWithValue("$id", id);
        if (command.ExecuteScalar() is null) throw new InvalidDataException($"找不到匯入批次：{id}");
    }

    private static Dictionary<string, BatchEntrySnapshot> Load(SqliteConnection connection, string batchId)
    {
        var result = new Dictionary<string, BatchEntrySnapshot>(StringComparer.Ordinal);
        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT e.id,e.title,e.entry_type,e.summary,s.source_sheet,s.source_row,e.normalized_title
                FROM entry_sources s JOIN entries e ON e.id=s.entry_id
                WHERE s.batch_id=$id
                ORDER BY e.normalized_title,e.entry_type,s.source_sheet,s.source_row;
                """;
            command.Parameters.AddWithValue("$id", batchId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var key = reader.GetString(6) + "\u001f" + reader.GetString(2);
                if (!result.ContainsKey(key))
                    result.Add(key, new BatchEntrySnapshot(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.GetInt64(5), Array.Empty<BatchFieldSnapshot>()));
            }
        }
        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT f.entry_id,f.field_key,f.field_label,f.field_value,f.value_type,f.source_sheet,f.source_row,f.source_column
                FROM entry_fields f WHERE f.source_batch_id=$id
                ORDER BY f.entry_id,f.field_key;
                """;
            command.Parameters.AddWithValue("$id", batchId);
            using var reader = command.ExecuteReader();
            var fields = new Dictionary<string, List<BatchFieldSnapshot>>(StringComparer.Ordinal);
            while (reader.Read())
            {
                var entryId = reader.GetString(0);
                if (!fields.TryGetValue(entryId, out var list)) fields[entryId] = list = new List<BatchFieldSnapshot>();
                list.Add(new BatchFieldSnapshot(reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetInt64(6), reader.IsDBNull(7) ? null : reader.GetString(7)));
            }
            foreach (var item in result.ToArray())
                if (fields.TryGetValue(item.Value.EntryId, out var list)) result[item.Key] = item.Value with { Fields = list };
        }
        return result;
    }
}
