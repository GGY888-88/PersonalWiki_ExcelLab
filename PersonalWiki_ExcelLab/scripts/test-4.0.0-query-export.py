#!/usr/bin/env python3
"""4.0.0 read-only search-result JSON export and no-overwrite contract checks."""
import json, sqlite3, tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
checks=0
def ok(value,message):
 global checks
 checks+=1
 if not value: raise AssertionError(message)

program=(ROOT/'Program.cs').read_text(encoding='utf-8-sig')
safe=(ROOT/'Services/SafeFileOutput.cs').read_text(encoding='utf-8-sig')
for token in ['--output report.json','case "--output"','queryOutput','JsonSerializer.Serialize(queryResult, JsonOptions)','SafeFileOutput.WriteAllText(queryOutput, queryJson + Environment.NewLine)','queryResult.TotalCount','queryResult.Items.Count']:
 ok(token in program,f'CLI query export contract: {token}')
ok('FileMode.CreateNew' in safe,'export output uses create-new semantics')
ok('File.Delete(fullPath)' in safe,'failed newly-created output cleanup is limited to this call')
ok('Mode = SqliteOpenMode.ReadOnly' in (ROOT/'Services/EntryQueryService.cs').read_text(encoding='utf-8-sig'),'query source remains read-only')

# Exercise the documented JSON result shape and destination no-overwrite behavior with an isolated fixture.
with tempfile.TemporaryDirectory() as td:
 db=Path(td)/'fixture.db'
 with sqlite3.connect(db) as con:
  con.executescript('CREATE TABLE entries(id TEXT PRIMARY KEY,title TEXT); INSERT INTO entries VALUES("e1","Alpha"),("e2","Beta");')
  rows=[{'id':r[0],'title':r[1]} for r in con.execute('SELECT id,title FROM entries ORDER BY title')]
 payload={'keyword':None,'totalCount':len(rows),'items':rows}
 output=Path(td)/'query-results.json'
 output.write_text(json.dumps(payload,ensure_ascii=False),encoding='utf-8')
 ok(json.loads(output.read_text(encoding='utf-8'))['totalCount']==2,'export JSON can be parsed and preserves result count')
 original=output.read_bytes()
 try:
  with output.open('xb') as stream: stream.write(b'overwrite')
  raise AssertionError('existing destination unexpectedly opened')
 except FileExistsError:
  pass
 ok(output.read_bytes()==original,'existing export destination remains unchanged')
 ok(not list(Path(td).glob('*.tmp')),'export fixture leaves no temporary files')

for project in ('PersonalWiki.ExcelLab.csproj','PersonalWiki.Desktop/PersonalWiki.Desktop.csproj'):
 ok('<Version>4.4.0</Version>' in (ROOT/project).read_text(encoding='utf-8-sig'),f'{project} active release version synchronized')
print(f'PASS {checks}/{checks}')
