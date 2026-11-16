#!/usr/bin/env python3
"""Run 4.3.0 complete query-plan checks and all prior release regressions."""
import subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
for script in ('test-4.3.0-complete-query-plan.py','run-4.2.0-python-tests.py'):
    subprocess.run([sys.executable,str(root/'scripts'/script)],cwd=root,check=True)
print('PASS 4.3.0 complete query-plan + 4.2.0 and earlier regression suites')
