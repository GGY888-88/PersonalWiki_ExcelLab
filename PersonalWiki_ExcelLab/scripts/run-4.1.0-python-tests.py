#!/usr/bin/env python3
"""Run 4.1.0 desktop-result checks plus all 4.0.0 and earlier regressions."""
import subprocess, sys
from pathlib import Path
root = Path(__file__).resolve().parents[1]
for script in ('test-4.1.0-desktop-result-management.py', 'run-4.0.0-python-tests.py'):
    subprocess.run([sys.executable, str(root/'scripts'/script)], cwd=root, check=True)
print('PASS 4.1.0 desktop result management + 4.0.0/3.9.0/3.8.0/3.7.0/3.6.0 regressions')
