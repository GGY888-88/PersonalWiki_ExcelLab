using Microsoft.Data.Sqlite;
using System.Security.Cryptography;

namespace PersonalWiki.ExcelLab.Services;

public sealed class EntryContentService
{
    public object AddNote(string db, string entryId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("筆記內容不可空白。");
        using var c = Open(db); EnsureEntry(c, entryId);
        var id = Guid.NewGuid().ToString("N"); var now = DateTimeOffset.UtcNow.ToString("O");
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO entry_notes(id,entry_id,note_text,created_at,updated_at) VALUES($id,$entry,$text,$now,$now);";
        cmd.Parameters.AddWithValue("$id", id); cmd.Parameters.AddWithValue("$entry", entryId); cmd.Parameters.AddWithValue("$text", text); cmd.Parameters.AddWithValue("$now", now); cmd.ExecuteNonQuery();
        return new { Id = id, EntryId = entryId, Note = text, CreatedAt = now };
    }

    public object ListNotes(string db, string entryId)
    {
        using var c = Open(db, true); EnsureEntry(c, entryId);
        using var cmd = c.CreateCommand(); cmd.CommandText = "SELECT id,note_text,created_at,updated_at FROM entry_notes WHERE entry_id=$id ORDER BY created_at DESC;"; cmd.Parameters.AddWithValue("$id", entryId);
        var rows = new List<object>(); using var r = cmd.ExecuteReader(); while (r.Read()) rows.Add(new { Id=r.GetString(0), Text=r.GetString(1), CreatedAt=r.GetString(2), UpdatedAt=r.GetString(3) }); return rows;
    }

    public object UpdateNote(string db, string noteId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("筆記內容不可空白。");
        using var c = Open(db); using var cmd = c.CreateCommand(); cmd.CommandText = "UPDATE entry_notes SET note_text=$text,updated_at=$now WHERE id=$id;";
        cmd.Parameters.AddWithValue("$text",text); cmd.Parameters.AddWithValue("$now",DateTimeOffset.UtcNow.ToString("O")); cmd.Parameters.AddWithValue("$id",noteId);
        if(cmd.ExecuteNonQuery()==0) throw new KeyNotFoundException($"找不到筆記：{noteId}"); return new { Id=noteId, Updated=true };
    }

    public object DeleteNote(string db, string noteId)
    {
        using var c=Open(db); using var cmd=c.CreateCommand(); cmd.CommandText="DELETE FROM entry_notes WHERE id=$id;"; cmd.Parameters.AddWithValue("$id",noteId);
        if(cmd.ExecuteNonQuery()==0) throw new KeyNotFoundException($"找不到筆記：{noteId}"); return new { Id=noteId, Deleted=true };
    }

    public object AddImage(string db, string entryId, string path, string? caption)
    {
        var full=Path.GetFullPath(path); if(!File.Exists(full)) throw new FileNotFoundException("找不到圖片檔案。",full);
        using var c=Open(db); EnsureEntry(c,entryId);
        string hash; using(var stream=File.OpenRead(full)) hash=Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        var id=Guid.NewGuid().ToString("N"); var now=DateTimeOffset.UtcNow.ToString("O"); var mime=MimeFromExtension(Path.GetExtension(full));
        string? existingId;
        using (var find=c.CreateCommand())
        {
            find.CommandText="SELECT id FROM entry_images WHERE entry_id=$entry AND file_path=$path LIMIT 1;";
            find.Parameters.AddWithValue("$entry",entryId); find.Parameters.AddWithValue("$path",full);
            existingId=find.ExecuteScalar()?.ToString();
        }
        using var cmd=c.CreateCommand();
        cmd.CommandText="""
INSERT INTO entry_images(id,entry_id,file_path,original_name,mime_type,sha256,caption,created_at)
VALUES($id,$entry,$path,$name,$mime,$hash,$caption,$now)
ON CONFLICT(entry_id,file_path) DO UPDATE SET caption=CASE WHEN excluded.caption IS NOT NULL AND trim(excluded.caption)<>'' THEN excluded.caption ELSE entry_images.caption END;
""";
        cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$entry",entryId);cmd.Parameters.AddWithValue("$path",full);cmd.Parameters.AddWithValue("$name",Path.GetFileName(full));cmd.Parameters.AddWithValue("$mime",(object?)mime??DBNull.Value);cmd.Parameters.AddWithValue("$hash",hash);cmd.Parameters.AddWithValue("$caption",(object?)caption??DBNull.Value);cmd.Parameters.AddWithValue("$now",now);cmd.ExecuteNonQuery();
        string savedId;
        using (var findSaved=c.CreateCommand())
        {
            findSaved.CommandText="SELECT id FROM entry_images WHERE entry_id=$entry AND file_path=$path LIMIT 1;";
            findSaved.Parameters.AddWithValue("$entry",entryId); findSaved.Parameters.AddWithValue("$path",full);
            savedId=findSaved.ExecuteScalar()?.ToString() ?? id;
        }
        return new { Id=savedId,EntryId=entryId,Path=full,Sha256=hash,MimeType=mime,Caption=caption,AlreadyExists=existingId is not null };
    }

