#!/usr/bin/env python3
"""Release-integrity and data-safety checks for PersonalWiki 3.9.0."""
import json, sqlite3, zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
checks = 0

def ok(value, message):
    global checks
    checks += 1
    if not value:
        raise AssertionError(message)

cli = (ROOT/'PersonalWiki.ExcelLab.csproj').read_text(encoding='utf-8-sig')
desktop = (ROOT/'PersonalWiki.Desktop/PersonalWiki.Desktop.csproj').read_text(encoding='utf-8-sig')
for project in (cli, desktop):
    ok('<Version>4.4.0</Version>' in project, 'active project version is 4.2.0')
    ok('net10.0' in project, 'project targets .NET 10')
ok('DocumentFormat.OpenXml' in cli and '3.5.1' in cli, 'OpenXml dependency pinned')
ok('Microsoft.Data.Sqlite' in cli and '10.0.14' in cli, 'SQLite dependency pinned')

for path in sorted((ROOT/'samples').glob('*.json')):
    json.loads(path.read_text(encoding='utf-8-sig'))
    ok(True, f'valid JSON: {path.name}')
for path in sorted((ROOT/'samples').glob('*.xlsx')):
    with zipfile.ZipFile(path) as archive:
        ok(archive.testzip() is None, f'valid XLSX archive: {path.name}')

sample_db = ROOT/'samples/PersonalWiki_3.6.0_Test.db'
with sqlite3.connect(sample_db) as db:
    ok(db.execute('PRAGMA user_version').fetchone()[0] == 1, 'sample DB schema version 1')
    ok(db.execute('PRAGMA quick_check').fetchone()[0] == 'ok', 'sample DB quick_check')
    ok(db.execute('PRAGMA foreign_key_check').fetchall() == [], 'sample DB foreign_key_check')
    ok(db.execute('SELECT COUNT(*) FROM entries').fetchone()[0] == 9, 'sample DB has 9 entries')
    ok(db.execute('SELECT COUNT(*) FROM entry_tags').fetchone()[0] == 17, 'sample DB has 17 tag links')
    ok(db.execute('SELECT COUNT(*) FROM entry_images').fetchone()[0] == 2, 'sample DB has 2 image associations')
ok((ROOT/'samples/PersonalWiki_3.6.0_Test_Image.png').is_file(), 'sample image exists')

schema = (ROOT/'Services/WikiDatabaseService.cs').read_text(encoding='utf-8-sig')
ok('PRAGMA user_version=1;' in schema, 'source schema remains v1')
backup = (ROOT/'Services/DatabaseBackupService.cs').read_text(encoding='utf-8-sig')
for token in ['HasSqliteFiles','File.Move(staging, target)','PRAGMA integrity_check','PRAGMA foreign_key_check','CleanupSqliteFiles']:
    ok(token in backup, f'backup safety contract: {token}')
recovery = (ROOT/'Services/DatabaseRecoveryService.cs').read_text(encoding='utf-8-sig')
for token in ['targetCreatedByThisOperation','target + "-wal"','target + "-shm"','target + "-journal"','Sha256(backup)','Sha256(target)']:
    ok(token in recovery, f'recovery safety contract: {token}')
content = (ROOT/'Services/EntryContentService.cs').read_text(encoding='utf-8-sig')
ok('FileDeleted=false' in content, 'removing image association preserves original file')
ok('SqliteOpenMode.ReadOnly' in content and 'SHA256.HashData(stream)' in content, 'image verification is read-only and hashes file')

for name in ('HANDOFF_3.9.0.md','UPDATE_NOTES_3.9.0.md','TEST_GUIDE_3.9.0.md','AUDIT_REPORT_3.9.0.md'):
    ok((ROOT/'docs/releases'/name).is_file(), f'release document exists: {name}')
print(f'PASS {checks}/{checks}')
