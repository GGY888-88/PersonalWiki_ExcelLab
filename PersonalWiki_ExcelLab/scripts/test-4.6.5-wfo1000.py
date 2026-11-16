#!/usr/bin/env python3
"""Static regression checks for the .NET 10 WinForms WFO1000 fix."""
from pathlib import Path
root = Path(__file__).resolve().parents[1]
src = (root / 'PersonalWiki.Desktop/VerticallyCenteredTextBox.cs').read_text(encoding='utf-8')
checks = {
    'ComponentModel namespace imported': 'using System.ComponentModel;' in src,
    'Text proxy is explicitly hidden from designer serialization': '[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]\n    public new string Text' in src,
    'PlaceholderText proxy is explicitly hidden from designer serialization': '[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]\n    public string PlaceholderText' in src,
    'ReadOnly proxy is explicitly hidden from designer serialization': '[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]\n    public bool ReadOnly' in src,
    'WFO1000 is not globally suppressed in Desktop project': 'WFO1000' not in (root / 'PersonalWiki.Desktop/PersonalWiki.Desktop.csproj').read_text(encoding='utf-8-sig'),
    'Native single-line editor and centering layout remain intact': 'Multiline = false' in src and 'AutoSize = true' in src and 'var top = Padding.Top + Math.Max(0, (availableHeight - editorHeight) / 2);' in src,
}
for label, ok in checks.items():
    print(('PASS' if ok else 'FAIL') + ' - ' + label)
print(f'PASS: {sum(checks.values())}/{len(checks)} WFO1000 checks' if all(checks.values()) else f'FAIL: {sum(checks.values())}/{len(checks)} WFO1000 checks')
raise SystemExit(0 if all(checks.values()) else 1)
