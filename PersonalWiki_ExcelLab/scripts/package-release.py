#!/usr/bin/env python3
"""Create a verified source ZIP with a stable extraction root and SHA256 manifest."""
import argparse, hashlib, os, re, shutil, tempfile, zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def sha256(path):
    h = hashlib.sha256()
    with path.open('rb') as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            h.update(chunk)
    return h.hexdigest()

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', help='ZIP output path; existing files are never overwritten')
    args = parser.parse_args()
    project = (ROOT/'PersonalWiki.ExcelLab.csproj').read_text(encoding='utf-8-sig')
    match = re.search(r'<Version>([^<]+)</Version>', project)
    if not match:
        raise SystemExit('Cannot determine project version')
    version = match.group(1)
    output = Path(args.output).expanduser().resolve() if args.output else ROOT.parent/f'PersonalWiki_ExcelLab_{version}.zip'
    if output.exists():
        raise SystemExit(f'Refusing to overwrite existing archive: {output}')
    output.parent.mkdir(parents=True, exist_ok=True)

    excluded_parts = {'.git', '__pycache__', 'bin', 'obj', '.vs'}
    files = [p for p in ROOT.rglob('*') if p.is_file()
             and not any(part in excluded_parts for part in p.relative_to(ROOT).parts)
             and p.suffix.lower() not in {'.pyc', '.pyo'}]
    manifest = ROOT/f'RELEASE_MANIFEST_{version}.sha256'
    if manifest.exists():
        files = [p for p in files if p != manifest]
    lines = [f'{sha256(p)}  {p.relative_to(ROOT).as_posix()}' for p in sorted(files)]
    manifest.write_text('PersonalWiki ExcelLab release file hashes\n'
                        f'Version: {version}\n'
                        'The manifest does not hash itself.\n' + '\n'.join(lines) + '\n', encoding='utf-8')
    files.append(manifest)

    temp_path = None
    created_output = False
    try:
        with tempfile.NamedTemporaryFile(prefix='personalwiki-', suffix='.zip.tmp', dir=output.parent, delete=False) as temp:
            temp_path = Path(temp.name)
        with zipfile.ZipFile(temp_path, 'w', compression=zipfile.ZIP_DEFLATED, compresslevel=9) as archive:
            for path in sorted(files):
                relative = path.relative_to(ROOT).as_posix()
                archive.write(path, f'PersonalWiki_ExcelLab/{relative}')
        with zipfile.ZipFile(temp_path, 'r') as archive:
            bad = archive.testzip()
            if bad:
                raise RuntimeError(f'ZIP CRC failure: {bad}')
            names = archive.namelist()
            if not names or any(not n.startswith('PersonalWiki_ExcelLab/') for n in names):
                raise RuntimeError('Unexpected ZIP root folder')
            if any(n.startswith(f'PersonalWiki_ExcelLab_{version}/') for n in names):
                raise RuntimeError('Versioned extraction folder is not allowed')
            required = ['README.md', f'docs/releases/HANDOFF_{version}.md', f'docs/releases/UPDATE_NOTES_{version}.md',
                        f'docs/releases/TEST_GUIDE_{version}.md', f'docs/releases/AUDIT_REPORT_{version}.md',
                        'samples/PersonalWiki_3.6.0_Test.db', f'scripts/run-{version}-python-tests.py', f'RELEASE_MANIFEST_{version}.sha256']
            missing = [name for name in required if f'PersonalWiki_ExcelLab/{name}' not in names]
            if missing:
                raise RuntimeError('Missing required archive content: ' + ', '.join(missing))
        # Exclusive creation: never overwrite a pre-existing release archive.
        with output.open('xb') as destination, temp_path.open('rb') as source:
            created_output = True
            shutil.copyfileobj(source, destination)
        with zipfile.ZipFile(output, 'r') as archive:
            if archive.testzip() is not None:
                raise RuntimeError('Final ZIP failed CRC verification')
        print(f'CREATED {output}')
        print(f'SIZE {output.stat().st_size}')
        print(f'FILES {len(files)}')
    except Exception:
        if created_output and output.exists():
            output.unlink()
        raise
    finally:
        if temp_path and temp_path.exists():
            temp_path.unlink()

if __name__ == '__main__':
    main()
