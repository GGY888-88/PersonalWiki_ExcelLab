#!/usr/bin/env python3
"""Static regression checks for 4.6.3 TextBox vertical alignment and BAT output."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
main = (ROOT / "PersonalWiki.Desktop/MainForm.cs").read_text(encoding="utf-8-sig")
centered = (ROOT / "PersonalWiki.Desktop/VerticallyCenteredTextBox.cs").read_text(encoding="utf-8")
tag = (ROOT / "PersonalWiki.Desktop/TagManagementDialog.cs").read_text(encoding="utf-8-sig")
settings = (ROOT / "PersonalWiki.Desktop/DesktopSettingsService.cs").read_text(encoding="utf-8-sig")
desktop = (ROOT / "PersonalWiki.Desktop/PersonalWiki.Desktop.csproj").read_text(encoding="utf-8-sig")
cli = (ROOT / "PersonalWiki.ExcelLab.csproj").read_text(encoding="utf-8-sig")

checks = {
    "CLI and Desktop projects are both version 4.6.3": "<Version>4.6.3</Version>" in cli and "<Version>4.6.3</Version>" in desktop,
    "database path uses the vertical-centering textbox": "VerticallyCenteredTextBox _database" in main,
    "full-text search uses the vertical-centering textbox": "VerticallyCenteredTextBox _keyword" in main,
    "tag filter uses the vertical-centering textbox": "VerticallyCenteredTextBox _tagFilter" in main,
    "custom input uses native single-line EDIT mode": "Multiline = false" in centered and "Multiline = true" not in centered,
    "custom input keeps layout-controlled height without AutoSize": "AutoSize = false" in centered,
    "native single-line cue banner remains supported": "PlaceholderText" in main and "Multiline = false" in centered,
    "native rectangle P/Invoke positioning is removed": "EM_SETRECT" not in centered and "DllImport" not in centered and "SendMessage" not in centered,
    "single-line behavior does not accept Enter or Tab": "AcceptsReturn = false" in centered and "AcceptsTab = false" in centered,
    "image keyword remains a standard single-line field": "TextBox _imageKeyword" in main and "Multiline = true" not in main[main.index("_imageKeyword"):main.index("_imageSort")],
    "entry title and type remain native single-line fields": "TextBox _title" in main and "TextBox _type" in main and "Multiline = false" in main,
    "tag rename remains a native single-line field": "new TextBox" in tag and "Multiline = true" not in tag,
    "main action button size configuration remains unchanged": "Math.Clamp(width, 40, 360)" in settings and "Math.Clamp(height, 20, 160)" in settings,
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
checks["failed build log opens in Notepad instead of CMD type"] = 'notepad.exe "build_release.log"' in build_bat and 'type "build_release.log"' not in build_bat
checks["build-and-start has no pause between build and app launch"] = "pause" not in run_bat[run_bat.index("dotnet build"):run_bat.index('start "PersonalWiki ExcelLab"')]

for label, passed in checks.items():
    print(("PASS" if passed else "FAIL") + " - " + label)
if not all(checks.values()):
    raise SystemExit(1)
print(f"PASS: {len(checks)}/{len(checks)} 4.6.3 input-alignment and BAT checks")
