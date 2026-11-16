using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed class EntryDetailService
{
    public object Get(string databasePath, string id)
    {
        using var connection = Open(databasePath, true);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id,title,entry_type,summary,created_at,updated_at,is_archived FROM entries WHERE id=$id;";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) throw new KeyNotFoundException($"找不到條目：{id}");
        var entry = new { Id=reader.GetString(0), Title=reader.GetString(1), EntryType=reader.GetString(2), Summary=reader.IsDBNull(3)?null:reader.GetString(3), CreatedAt=reader.GetString(4), UpdatedAt=reader.GetString(5), IsArchived=reader.GetInt32(6)==1 };
        return entry;
    }

    public object Update(string databasePath, string id, string title, string entryType, string? summary)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("條目名稱不可空白。");
        using var connection = Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE entries SET title=$title,normalized_title=$normalized,entry_type=$type,summary=$summary,updated_at=$now WHERE id=$id;";
        command.Parameters.AddWithValue("$title", title.Trim());
        command.Parameters.AddWithValue("$normalized", title.Trim().Normalize(System.Text.NormalizationForm.FormKC).ToUpperInvariant());
        command.Parameters.AddWithValue("$type", string.IsNullOrWhiteSpace(entryType)?"General":entryType.Trim());
        command.Parameters.AddWithValue("$summary", (object?)summary ?? DBNull.Value);
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$id", id);
        if (command.ExecuteNonQuery()==0) throw new KeyNotFoundException($"找不到條目：{id}");
        using var fts = connection.CreateCommand();
        fts.CommandText = "DELETE FROM entries_fts WHERE entry_id=$id; INSERT INTO entries_fts(entry_id,title,summary) SELECT id,title,COALESCE(summary,'') FROM entries WHERE id=$id;";
        fts.Parameters.AddWithValue("$id", id); fts.ExecuteNonQuery();
        return Get(databasePath,id);
    }

    public object SetArchived(string databasePath, string id, bool archived)
    {
        using var connection = Open(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE entries SET is_archived=$archived,updated_at=$now WHERE id=$id;";
        command.Parameters.AddWithValue("$archived", archived?1:0);
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$id", id);
        if(command.ExecuteNonQuery()==0) throw new KeyNotFoundException($"找不到條目：{id}");
        return Get(databasePath,id);
    }

    public object SetField(string databasePath,string entryId,string key,string label,string? value,string valueType="Text")
    {
        if(string.IsNullOrWhiteSpace(key)||string.IsNullOrWhiteSpace(label)) throw new ArgumentException("欄位 key 與 label 不可空白。");
        using var connection=Open(databasePath);
        using var command=connection.CreateCommand();
        command.CommandText=@"INSERT INTO entry_fields(entry_id,field_key,field_label,field_value,value_type)
VALUES($entry,$key,$label,$value,$type)
ON CONFLICT(entry_id,field_key) DO UPDATE SET field_label=excluded.field_label,field_value=excluded.field_value,value_type=excluded.value_type;";
        command.Parameters.AddWithValue("$entry",entryId); command.Parameters.AddWithValue("$key",key.Trim()); command.Parameters.AddWithValue("$label",label.Trim());
        command.Parameters.AddWithValue("$value",(object?)value??DBNull.Value); command.Parameters.AddWithValue("$type",string.IsNullOrWhiteSpace(valueType)?"Text":valueType.Trim());
        command.ExecuteNonQuery();
        return ListFields(databasePath,entryId);
    }

    public object ListFields(string databasePath,string entryId)
    {
        using var connection=Open(databasePath,true); using var command=connection.CreateCommand();
        command.CommandText="SELECT field_key,field_label,field_value,value_type,sort_order,source_sheet,source_row,source_column FROM entry_fields WHERE entry_id=$id ORDER BY sort_order,field_key;";
        command.Parameters.AddWithValue("$id",entryId); var fields=new List<object>(); using var reader=command.ExecuteReader();
        while(reader.Read()) fields.Add(new {Key=reader.GetString(0),Label=reader.GetString(1),Value=reader.IsDBNull(2)?null:reader.GetString(2),ValueType=reader.GetString(3),SortOrder=reader.GetInt32(4),SourceSheet=reader.IsDBNull(5)?null:reader.GetString(5),SourceRow=reader.IsDBNull(6)?(int?)null:reader.GetInt32(6),SourceColumn=reader.IsDBNull(7)?null:reader.GetString(7)});
        return fields;
    }


    public object ListSheetLayout(string databasePath, string sheetName)
    {
        using var connection = Open(databasePath, true);
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT field_key, field_label, MIN(sort_order) AS first_order
FROM entry_fields
WHERE source_sheet=$sheet AND source_sheet IS NOT NULL
GROUP BY field_key, field_label
ORDER BY first_order, field_key;";
        command.Parameters.AddWithValue("$sheet", sheetName);
        using var reader = command.ExecuteReader();
        var fields = new List<object>();
        while (reader.Read())
            fields.Add(new { Key = reader.GetString(0), Label = reader.GetString(1), SortOrder = reader.GetInt32(2) });
        return fields;
    }

    private static SqliteConnection Open(string path,bool readOnly=false)
    {
        var full=Path.GetFullPath(path); if(!File.Exists(full)) throw new FileNotFoundException("找不到知識庫，請先執行 db-init。",full);
        var c=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=full,Mode=readOnly?SqliteOpenMode.ReadOnly:SqliteOpenMode.ReadWrite,Pooling=false}.ToString()); c.Open();
        using var p=c.CreateCommand(); p.CommandText="PRAGMA foreign_keys=ON;"; p.ExecuteNonQuery(); return c;
    }
}
