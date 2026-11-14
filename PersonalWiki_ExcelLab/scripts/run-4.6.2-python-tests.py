#!/usr/bin/env python3
import subprocess
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[1]
for script in [
    "scripts/test-4.6.2-dpi.py",
    "scripts/test-4.6.2-filter-layout.py",
    "scripts/test-4.6.2-responsive-tables.py",
    "scripts/test-4.6.2-viewport-space.py",
    "scripts/test-4.6.2-build-fix.py",
    "scripts/test-4.6.2-main-ui-refine.py",
    "scripts/run-4.6.1-python-tests.py",
    "scripts/test-4.6.0-query-acceptance.py",
]:
    print(f"\n=== {script} ===", flush=True)
    result = subprocess.run([sys.executable, str(root / script)], cwd=root)
    if result.returncode:
        raise SystemExit(result.returncode)
print("PASS: 4.6.2 DPI, filter-layout, responsive-table, viewport-space, MainUiRefine checks plus 4.6.1 / 4.6.0 Python regressions")