    public object ListImages(string db,string entryId)
    {
        using var c=Open(db,true);EnsureEntry(c,entryId);using var cmd=c.CreateCommand();cmd.CommandText="SELECT id,file_path,original_name,mime_type,sha256,caption,sort_order,created_at FROM entry_images WHERE entry_id=$id ORDER BY sort_order,created_at;";cmd.Parameters.AddWithValue("$id",entryId);
        var rows=new List<object>();using var r=cmd.ExecuteReader();while(r.Read()) rows.Add(new {Id=r.GetString(0),Path=r.GetString(1),OriginalName=r.IsDBNull(2)?null:r.GetString(2),MimeType=r.IsDBNull(3)?null:r.GetString(3),Sha256=r.IsDBNull(4)?null:r.GetString(4),Caption=r.IsDBNull(5)?null:r.GetString(5),SortOrder=r.GetInt32(6),CreatedAt=r.GetString(7),FileExists=File.Exists(r.GetString(1))});return rows;
    }

    public object RemoveImage(string db,string imageId)
    {
        using var c=Open(db);using var cmd=c.CreateCommand();cmd.CommandText="DELETE FROM entry_images WHERE id=$id;";cmd.Parameters.AddWithValue("$id",imageId);if(cmd.ExecuteNonQuery()==0)throw new KeyNotFoundException($"找不到圖片關聯：{imageId}");return new {Id=imageId,AssociationDeleted=true,FileDeleted=false,Note="只移除資料庫關聯，不刪除硬碟原始圖片。"};
    }

    public object UpdateImageCaption(string db, string imageId, string? caption)
    {
        if (caption is { Length: > 2000 }) throw new ArgumentException("圖片說明不可超過 2,000 個字元。");
        var clean = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim();
        using var c = Open(db);
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE entry_images SET caption=$caption WHERE id=$id;";
        cmd.Parameters.AddWithValue("$caption", (object?)clean ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$id", imageId);
        if (cmd.ExecuteNonQuery() == 0) throw new KeyNotFoundException($"找不到圖片關聯：{imageId}");
        return new { Id = imageId, Caption = clean, Updated = true, OriginalFileChanged = false };
    }

    public object VerifyImageFile(string db, string imageId)
    {
        using var c = Open(db, true);
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT file_path,sha256 FROM entry_images WHERE id=$id;";
        cmd.Parameters.AddWithValue("$id", imageId);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) throw new KeyNotFoundException($"找不到圖片關聯：{imageId}");
        var path = r.GetString(0);
        var expected = r.IsDBNull(1) ? null : r.GetString(1);
        if (!File.Exists(path)) return new { Id = imageId, Path = path, FileExists = false, HashMatches = (bool?)null, ActualSha256 = (string?)null };
        string actual;
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            actual = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        var matches = expected is not null && string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
        return new { Id = imageId, Path = path, FileExists = true, HashMatches = (bool?)matches, ActualSha256 = actual };
    }

    public object ListSources(string db,string entryId)
    {
        using var c=Open(db,true);EnsureEntry(c,entryId);using var cmd=c.CreateCommand();cmd.CommandText="SELECT batch_id,source_file,source_sheet,source_row,source_table,imported_at FROM entry_sources WHERE entry_id=$id ORDER BY imported_at DESC;";cmd.Parameters.AddWithValue("$id",entryId);var rows=new List<object>();using var r=cmd.ExecuteReader();while(r.Read())rows.Add(new {BatchId=r.IsDBNull(0)?null:r.GetString(0),SourceFile=r.GetString(1),Sheet=r.GetString(2),Row=r.GetInt32(3),Table=r.IsDBNull(4)?null:r.GetString(4),ImportedAt=r.GetString(5)});return rows;
    }

    private static void EnsureEntry(SqliteConnection c,string id){using var q=c.CreateCommand();q.CommandText="SELECT 1 FROM entries WHERE id=$id;";q.Parameters.AddWithValue("$id",id);if(q.ExecuteScalar()==null)throw new KeyNotFoundException($"找不到條目：{id}");}
    private static SqliteConnection Open(string path,bool readOnly=false){var full=Path.GetFullPath(path);if(!File.Exists(full))throw new FileNotFoundException("找不到知識庫，請先執行 db-init。",full);var c=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=full,Mode=readOnly?SqliteOpenMode.ReadOnly:SqliteOpenMode.ReadWrite,Pooling=false}.ToString());c.Open();using var p=c.CreateCommand();p.CommandText="PRAGMA foreign_keys=ON;";p.ExecuteNonQuery();return c;}
    private static string? MimeFromExtension(string ext)=>ext.ToLowerInvariant() switch{".jpg" or ".jpeg"=>"image/jpeg",".png"=>"image/png",".gif"=>"image/gif",".webp"=>"image/webp",".bmp"=>"image/bmp",".svg"=>"image/svg+xml",_=>null};
}
