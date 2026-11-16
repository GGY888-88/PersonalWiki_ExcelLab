#!/usr/bin/env python3
"""4.3.0 complete keyword/date query-plan contract and safety checks."""
from pathlib import Path
import sqlite3, tempfile
ROOT=Path(__file__).resolve().parents[1]
checks=0
def ok(v,m):
    global checks
    checks+=1
    if not v: raise AssertionError(m)
svc=(ROOT/'Services/EntryQueryPlanService.cs').read_text(encoding='utf-8-sig')
program=(ROOT/'Program.cs').read_text(encoding='utf-8-sig')
for token in ('entries_fts MATCH $keyword','e.title LIKE $like','entry_fields','entry_notes','entry_images','--created-from','--created-to','--updated-from','--updated-to','SqliteOpenMode.ReadOnly','Pooling = false','NormalizeDate','ValidateRange','EndExclusive'):
    ok(token in svc or token in program, f'missing complete query plan support: {token}')
for token in ('planKeyword','planCreatedFrom','planCreatedTo','planUpdatedFrom','planUpdatedTo','new EntryQueryPlanService().Explain'):
    ok(token in program, f'missing CLI option binding: {token}')

# Mirror the production query predicates on an isolated database, including FTS and date boundaries.
with tempfile.TemporaryDirectory() as d:
    db=Path(d)/'isolated.db'
    c=sqlite3.connect(db)
    c.executescript('''
    CREATE TABLE entries(id TEXT PRIMARY KEY,title TEXT,entry_type TEXT,summary TEXT,created_at TEXT,updated_at TEXT,is_archived INTEGER);
    CREATE TABLE entry_fields(entry_id TEXT,field_label TEXT,field_value TEXT);
    CREATE TABLE entry_notes(entry_id TEXT,note_text TEXT);
    CREATE TABLE entry_tags(entry_id TEXT,tag TEXT,normalized_tag TEXT);
    CREATE TABLE entry_images(entry_id TEXT,caption TEXT,original_name TEXT);
    CREATE VIRTUAL TABLE entries_fts USING fts5(entry_id UNINDEXED,title,summary,content='');
    CREATE INDEX idx_entries_type_updated ON entries(entry_type,updated_at DESC);
    CREATE INDEX idx_entries_created ON entries(created_at);
    CREATE INDEX idx_entry_tags_normalized ON entry_tags(normalized_tag,entry_id);
    ''')
    c.executemany('INSERT INTO entries VALUES(?,?,?,?,?,?,?)',[
        ('a','Alpha needle','人物','needle summary','2024-01-01T00:00:00','2024-02-01T00:00:00',0),
        ('b','Beta','人物','other','2024-01-31T23:59:59','2024-02-02T00:00:00',0),
        ('c','Gamma','地點','needle','2024-02-01T00:00:00','2024-02-03T00:00:00',1)])
    c.executemany('INSERT INTO entries_fts(entry_id,title,summary) VALUES(?,?,?)',[
        ('a','Alpha needle','needle summary'),('b','Beta','other'),('c','Gamma','needle')])
    c.execute("INSERT INTO entry_notes VALUES('b','needle in note')")
    c.execute("INSERT INTO entry_fields VALUES('b','memo','needle value')")
    c.execute("INSERT INTO entry_tags VALUES('a','Research','RESEARCH')")
    c.execute("INSERT INTO entry_images VALUES('c','needle caption','image.png')")
    c.commit()
    where="""WHERE (e.id IN (SELECT entry_id FROM entries_fts WHERE entries_fts MATCH ?) OR e.title LIKE ? ESCAPE '\\' OR COALESCE(e.summary,'') LIKE ? ESCAPE '\\' OR EXISTS(SELECT 1 FROM entry_fields f WHERE f.entry_id=e.id AND (COALESCE(f.field_label,'') LIKE ? ESCAPE '\\' OR COALESCE(f.field_value,'') LIKE ? ESCAPE '\\')) OR EXISTS(SELECT 1 FROM entry_notes n WHERE n.entry_id=e.id AND n.note_text LIKE ? ESCAPE '\\') OR EXISTS(SELECT 1 FROM entry_tags t WHERE t.entry_id=e.id AND t.tag LIKE ? ESCAPE '\\') OR EXISTS(SELECT 1 FROM entry_images i WHERE i.entry_id=e.id AND (COALESCE(i.caption,'') LIKE ? ESCAPE '\\' OR COALESCE(i.original_name,'') LIKE ? ESCAPE '\\')))
    AND e.created_at >= ? AND e.created_at < ? AND e.updated_at >= ? AND e.updated_at < ? AND e.is_archived=0"""
    params=('"needle"','%needle%','%needle%','%needle%','%needle%','%needle%','%needle%','%needle%','%needle%','2024-01-01T00:00:00','2024-02-01T00:00:00','2024-02-01T00:00:00','2024-02-03T00:00:00')
    rows=c.execute('SELECT e.id FROM entries e '+where+' ORDER BY e.updated_at DESC LIMIT 50 OFFSET 0',params).fetchall()
    ok(rows==[('b',),('a',)],'keyword + inclusive created-date + inclusive updated-date + archive conditions should match expected ordered entries')
    plan=c.execute('EXPLAIN QUERY PLAN SELECT e.id FROM entries e '+where+' ORDER BY e.updated_at DESC LIMIT 50 OFFSET 0',params).fetchall()
    ok(bool(plan),'complete query plan should return steps')
    ok(c.execute('PRAGMA integrity_check').fetchone()[0]=='ok','isolated database integrity')
    c.close()
print(f'PASS {checks}/{checks}; isolated keyword/FTS/date/archive fixture')
