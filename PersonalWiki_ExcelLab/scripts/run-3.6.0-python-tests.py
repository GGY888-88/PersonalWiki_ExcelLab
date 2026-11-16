#!/usr/bin/env python3
"""Run restored 3.6.0 regression fixtures and current release static verification."""
import subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
tests=['test-3.6.0-search-sorting.py','test-3.6.0-database-recovery-safety.py','test-3.6.0-backup-wal-locking.py','test-3.6.0-search-large-dataset.py','verify-release.py']
for name in tests:
 print(f'=== {name} ===',flush=True)
 subprocess.run([sys.executable,str(root/'scripts'/name)],cwd=root,check=True)
print(f'PASS {len(tests)}/{len(tests)} scripts')
