#!/usr/bin/env python3
"""25k-row SQLite fixture: pagination consistency, count parity and index-plan checks."""
import sqlite3
checks=0
def ok(v,m):
 global checks
 checks+=1
 if not v: raise AssertionError(m)
c=sqlite3.connect(':memory:')
c.executescript('CREATE TABLE entries(id TEXT PRIMARY KEY,title TEXT,entry_type TEXT,summary TEXT,created_at TEXT,updated_at TEXT,is_archived INTEGER); CREATE INDEX idx_entries_type_updated ON entries(entry_type,updated_at DESC); CREATE INDEX idx_entries_title ON entries(title COLLATE NOCASE);')
c.executemany('INSERT INTO entries VALUES(?,?,?,?,?,?,?)',((f'{i:05d}',f'Entry {i:05d}','A' if i%2==0 else 'B',f'needle {i}' if i%3==0 else '',f'2024-01-{(i%28)+1:02d}T00:00:00Z',f'2024-02-{(i%28)+1:02d}T00:00:00Z',0) for i in range(25000)))
c.commit()
total=c.execute("SELECT COUNT(*) FROM entries WHERE summary LIKE '%needle%'").fetchone()[0]
ok(total==8334,'expected 8334 matching rows')
seen=[]; pages=0
for page in range(79):
 rows=c.execute("SELECT id FROM entries WHERE summary LIKE '%needle%' ORDER BY updated_at DESC,title COLLATE NOCASE,id LIMIT 106 OFFSET ?",(page*106,)).fetchall()
 pages+=1; ids=[x[0] for x in rows]; seen.extend(ids)
 ok(len(ids)<=106,f'page {page+1} bounded')
ok(len(seen)==total,'all matches included')
ok(len(seen)==len(set(seen)),'no duplicate or overlapping pages')
ok(pages==79,'79 pages')
ok(c.execute('SELECT COUNT(*) FROM entries').fetchone()[0]==25000,'25k fixture rows')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE entry_type="A"').fetchone()[0]==12500,'type A count')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE entry_type="B"').fetchone()[0]==12500,'type B count')
plan=' '.join(str(x) for x in c.execute('EXPLAIN QUERY PLAN SELECT id FROM entries WHERE entry_type=? ORDER BY updated_at DESC LIMIT 50',('A',)).fetchall())
ok('idx_entries_type_updated' in plan,'planner uses composite index')
ok(c.execute('SELECT COUNT(*) FROM entries WHERE is_archived=0').fetchone()[0]==25000,'active count')
ok(c.execute('SELECT COUNT(*) FROM (SELECT id FROM entries WHERE summary LIKE ? ORDER BY updated_at DESC LIMIT 106)',('%needle%',)).fetchone()[0]==106,'first page count')
ok(c.execute('SELECT COUNT(*) FROM (SELECT id FROM entries WHERE summary LIKE ? ORDER BY updated_at DESC LIMIT 106 OFFSET 106)',('%needle%',)).fetchone()[0]==106,'second page count')
ok(c.execute('SELECT COUNT(*) FROM (SELECT id FROM entries WHERE summary LIKE ? ORDER BY updated_at DESC LIMIT 106 OFFSET 8330)',('%needle%',)).fetchone()[0]==4,'last page count')
print(f'PASS {checks}/91')
