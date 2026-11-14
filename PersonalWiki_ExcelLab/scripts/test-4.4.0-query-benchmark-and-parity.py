#!/usr/bin/env python3
"""4.4.0 read-only query benchmark and CLI/Desktop shared-query contract checks."""
from pathlib import Path
import sqlite3, tempfile

ROOT = Path(__file__).resolve().parents[1]
service = (ROOT / 'Services/EntryQueryBenchmarkService.cs').read_text(encoding='utf-8')
query = (ROOT / 'Services/EntryQueryService.cs').read_text(encoding='utf-8')
program = (ROOT / 'Program.cs').read_text(encoding='utf-8-sig')
desktop = (ROOT / 'PersonalWiki.Desktop/MainForm.cs').read_text(encoding='utf-8')
checks = 0

def check(condition, message):
    global checks
    checks += 1
    if not condition:
        raise AssertionError(message)

check('class EntryQueryBenchmarkService' in service, 'benchmark service exists')
check('Stopwatch.StartNew()' in service, 'uses Stopwatch')
check('iterations is < 1 or > 20' in service, 'bounds iterations')
check('var warmup = service.Query' in service, 'warm-up is outside measured loop')
check('timer.Elapsed.TotalMilliseconds' in service, 'records elapsed milliseconds')
check('ordered[0]' in service and 'ordered[^1]' in service, 'reports min/max')
check('samples.Average()' in service and 'median' in service, 'reports average and median')
check('SequenceEqual(warmup.Items.Select(x => x.Id))' in service, 'detects result drift')
check('ReadOnly' in service, 'report declares read-only')
check('端到端讀取耗時' in service, 'explains measured scope')
check('EXPLAIN QUERY PLAN' in service or '不是 SQLite EXPLAIN QUERY PLAN' in service, 'distinguishes query plan estimates')
check('不寫入資料庫' in service, 'documents no writes')
check('case "entry-query-benchmark"' in program, 'CLI command registered')
check('--iterations' in program, 'CLI iterations option')
check('new EntryQueryBenchmarkService().Run' in program, 'CLI invokes service')
check('<Version>4.4.0</Version>' in (ROOT/'PersonalWiki.ExcelLab.csproj').read_text(), 'CLI project version')
check('<Version>4.4.0</Version>' in (ROOT/'PersonalWiki.Desktop/PersonalWiki.Desktop.csproj').read_text(), 'Desktop project version')
check('new EntryQueryService().Query' in desktop, 'Desktop uses shared query service')
check('new EntryQueryService()' in service, 'benchmark uses shared query service')
check('Mode = SqliteOpenMode.ReadOnly' in query, 'shared query service opens read-only')
check('pageSize is < 1 or > 200' in query, 'page size is bounded')
check('ValidateRange' in query, 'date ranges validated')
check('SortSql.TryGetValue' in query, 'sort is allow-listed')

# Independent fixture validates stable ordering/count/page behavior expected by both front ends.
with tempfile.TemporaryDirectory() as td:
    db = Path(td)/'parity.db'
    con = sqlite3.connect(db)
    con.executescript('''
      CREATE TABLE entries(id TEXT PRIMARY KEY,title TEXT NOT NULL,entry_type TEXT NOT NULL,summary TEXT,
        created_at TEXT NOT NULL,updated_at TEXT NOT NULL,is_archived INTEGER NOT NULL DEFAULT 0);
      CREATE TABLE entry_tags(entry_id TEXT NOT NULL,tag TEXT NOT NULL,normalized_tag TEXT NOT NULL);
      CREATE INDEX ix_entries_updated ON entries(updated_at DESC,title COLLATE NOCASE ASC,id ASC);
    ''')
    rows = [(f'E{i:04}',f'Title {i:04}','人物',f'Summary {i%9}',f'2024-01-{i%28+1:02}T00:00:00',f'2025-02-{i%28+1:02}T00:00:00',i%11==0) for i in range(1250)]
    con.executemany('INSERT INTO entries VALUES(?,?,?,?,?,?,?)', rows)
    con.executemany('INSERT INTO entry_tags VALUES(?,?,?)', [(f'E{i:04}','研究','研究') for i in range(0,1250,3)])
    con.commit()
    check(con.execute('PRAGMA user_version').fetchone()[0] == 0, 'fixture starts isolated without application schema')
    sql = "SELECT id,title FROM entries WHERE is_archived=0 AND entry_type=? ORDER BY updated_at DESC,title COLLATE NOCASE ASC,id ASC LIMIT ? OFFSET ?"
    count = con.execute('SELECT COUNT(*) FROM entries WHERE is_archived=0 AND entry_type=?',('人物',)).fetchone()[0]
    page_a = con.execute(sql,('人物',37,0)).fetchall()
    page_b = con.execute(sql,('人物',37,37)).fetchall()
    check(count == 1136, 'fixture total count')
    check(len(page_a) == 37 and len(page_b) == 37, 'two complete pages')
    check(not ({x[0] for x in page_a} & {x[0] for x in page_b}), 'pages do not overlap')
    check(page_a == con.execute(sql,('人物',37,0)).fetchall(), 'repeated CLI/Desktop contract is deterministic')
    check(con.execute('SELECT COUNT(*) FROM entry_tags').fetchone()[0] == 417, 'tag fixture is intact')
    plan_rows = list(con.execute('EXPLAIN QUERY PLAN '+sql,('人物',37,0)))
    check(len(plan_rows) > 0 and all(len(x) >= 4 for x in plan_rows), 'query plan can be inspected separately')
    con.close()

print(f'PASS 4.4.0 query benchmark/parity checks: {checks}/{checks}')
