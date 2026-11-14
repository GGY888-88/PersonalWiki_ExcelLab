#!/usr/bin/env python3
"""Static checks for the three deliberately compact main-form input boxes."""
from pathlib import Path
root = Path(__file__).resolve().parents[1]
src = (root / 'PersonalWiki.Desktop/MainForm.cs').read_text(encoding='utf-8-sig')
checks = {
    'database path height is 26 logical px': '_database.Height = 26;' in src and '_database.MinimumSize = new Size(180, 26);' in src,
    'database path fit baseline and padding are compact': 'EnsureTextFits(_database, this, 26, 4)' in src,
    'full-text search initial height is 26': 'Width = 360, Height = 26, AutoSize = false' in src,
    'full-text search fit baseline and padding are compact': 'EnsureTextFits(_keyword, this, 26, 4)' in src,
    'tag filter initial height is 26': 'Width = 140, Height = 26, AutoSize = false' in src,
    'tag filter fit baseline and padding are compact': 'EnsureTextFits(_tagFilter, this, 26, 4)' in src,
    'other known single-line controls retain their existing sizing': 'Width = 220, Height = 32, PlaceholderText = "篩選檔名或圖片說明"' in src,
    'centered custom textbox implementation is unchanged': 'VerticallyCenteredTextBox _keyword' in src,
}
for label, ok in checks.items():
    print(('PASS' if ok else 'FAIL') + ' - ' + label)
print(f'{sum(checks.values())}/{len(checks)} static checks')
raise SystemExit(0 if all(checks.values()) else 1)
