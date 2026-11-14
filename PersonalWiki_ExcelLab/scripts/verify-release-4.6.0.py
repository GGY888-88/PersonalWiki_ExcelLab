#!/usr/bin/env python3
"""Cross-platform static release verification; does not compile or run .NET."""
from pathlib import Path
import re,zipfile,json
root=Path(__file__).resolve().parents[1]; checks=0
def ok(v,m):
 global checks
 checks+=1
 if not v: raise SystemExit('FAIL: '+m)
cli=(root/'PersonalWiki.ExcelLab.csproj').read_text(encoding='utf-8-sig')
desktop=(root/'PersonalWiki.Desktop/PersonalWiki.Desktop.csproj').read_text(encoding='utf-8-sig')
service=(root/'Services/EntryQueryService.cs').read_text(encoding='utf-8-sig')
plan_service=(root/'Services/EntryQueryPlanService.cs').read_text(encoding='utf-8-sig')
program=(root/'Program.cs').read_text(encoding='utf-8-sig')
main=(root/'PersonalWiki.Desktop/MainForm.cs').read_text(encoding='utf-8-sig')
for p in ['README.md','docs/releases/INDEX.md','docs/releases/HANDOFF_3.6.0.md','docs/releases/UPDATE_NOTES_3.6.0.md','docs/releases/TEST_GUIDE_3.6.0.md','docs/releases/AUDIT_REPORT_3.6.0.md','docs/releases/HANDOFF_3.7.0.md','docs/releases/UPDATE_NOTES_3.7.0.md','docs/releases/TEST_GUIDE_3.7.0.md','docs/releases/AUDIT_REPORT_3.7.0.md','docs/releases/HANDOFF_3.8.0.md','docs/releases/UPDATE_NOTES_3.8.0.md','docs/releases/TEST_GUIDE_3.8.0.md','docs/releases/AUDIT_REPORT_3.8.0.md','docs/releases/HANDOFF_3.9.0.md','docs/releases/UPDATE_NOTES_3.9.0.md','docs/releases/TEST_GUIDE_3.9.0.md','docs/releases/AUDIT_REPORT_3.9.0.md','docs/releases/HANDOFF_4.0.0.md','docs/releases/UPDATE_NOTES_4.0.0.md','docs/releases/TEST_GUIDE_4.0.0.md','docs/releases/AUDIT_REPORT_4.0.0.md','docs/releases/HANDOFF_4.1.0.md','docs/releases/UPDATE_NOTES_4.1.0.md','docs/releases/TEST_GUIDE_4.1.0.md','docs/releases/AUDIT_REPORT_4.1.0.md','docs/releases/HANDOFF_4.2.0.md','docs/releases/UPDATE_NOTES_4.2.0.md','docs/releases/TEST_GUIDE_4.2.0.md','docs/releases/AUDIT_REPORT_4.2.0.md','docs/releases/HANDOFF_4.3.0.md','docs/releases/UPDATE_NOTES_4.3.0.md','docs/releases/TEST_GUIDE_4.3.0.md','docs/releases/AUDIT_REPORT_4.3.0.md','docs/releases/HANDOFF_4.4.0.md','docs/releases/UPDATE_NOTES_4.4.0.md','docs/releases/TEST_GUIDE_4.4.0.md','docs/releases/AUDIT_REPORT_4.4.0.md','docs/releases/HANDOFF_4.6.0.md','docs/releases/UPDATE_NOTES_4.6.0.md','docs/releases/TEST_GUIDE_4.6.0.md','docs/releases/AUDIT_REPORT_4.6.0.md']:
 ok((root/p).exists(),f'missing {p}')
ok('<TargetFramework>net10.0</TargetFramework>' in cli,'CLI framework')
ok('<TargetFramework>net10.0-windows</TargetFramework>' in desktop,'Desktop framework')
ok('DocumentFormat.OpenXml' in cli and '3.5.1' in cli,'OpenXml dependency')
ok('Microsoft.Data.Sqlite' in cli and '10.0.14' in cli,'SQLite dependency')
ok('<Version>4.6.0</Version>' in cli and '<Version>4.6.0</Version>' in desktop,'versions synchronized')
ok('EXPLAIN QUERY PLAN' in plan_service and 'SqliteOpenMode.ReadOnly' in plan_service,'read-only query-plan diagnostics')
ok('entries_fts MATCH $keyword' in plan_service and 'createdFrom' in plan_service and 'updatedFrom' in plan_service,'complete keyword/date query-plan diagnostics')
ok('ResultIds' in (root/'Services/EntryQueryBenchmarkService.cs').read_text(encoding='utf-8-sig'),'benchmark exports result IDs')
ok((root/'scripts/entry-query-acceptance.py').exists(),'CLI acceptance runner exists')
acceptance=(root/'scripts/entry-query-acceptance.py').read_text(encoding='utf-8')
for token in ['--case-index','rerunCommand','setupError','notRunCases','Partial report:']:
 ok(token in acceptance,f'acceptance diagnostics include {token}')
ok((root/'scripts/test-4.6.0-query-acceptance.py').exists(),'4.6.0 acceptance contract tests exist')
ok('user_version=1' in (root/'Services/WikiDatabaseService.cs').read_text(encoding='utf-8-sig'),'schema version remains 1')
for needle in ['updated-desc','title-asc','title-desc','created-desc','created-asc','createdFrom','updatedFrom','LIMIT $limit OFFSET $offset','SqliteOpenMode.ReadOnly']:
 ok(needle in service,f'query service contains {needle}')
for needle in ['--created-from','--created-to','--updated-from','--updated-to','--sort','--output','SafeFileOutput.WriteAllText']:
 ok(needle in program,f'CLI supports {needle}')
for needle in ['_copySelectedResults','CopySelectedResultSummaries()','_tagFilter','_createdFrom','_createdTo','_updatedFrom','_updatedTo','清除條件','_sortFilter','編輯圖片說明','檢查圖片檔案','item.SharedTags']:
 ok(needle in main,f'desktop UI contains {needle}')
for f in (root/'samples').glob('*.json'):
 try: json.loads(f.read_text(encoding='utf-8-sig')); ok(True,f'valid JSON {f.name}')
 except Exception as e: ok(False,f'invalid JSON {f.name}: {e}')
for f in (root/'samples').glob('*.xlsx'):
 with zipfile.ZipFile(f) as z: ok(z.testzip() is None,f'valid XLSX zip {f.name}')
ok('UpdateImageCaption' in (root/'Services/EntryContentService.cs').read_text(encoding='utf-8-sig'),'image caption update exists')
ok('VerifyImageFile' in (root/'Services/EntryContentService.cs').read_text(encoding='utf-8-sig'),'image hash verification exists')
ok('GROUP_CONCAT(DISTINCT shared.tag)' in (root/'Services/RelatedEntryService.cs').read_text(encoding='utf-8-sig'),'related entries expose shared tag names')
ok('PRAGMA foreign_keys=ON' in (root/'Services/DatabaseBackupService.cs').read_text(encoding='utf-8-sig'),'backup enables foreign keys')
ok('HasSqliteFiles' in (root/'Services/DatabaseBackupService.cs').read_text(encoding='utf-8-sig'),'backup checks sidecars')
ok('File.Delete(candidate)' in (root/'Services/DatabaseRecoveryService.cs').read_text(encoding='utf-8-sig'),'recovery cleanup owned sidecars')
print(f'PASS {checks}/{checks}')
