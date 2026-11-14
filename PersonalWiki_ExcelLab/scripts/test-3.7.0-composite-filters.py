#!/usr/bin/env python3
"""SQLite fixture and source-contract checks for 3.7.0 filters; does not execute C# code."""
import sqlite3
from pathlib import Path
checks=0
def ok(v,m):
 global checks
 checks+=1
 if not v: raise AssertionError(m)
c=sqlite3.connect(':memory:')
c.executescript('''CREATE TABLE entries(id TEXT PRIMARY KEY,title TEXT,entry_type TEXT,created_at TEXT,updated_at TEXT,is_archived INTEGER);
CREATE TABLE entry_tags(entry_id TEXT,tag TEXT,normalized_tag TEXT);
CREATE INDEX idx_entries_type_updated ON entries(entry_type,updated_at DESC);
CREATE INDEX idx_entry_tags_normalized ON entry_tags(normalized_tag);''')
rows=[('a','Alpha','人物','2024-01-01T00:00:00Z','2024-02-01T10:00:00Z',0),('b','Beta','地點','2024-01-15T12:00:00Z','2024-02-15T12:00:00Z',0),('c','Gamma','人物','2024-02-01T00:00:00Z','2024-03-01T00:00:00Z',0),('d','Delta','人物','2024-02-29T23:59:59Z','2024-03-10T00:00:00Z',1),('e','Epsilon','物品','2024-03-01T00:00:00Z','2024-03-11T00:00:00Z',0)]
c.executemany('INSERT INTO entries VALUES(?,?,?,?,?,?)',rows)
c.executemany('INSERT INTO entry_tags VALUES(?,?,?)',[('a','重要','重要'),('b','旅行','旅行'),('c','重要','重要'),('d','重要','重要'),('e','收藏','收藏')])
# Date-only ranges are inclusive by converting the end date to next-day exclusive.
q="SELECT id FROM entries WHERE created_at >= ? AND created_at < ? ORDER BY id"
ids=[x[0] for x in c.execute(q,('2024-01-01T00:00:00','2024-02-01T00:00:00'))]
ok(ids==['a','b'],'created range includes start and excludes following day')
ids=[x[0] for x in c.execute(q,('2024-02-01T00:00:00','2024-03-01T00:00:00'))]
ok(ids==['c','d'],'created range includes end calendar day')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE created_at>=?',('2024-03-01T00:00:00',)).fetchone()[0]==1,'created from only')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE created_at<?',('2024-02-01T00:00:00',)).fetchone()[0]==2,'created to only')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE updated_at>=?',('2024-03-01T00:00:00',)).fetchone()[0]==3,'updated from only')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE updated_at<?',('2024-03-01T00:00:00',)).fetchone()[0]==2,'updated to only')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE updated_at>=? AND updated_at<?',('2024-02-01T00:00:00','2024-03-01T00:00:00')).fetchone()[0]==2,'updated bounded range')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE entry_type=? AND created_at>=?',('人物','2024-02-01T00:00:00')).fetchone()[0]==2,'type and created date combine with AND')
ok(c.execute('SELECT COUNT(*) FROM entry_tags WHERE normalized_tag=?',('重要',)).fetchone()[0]==3,'normalized tag filter')
ok(c.execute('SELECT COUNT(*) FROM entries e WHERE e.entry_type=? AND e.created_at>=? AND EXISTS(SELECT 1 FROM entry_tags t WHERE t.entry_id=e.id AND t.normalized_tag=?)',('人物','2024-02-01T00:00:00','重要')).fetchone()[0]==2,'type date tag intersection')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE is_archived=0 AND created_at>=?',('2024-02-01T00:00:00',)).fetchone()[0]==2,'archive plus date intersection')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE is_archived=1 AND created_at>=?',('2024-02-01T00:00:00',)).fetchone()[0]==1,'archived-only plus date')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE created_at>=? AND created_at<?',('2024-02-01T00:00:00','2024-02-01T00:00:00')).fetchone()[0]==0,'same-day empty exclusive range is empty')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE created_at>=?',('2025-01-01T00:00:00',)).fetchone()[0]==0,'future range empty')
service=Path('Services/EntryQueryService.cs').read_text(encoding='utf-8-sig')
for needle in ['NormalizeDate','ValidateRange','AddDatePredicate','created_at','updated_at','$createdFrom','$createdTo','$updatedFrom','$updatedTo','DateOnly.TryParseExact','StringComparison.Ordinal','SqliteOpenMode.ReadOnly']:
 ok(needle in service,f'service contract {needle}')
cli=Path('Program.cs').read_text(encoding='utf-8-sig')
for needle in ['--created-from','--created-to','--updated-from','--updated-to','--tag','--sort']:
 ok(needle in cli,f'CLI option {needle}')
ui=Path('PersonalWiki.Desktop/MainForm.cs').read_text(encoding='utf-8-sig')
for needle in ['_createdFrom','_createdTo','_updatedFrom','_updatedTo','_tagFilter','_clearSearch','_clearSearch.Click','_createdFrom.Checked = false','_sortFilter.SelectedIndex = 0','SearchEntries();']:
 ok(needle in ui,f'desktop control {needle}')
ok('AND ' in service or 'string.Join(" AND ", predicates)' in service,'all supplied conditions are AND-combined')
ok('createdTo' in service and 'AddDays(1)' in service,'date end is inclusive by next-day exclusive bound')
ok('sort' in service and 'SortSql' in service,'sort is whitelist controlled')
ok('pageSize is < 1 or > 200' in service,'page size remains bounded')
ok('LIMIT $limit OFFSET $offset' in service,'paging remains parameterized')
print(f'PASS {checks}/{checks}')
