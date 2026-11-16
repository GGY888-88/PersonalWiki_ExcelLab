#!/usr/bin/env python3
import subprocess, sys
from pathlib import Path
root = Path(__file__).resolve().parents[1]
for script in ["scripts/test-4.6.1-ui.py", "scripts/test-4.6.0-query-acceptance.py"]:
    print(f"\n=== {script} ===", flush=True)
    result = subprocess.run([sys.executable, str(root / script)], cwd=root)
    if result.returncode:
        raise SystemExit(result.returncode)
print("PASS: 4.6.1 UI checks and 4.6.0 acceptance regression")
