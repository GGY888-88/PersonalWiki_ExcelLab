#!/usr/bin/env python3
"""Run inherited 4.6.5/4.6.4 regressions plus 4.6.6 three-box checks."""
import subprocess
import sys
from pathlib import Path
root = Path(__file__).resolve().parents[1]
for script in ['scripts/run-4.6.4-python-tests.py', 'scripts/test-4.6.6-three-input-heights.py']:
    print(f'\n=== {script} ===', flush=True)
    result = subprocess.run([sys.executable, str(root / script)], cwd=root)
    if result.returncode:
        raise SystemExit(result.returncode)
print('PASS: inherited regressions and 4.6.6 compact-height checks')
