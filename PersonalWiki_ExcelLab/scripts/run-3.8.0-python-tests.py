#!/usr/bin/env python3
"""Run 3.8.0 feature checks and all 3.7.0/3.6.0 regressions."""
import subprocess, sys
from pathlib import Path
root = Path(__file__).resolve().parents[1]
for script in ('test-3.8.0-image-relationship.py', 'run-3.7.0-python-tests.py'):
    subprocess.run([sys.executable, str(root/'scripts'/script)], cwd=root, check=True)
print('PASS 3.8.0 feature suite + 3.7.0/3.6.0 regression suites')
