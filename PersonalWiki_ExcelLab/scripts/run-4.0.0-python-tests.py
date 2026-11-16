#!/usr/bin/env python3
"""Run 4.0.0 feature checks and all previous regression suites."""
import subprocess, sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
for script in ('test-4.0.0-query-export.py','run-3.9.0-python-tests.py'):
 subprocess.run([sys.executable,str(root/'scripts'/script)],cwd=root,check=True)
print('PASS 4.0.0 query export + 3.9.0/3.8.0/3.7.0/3.6.0 regression suites')
