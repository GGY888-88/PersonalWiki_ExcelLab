#!/usr/bin/env python3
"""Run 3.7.0 composite-filter checks plus the restored 3.6.0 regression suite."""
import subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
subprocess.run([sys.executable,str(root/'scripts/test-3.7.0-composite-filters.py')],cwd=root,check=True)
subprocess.run([sys.executable,str(root/'scripts/run-3.6.0-python-tests.py')],cwd=root,check=True)
print('PASS 3.7.0 filter suite + 3.6.0 regression suite')
