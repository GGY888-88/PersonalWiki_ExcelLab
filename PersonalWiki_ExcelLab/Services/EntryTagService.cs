using Microsoft.Data.Sqlite;

namespace PersonalWiki.ExcelLab.Services;

public sealed class EntryTagService
{
    public object Add(string db,string entryId,string tag)
    {
        var clean=Normalize(tag);using var c=Open(db);EnsureEntry(c,entryId);using var cmd=c.CreateCommand();cmd.CommandText="INSERT OR IGNORE INTO entry_tags(entry_id,tag,normalized_tag) VALUES($entry,$tag,$normalized);";cmd.Parameters.AddWithValue("$entry",entryId);cmd.Parameters.AddWithValue("$tag",clean);cmd.Parameters.AddWithValue("$normalized",clean.Normalize(System.Text.NormalizationForm.FormKC).ToUpperInvariant());var changed=cmd.ExecuteNonQuery();return new {EntryId=entryId,Tag=clean,Added=changed==1};
    }
    public object Remove(string db,string entryId,string tag)
    {
        var normalized=Normalize(tag).Normalize(System.Text.NormalizationForm.FormKC).ToUpperInvariant();using var c=Open(db);using var cmd=c.CreateCommand();cmd.CommandText="DELETE FROM entry_tags WHERE entry_id=$entry AND normalized_tag=$tag;";cmd.Parameters.AddWithValue("$entry",entryId);cmd.Parameters.AddWithValue("$tag",normalized);var n=cmd.ExecuteNonQuery();return new {EntryId=entryId,Tag=tag,Removed=n==1};
    }
    public object List(string db,string entryId)
    {
        using var c=Open(db,true);EnsureEntry(c,entryId);using var cmd=c.CreateCommand();cmd.CommandText="SELECT tag FROM entry_tags WHERE entry_id=$id ORDER BY normalized_tag;";cmd.Parameters.AddWithValue("$id",entryId);var tags=new List<string>();using var r=cmd.ExecuteReader();while(r.Read())tags.Add(r.GetString(0));return tags;
    }
    public object Search(string db,string tag)
    {
        var normalized=Normalize(tag).Normalize(System.Text.NormalizationForm.FormKC).ToUpperInvariant();using var c=Open(db,true);using var cmd=c.CreateCommand();cmd.CommandText="SELECT e.id,e.title,e.entry_type,e.summary,e.updated_at FROM entry_tags t JOIN entries e ON e.id=t.entry_id WHERE t.normalized_tag=$tag AND e.is_archived=0 ORDER BY e.updated_at DESC;";cmd.Parameters.AddWithValue("$tag",normalized);var rows=new List<object>();using var r=cmd.ExecuteReader();while(r.Read())rows.Add(new {Id=r.GetString(0),Title=r.GetString(1),EntryType=r.GetString(2),Summary=r.IsDBNull(3)?null:r.GetString(3),UpdatedAt=r.GetString(4)});return rows;
    }
    private static string Normalize(string tag){if(string.IsNullOrWhiteSpace(tag))throw new ArgumentException("標籤不可空白。");var v=tag.Trim();if(v.Length>100)throw new ArgumentException("標籤不可超過 100 個字元。");return v;}
    private static void EnsureEntry(SqliteConnection c,string id){using var q=c.CreateCommand();q.CommandText="SELECT 1 FROM entries WHERE id=$id;";q.Parameters.AddWithValue("$id",id);if(q.ExecuteScalar()==null)throw new KeyNotFoundException($"找不到條目：{id}");}
    private static SqliteConnection Open(string path,bool readOnly=false){var full=Path.GetFullPath(path);if(!File.Exists(full))throw new FileNotFoundException("找不到知識庫，請先執行 db-init。",full);var c=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=full,Mode=readOnly?SqliteOpenMode.ReadOnly:SqliteOpenMode.ReadWrite,Pooling=false}.ToString());c.Open();using var p=c.CreateCommand();p.CommandText="PRAGMA foreign_keys=ON;";p.ExecuteNonQuery();return c;}
}
