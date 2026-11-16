#!/usr/bin/env python3
"""Run 4.2.0 query-plan/parity checks and all prior release regressions."""
import subprocess, sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
for script in ('test-4.2.0-query-plan-and-parity.py','run-4.1.0-python-tests.py'):
    subprocess.run([sys.executable,str(root/'scripts'/script)],cwd=root,check=True)
print('PASS 4.2.0 query-plan/parity + 4.1.0 and earlier regression suites')
