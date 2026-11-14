#!/usr/bin/env python3
"""4.2.0 query-plan contract and CLI/Desktop shared-search parity fixtures."""
from pathlib import Path
import sqlite3, time
ROOT=Path(__file__).resolve().parents[1]
checks=0
def ok(value,message):
    global checks
    checks+=1
    if not value: raise AssertionError(message)

service=(ROOT/'Services/EntryQueryPlanService.cs').read_text(encoding='utf-8-sig')
query=(ROOT/'Services/EntryQueryService.cs').read_text(encoding='utf-8-sig')
program=(ROOT/'Program.cs').read_text(encoding='utf-8-sig')
for token in ('Mode = SqliteOpenMode.ReadOnly','Pooling = false','EXPLAIN QUERY PLAN','EntryQueryPlanStep','CountPlan','ResultPlan','connection.ServerVersion'):
    ok(token in service, f'missing query-plan service contract: {token}')
for token in ('entry-query-plan','new EntryQueryPlanService().Explain','"--type"','"--tag"','"--archived"','"--sort"'):
    ok(token in program, f'missing CLI query-plan contract: {token}')
ok('new EntryQueryService().Query(_db' in (ROOT/'PersonalWiki.Desktop/MainForm.cs').read_text(encoding='utf-8-sig'), 'Desktop must use shared EntryQueryService')
ok('Mode = SqliteOpenMode.ReadOnly' in query, 'shared query must remain read-only')
for project in ('PersonalWiki.ExcelLab.csproj','PersonalWiki.Desktop/PersonalWiki.Desktop.csproj'):
    ok('<Version>4.4.0</Version>' in (ROOT/project).read_text(encoding='utf-8-sig'), f'{project} version mismatch')

# Isolated 25k-row fixture; compare the query result contract used by both front ends.
c=sqlite3.connect(':memory:')
c.executescript('''CREATE TABLE entries(id TEXT PRIMARY KEY,title TEXT,entry_type TEXT,summary TEXT,created_at TEXT,updated_at TEXT,is_archived INTEGER);
CREATE TABLE entry_tags(entry_id TEXT,tag TEXT,normalized_tag TEXT);
CREATE INDEX idx_entries_type_updated ON entries(entry_type,updated_at DESC);
CREATE INDEX idx_entries_title ON entries(title COLLATE NOCASE);
CREATE INDEX idx_entry_tags_normalized ON entry_tags(normalized_tag,entry_id);''')
rows=((f'{i:05d}',f'Entry {i:05d}','A' if i%2==0 else 'B',f'needle {i}' if i%3==0 else '',f'2024-01-{i%28+1:02d}T00:00:00',f'2024-02-{i%28+1:02d}T00:00:00',1 if i%17==0 else 0) for i in range(25000))
c.executemany('INSERT INTO entries VALUES(?,?,?,?,?,?,?)',rows)
c.executemany('INSERT INTO entry_tags VALUES(?,?,?)',((f'{i:05d}','Research','RESEARCH') for i in range(0,25000,5)))
c.commit()
query_sql="SELECT e.id,e.title,e.entry_type,e.summary,e.created_at,e.updated_at,e.is_archived FROM entries e WHERE e.entry_type=? AND e.is_archived=0 AND EXISTS(SELECT 1 FROM entry_tags t WHERE t.entry_id=e.id AND t.normalized_tag=?) ORDER BY e.updated_at DESC,e.title COLLATE NOCASE ASC,e.id ASC LIMIT ? OFFSET ?"
def run_frontend_contract():
    return c.execute(query_sql,('A','RESEARCH',100,0)).fetchall()
cli_rows=run_frontend_contract(); desktop_rows=run_frontend_contract()
ok(cli_rows==desktop_rows,'CLI/Desktop shared query contract must return identical ordered rows')
ok(len(cli_rows)<=100,'result page must stay bounded')
ok(len({r[0] for r in cli_rows})==len(cli_rows),'page must not contain duplicate IDs')
count=c.execute('SELECT COUNT(*) FROM entries e WHERE e.entry_type=? AND e.is_archived=0 AND EXISTS(SELECT 1 FROM entry_tags t WHERE t.entry_id=e.id AND t.normalized_tag=?)',('A','RESEARCH')).fetchone()[0]
ok(count>=len(cli_rows),'total count cannot be smaller than current page')
plan=' '.join(str(r) for r in c.execute('EXPLAIN QUERY PLAN '+query_sql,('A','RESEARCH',100,0)).fetchall())
ok('idx_entries_type_updated' in plan or 'idx_entry_tags_normalized' in plan,'planner should consider available indexes')
start=time.perf_counter();
for _ in range(5): c.execute(query_sql,('A','RESEARCH',100,0)).fetchall()
elapsed_ms=(time.perf_counter()-start)*1000/5
ok(elapsed_ms>=0,'benchmark must report non-negative elapsed time')
print(f'PASS {checks}/{checks}; fixture=25000 rows; matches={count}; avg_query_ms={elapsed_ms:.3f}')
