#!/usr/bin/env python3
"""Static safety guards for backup/recovery source; runtime .NET tests remain separate."""
from pathlib import Path
checks=0
def ok(v,m):
 global checks
 checks+=1
 if not v: raise AssertionError(m)
b=Path('Services/DatabaseBackupService.cs').read_text(encoding='utf-8-sig')
r=Path('Services/DatabaseRecoveryService.cs').read_text(encoding='utf-8-sig')
for needle in ['HasSqliteFiles(target)','path + "-wal"','path + "-shm"','path + "-journal"','File.Move(staging, target)','Pooling = false','SqliteOpenMode.ReadOnly','PRAGMA foreign_keys=ON','PRAGMA integrity_check','PRAGMA foreign_key_check']:
 ok(needle in b, f'backup missing {needle}')
for needle in ['target + "-wal"','target + "-shm"','target + "-journal"','targetCreatedByThisOperation','catch']:
 ok(needle in r, f'recovery missing {needle}')
print(f'PASS {checks}/15')
