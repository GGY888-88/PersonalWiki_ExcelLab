#!/usr/bin/env python3
"""Run 4.6.4 UI checks plus retained 4.6.2, 4.6.1, and query regressions."""
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPTS = [
    "scripts/test-4.6.5-wfo1000.py",
    "scripts/test-4.6.4-input-alignment.py",
    "scripts/test-4.6.4-dpi-regression.py",
    "scripts/test-4.6.2-filter-layout.py",
    "scripts/test-4.6.2-responsive-tables.py",
    "scripts/test-4.6.2-viewport-space.py",
    "scripts/test-4.6.2-build-fix.py",
    "scripts/test-4.6.4-main-ui-regression.py",
    "scripts/run-4.6.1-python-tests.py",
    "scripts/test-4.6.0-query-acceptance.py",
]
for script in SCRIPTS:
    print(f"\n=== {script} ===", flush=True)
    result = subprocess.run([sys.executable, str(ROOT / script)], cwd=ROOT)
    if result.returncode:
        raise SystemExit(result.returncode)
print("PASS: 4.6.5 WFO1000 fix plus 4.6.4 composite input checks and retained 4.6.2 / 4.6.1 / 4.6.0 regressions")
