#!/usr/bin/env python3
"""4.6.0 acceptance workflow and benchmark/search comparison contract checks."""
import importlib.util, json, subprocess, sys, tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
checks=0
def check(ok,msg):
 global checks
 checks+=1
 if not ok: raise AssertionError(msg)
service=(ROOT/'Services/EntryQueryBenchmarkService.cs').read_text(encoding='utf-8-sig')
cli=(ROOT/'Program.cs').read_text(encoding='utf-8-sig')
script=(ROOT/'scripts/entry-query-acceptance.py').read_text(encoding='utf-8')
check('IReadOnlyList<string> ResultIds' in service,'benchmark report includes exact ordered result IDs')
check('last.Items.Select(x => x.Id).ToArray()' in service,'benchmark IDs come from measured query result')
check('Mode = SqliteOpenMode.ReadOnly' in (ROOT/'Services/EntryQueryService.cs').read_text(encoding='utf-8-sig'),'query remains read-only')
check("case \"entry-query-benchmark\"" in cli,'benchmark CLI remains registered')
check("case \"entry-query\"" in cli,'search CLI remains registered')
for field in ('totalCount','returnedCount','resultIds','page','pageSize','sort','archivedMode','createdFrom','createdTo','updatedFrom','updatedTo','tag','entryType'):
 check(field in script,f'comparison includes {field}')
for sort in ('updated-desc','title-asc','title-desc','created-desc','created-asc'):
 check(sort in script,f'acceptance covers sort {sort}')
check("'created-from'" in script and "'updated-from'" in script,'acceptance covers date filters')
check("'tag':'測試'" in script and "'archived':'only'" in script,'acceptance covers tag and archive modes')
check('--case-index' in script,'acceptance runner supports targeted case reruns')
check('Partial report:' in script and 'write_reports(output,report)' in script,'acceptance runner persists partial diagnostics')
check('rerunCommand' in script and 'commands' in script,'acceptance report includes reproducible commands')
check('setupError' in script and 'notRunCases' in script,'acceptance report distinguishes setup failures and unrun cases')
for proj in ('PersonalWiki.ExcelLab.csproj','PersonalWiki.Desktop/PersonalWiki.Desktop.csproj'):
 check(any(f'<Version>{version}</Version>' in (ROOT/proj).read_text(encoding='utf-8-sig') for version in ('4.6.0','4.6.1','4.6.2','4.6.3','4.6.4','4.6.5')),f'{proj} version remains within 4.6.x compatibility line')
subprocess.run([sys.executable,str(ROOT/'scripts/entry-query-acceptance.py'),'--self-test'],check=True,cwd=ROOT)
# Exercise the runner's failure path without requiring a .NET SDK.
spec=importlib.util.spec_from_file_location('acceptance_runner',ROOT/'scripts/entry-query-acceptance.py')
runner=importlib.util.module_from_spec(spec); spec.loader.exec_module(runner)
with tempfile.TemporaryDirectory(prefix='pw46-runner-test-') as td:
 import os
 from unittest.mock import patch
 temp=Path(td); db=temp/'source.db'; db.write_bytes(b'isolated-fixture')
 fake_dll=ROOT/'bin/Release/net10.0/PersonalWiki.ExcelLab.dll'
 fake_dll.parent.mkdir(parents=True,exist_ok=True)
 made=fake_dll.exists()
 if not made: fake_dll.write_bytes(b'test-placeholder')
 out=temp/'failure-report'
 with patch.object(sys,'argv',['entry-query-acceptance.py','--database',str(db),'--output',str(out),'--case-index','1','--no-build']), \
      patch.object(runner,'invoke',side_effect=RuntimeError('simulated CLI failure')):
  rc=runner.main()
 check(rc==2,'case-level CLI failure returns acceptance failure status')
 doc=json.loads((out/'acceptance-comparison.json').read_text(encoding='utf-8'))
 check(doc['failedCases']==1 and doc['cases'][0]['error'].find('simulated CLI failure')>=0,'failure details persist in partial report')
 check('--case-index 1' in doc['cases'][0]['rerunCommand'],'failure report contains targeted rerun command')
 if not made: fake_dll.unlink()
# Fixture sanity: acceptance always copies this DB before invoking CLI.
fixture=ROOT/'samples/PersonalWiki_3.6.0_Test.db'
check(fixture.is_file() and fixture.stat().st_size>0,'acceptance fixture exists')
print(f'PASS 4.6.0 acceptance contract checks: {checks}/{checks}')
