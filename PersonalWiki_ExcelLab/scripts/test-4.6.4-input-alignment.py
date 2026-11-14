#!/usr/bin/env python3
"""Static regression checks for the 4.6.4 composite single-line input fix."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
main = (ROOT / "PersonalWiki.Desktop/MainForm.cs").read_text(encoding="utf-8-sig")
control = (ROOT / "PersonalWiki.Desktop/VerticallyCenteredTextBox.cs").read_text(encoding="utf-8")
settings = (ROOT / "PersonalWiki.Desktop/DesktopSettingsService.cs").read_text(encoding="utf-8-sig")
desktop = (ROOT / "PersonalWiki.Desktop/PersonalWiki.Desktop.csproj").read_text(encoding="utf-8-sig")
cli = (ROOT / "PersonalWiki.ExcelLab.csproj").read_text(encoding="utf-8-sig")

checks = {
    "CLI and Desktop projects are both version 4.6.5": "<Version>4.6.5</Version>" in cli and "<Version>4.6.5</Version>" in desktop,
    "custom vertical-centering control is an outer UserControl container": "class VerticallyCenteredTextBox : UserControl" in control,
    "inner editor remains a native single-line TextBox": "private readonly TextBox _editor" in control and "Multiline = false" in control,
    "inner editor uses native preferred height": "AutoSize = true" in control and "GetPreferredSize(new Size(availableWidth, 0)).Height" in control,
    "inner editor is placed inside the outer border container": "Controls.Add(_editor)" in control and "_editor.SetBounds(" in control,
    "inner editor is centered vertically from its preferred height": "(availableHeight - editorHeight) / 2" in control,
    "outer control preserves the existing sunken input border": "BorderStyle = BorderStyle.Fixed3D" in control,
    "native cue banner is retained through the inner TextBox": "get => _editor.PlaceholderText" in control and "set => _editor.PlaceholderText" in control,
    "read-only, clear, select, and copy operations proxy to the inner editor": all(x in control for x in ["get => _editor.ReadOnly", "set => _editor.ReadOnly", "_editor.Clear()", "_editor.SelectAll()", "_editor.Copy()"]),
    "text-changed events are synchronized to the outer control": "base.Text = _editor.Text" in control,
    "mouse double-click is forwarded for database path copy": "_editor.MouseDoubleClick += (_, e) => OnMouseDoubleClick(e)" in control,
    "database/search/tag fields remain the only custom wrapped fields": all(x in main for x in ["VerticallyCenteredTextBox _database", "VerticallyCenteredTextBox _keyword", "VerticallyCenteredTextBox _tagFilter"]),
    "database path read-only and copy behavior is retained": "VerticallyCenteredTextBox _database = new() { ReadOnly = true" in main and "_database.SelectAll(); _database.Copy();" in main,
    "full-text search Enter behavior is forwarded from the inner editor": "_editor.KeyDown += (_, e) => OnKeyDown(e)" in control and "_keyword.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { SearchEntries(); e.SuppressKeyPress = true; } };" in main,
    "tag and database tooltips are also attached to the actual inner editor": "_tips.SetToolTip(_tagFilter.Editor" in main and "_tips.SetToolTip(_database.Editor" in main,
    "image filter remains an untouched standard TextBox": "private readonly TextBox _imageKeyword" in main,
    "entry title and type remain their original standard single-line TextBoxes": "private readonly TextBox _title" in main and "private readonly TextBox _type" in main,
    "multi-line summary remains unchanged": "private readonly TextBox _summary = new() { Dock = DockStyle.Fill, Multiline = true" in main,
    "the old native-format-rectangle workaround is not used": all(token not in control for token in ["DllImport", "SendMessage", "WM_PAINT"]),
    "main action button size bounds remain unchanged": "Math.Clamp(width, 40, 360)" in settings and "Math.Clamp(height, 20, 160)" in settings,
    "PerMonitorV2 remains enabled": "<ApplicationHighDpiMode>PerMonitorV2</ApplicationHighDpiMode>" in desktop,
}

bat_names = ["01_建置程式.bat", "02_啟動程式.bat", "03_建置並啟動.bat"]
for name in bat_names:
    data = (ROOT / name).read_bytes()
    checks[f"{name} is ASCII and CRLF"] = data.decode("ascii") is not None and b"\r\n" in data and b"\n" not in data.replace(b"\r\n", b"")
    checks[f"{name} does not change CMD code page"] = b"chcp" not in data.lower()
    checks[f"{name} has no localized console literals"] = all(byte < 128 for byte in data)

build_bat = (ROOT / "01_建置程式.bat").read_text(encoding="ascii")
run_bat = (ROOT / "03_建置並啟動.bat").read_text(encoding="ascii")
checks["build output is redirected to a persistent log"] = '> "build_release.log" 2>&1' in build_bat
checks["failed build log opens in Notepad rather than CMD type"] = 'notepad.exe "build_release.log"' in build_bat and 'type "build_release.log"' not in build_bat
checks["build-and-start has no pause between build and app launch"] = "pause" not in run_bat[run_bat.index("dotnet build"):run_bat.index('start "PersonalWiki ExcelLab"')]

for label, passed in checks.items():
    print(("PASS" if passed else "FAIL") + " - " + label)
if not all(checks.values()):
    raise SystemExit(1)
print(f"PASS: {len(checks)}/{len(checks)} 4.6.4 input-alignment and BAT checks")
