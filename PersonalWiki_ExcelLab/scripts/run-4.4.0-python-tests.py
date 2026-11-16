#!/usr/bin/env python3
"""Run 4.4.0 benchmark/parity checks and all prior release regressions."""
import subprocess, sys
from pathlib import Path
root = Path(__file__).resolve().parents[1]
for script in ('test-4.4.0-query-benchmark-and-parity.py', 'run-4.3.0-python-tests.py'):
    subprocess.run([sys.executable, str(root/'scripts'/script)], cwd=root, check=True)
print('PASS 4.4.0 benchmark/parity + 4.3.0 and earlier regression suites')
