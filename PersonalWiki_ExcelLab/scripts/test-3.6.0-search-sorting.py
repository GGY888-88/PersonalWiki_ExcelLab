#!/usr/bin/env python3
"""SQLite fixture tests for deterministic sort/paging semantics; does not execute the .NET service."""
import sqlite3, tempfile, os
checks=0
def ok(v, msg):
 global checks
 checks+=1
 if not v: raise AssertionError(msg)
fd,p=tempfile.mkstemp(suffix='.db'); os.close(fd)
try:
 c=sqlite3.connect(p); c.executescript('CREATE TABLE entries(id TEXT PRIMARY KEY,title TEXT,entry_type TEXT,summary TEXT,created_at TEXT,updated_at TEXT,is_archived INTEGER); CREATE TABLE entry_tags(entry_id TEXT,tag TEXT,normalized_tag TEXT); CREATE INDEX idx_entries_type_updated ON entries(entry_type,updated_at DESC);')
 rows=[('a','Zeta','A',None,'2024-01-01T00:00:00Z','2024-03-01T00:00:00Z',0),('b','Alpha','A',None,'2024-02-01T00:00:00Z','2024-02-01T00:00:00Z',0),('c','Beta','B',None,'2024-03-01T00:00:00Z','2024-01-01T00:00:00Z',0),('d','alpha','A',None,'2024-04-01T00:00:00Z','2024-04-01T00:00:00Z',1)]
 c.executemany('INSERT INTO entries VALUES(?,?,?,?,?,?,?)',rows); c.commit()
 sorts={'updated-desc':'updated_at DESC,title COLLATE NOCASE ASC,id ASC','title-asc':'title COLLATE NOCASE ASC,id ASC','title-desc':'title COLLATE NOCASE DESC,id ASC','created-desc':'created_at DESC,title COLLATE NOCASE ASC,id ASC','created-asc':'created_at ASC,title COLLATE NOCASE ASC,id ASC'}
 expected={'updated-desc':['d','a','b','c'],'title-asc':['b','d','c','a'],'title-desc':['a','c','b','d'],'created-desc':['d','c','b','a'],'created-asc':['a','b','c','d']}
 for k,sql in sorts.items():
  got=[r[0] for r in c.execute('SELECT id FROM entries ORDER BY '+sql)]
  ok(got==expected[k],f'{k} order mismatch: {got}')
  ok(len(got)==4,f'{k} row count')
 ok([r[0] for r in c.execute('SELECT id FROM entries ORDER BY updated_at DESC,title COLLATE NOCASE ASC,id ASC LIMIT 2 OFFSET 0')]==['d','a'],'page 1')
 ok([r[0] for r in c.execute('SELECT id FROM entries ORDER BY updated_at DESC,title COLLATE NOCASE ASC,id ASC LIMIT 2 OFFSET 2')]==['b','c'],'page 2')
 ok(c.execute('SELECT COUNT(*) FROM entries WHERE is_archived=0').fetchone()[0]==3,'exclude archived')
 ok(c.execute('SELECT COUNT(*) FROM entries WHERE is_archived=1').fetchone()[0]==1,'only archived')
 ok(c.execute('SELECT COUNT(*) FROM entries WHERE entry_type=?',('A',)).fetchone()[0]==3,'type filter')
 ok(c.execute('SELECT COUNT(*) FROM entries WHERE updated_at>=?',('2024-02-01',)).fetchone()[0]==3,'ISO date prefix compare')
 ok(c.execute('PRAGMA query_only').fetchone()[0]==0,'fixture writable before readonly test')
 c.execute('PRAGMA query_only=ON')
 try: c.execute("UPDATE entries SET title='x' WHERE id='a'"); writable=False
 except sqlite3.OperationalError: writable=True
 ok(writable,'query-only rejects writes')
 ok(c.execute('SELECT title FROM entries WHERE id="a"').fetchone()[0]=='Zeta','data unchanged')
 ok('ORDER BY' in open('Services/EntryQueryService.cs',encoding='utf-8-sig').read(),'service includes explicit ordering')
 print(f'PASS {checks}/20')
finally:
 try:c.close()
 except:pass
 os.unlink(p)
