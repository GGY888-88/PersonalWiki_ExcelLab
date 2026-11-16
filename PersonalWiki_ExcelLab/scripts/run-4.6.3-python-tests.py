#!/usr/bin/env python3
"""Run 4.6.3 UI, DPI, BAT, and historical query regressions."""
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPTS = [
    "scripts/test-4.6.3-input-alignment.py",
    "scripts/test-4.6.2-dpi.py",
    "scripts/test-4.6.2-filter-layout.py",
    "scripts/test-4.6.2-responsive-tables.py",
    "scripts/test-4.6.2-viewport-space.py",
    "scripts/test-4.6.2-build-fix.py",
    "scripts/test-4.6.2-main-ui-refine.py",
    "scripts/run-4.6.1-python-tests.py",
    "scripts/test-4.6.0-query-acceptance.py",
]
for script in SCRIPTS:
    print(f"\n=== {script} ===", flush=True)
    result = subprocess.run([sys.executable, str(ROOT / script)], cwd=ROOT)
    if result.returncode:
        raise SystemExit(result.returncode)
print("PASS: 4.6.3 input alignment and BAT checks plus retained 4.6.2 / 4.6.1 / 4.6.0 regressions")
