#!/usr/bin/env python3
"""4.5.0 acceptance workflow and benchmark/search comparison contract checks."""
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
for proj in ('PersonalWiki.ExcelLab.csproj','PersonalWiki.Desktop/PersonalWiki.Desktop.csproj'):
 check('<Version>4.5.0</Version>' in (ROOT/proj).read_text(encoding='utf-8-sig'),f'{proj} version updated')
subprocess.run([sys.executable,str(ROOT/'scripts/entry-query-acceptance.py'),'--self-test'],check=True,cwd=ROOT)
# Fixture sanity: acceptance always copies this DB before invoking CLI.
fixture=ROOT/'samples/PersonalWiki_3.6.0_Test.db'
check(fixture.is_file() and fixture.stat().st_size>0,'acceptance fixture exists')
print(f'PASS 4.5.0 acceptance contract checks: {checks}/{checks}')
