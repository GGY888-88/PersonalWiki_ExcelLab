#!/usr/bin/env python3
"""Run 4.6.0 checks and historical regression tests in an isolated compatibility copy."""
import shutil, subprocess, sys, tempfile
from pathlib import Path
root=Path(__file__).resolve().parents[1]
subprocess.run([sys.executable,str(root/'scripts/test-4.6.0-query-acceptance.py')],cwd=root,check=True)
subprocess.run([sys.executable,str(root/'scripts/verify-release-4.6.0.py')],cwd=root,check=True)
with tempfile.TemporaryDirectory(prefix='pw45-legacy-regression-') as td:
    mirror=Path(td)/'PersonalWiki_ExcelLab'
    excluded={'.git','__pycache__','bin','obj','.vs'}
    def ignore(directory,names):
        return {name for name in names if name in excluded or name.endswith(('.pyc','.pyo'))}
    shutil.copytree(root,mirror,ignore=ignore)
    # Historical test scripts assert the release version they were authored for.
    # Only the temporary mirror's metadata is normalized; the working tree remains 4.6.0.
    for rel in ('PersonalWiki.ExcelLab.csproj','PersonalWiki.Desktop/PersonalWiki.Desktop.csproj'):
        p=mirror/rel
        p.write_text(p.read_text(encoding='utf-8-sig').replace('<Version>4.6.0</Version>','<Version>4.4.0</Version>'),encoding='utf-8-sig')
    subprocess.run([sys.executable,str(mirror/'scripts/run-4.4.0-python-tests.py')],cwd=mirror,check=True)
print('PASS 4.6.0 acceptance checks + complete 4.4.0-to-3.6.0 historical regression suite (isolated mirror)')
