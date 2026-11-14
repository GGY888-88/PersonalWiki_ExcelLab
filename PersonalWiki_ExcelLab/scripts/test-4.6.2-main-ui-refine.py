from pathlib import Path

root = Path(__file__).resolve().parents[1]
main = (root / "PersonalWiki.Desktop" / "MainForm.cs").read_text(encoding="utf-8-sig")
settings = (root / "PersonalWiki.Desktop" / "DesktopSettingsService.cs").read_text(encoding="utf-8-sig")
centered = (root / "PersonalWiki.Desktop" / "VerticallyCenteredTextBox.cs").read_text(encoding="utf-8")
checks = {
    "main button setting lower bound 40": "Math.Clamp(width, 40, 360)" in settings,
    "main button height lower bound 20": "Math.Clamp(height, 20, 160)" in settings,
    "main button settings dialog lower bound": "minimumWidth: 40, minimumHeight: 20" in main,
    "main button compact padding": "button.Padding = new Padding(2, 1, 2, 1)" in main,
    "main button compact font": "new Font(Font.FontFamily, 9F" in main,
    "keyword uses vertically centered textbox": "VerticallyCenteredTextBox _keyword" in main,
    "tag filter uses vertically centered textbox": "VerticallyCenteredTextBox _tagFilter" in main,
    "database path uses vertically centered textbox": "VerticallyCenteredTextBox _database" in main,
    "native single-line EDIT mode and layout-controlled height are used": "Multiline = false" in centered and "AutoSize = false" in centered and "Multiline = true" not in centered,
    "build bat exists": (root / "01_建置程式.bat").exists(),
    "run bat exists": (root / "02_啟動程式.bat").exists(),
    "build and run bat exists": (root / "03_建置並啟動.bat").exists(),
    "build log retained without echoing it into CMD": "build_release.log" in (root / "01_建置程式.bat").read_text(encoding="ascii") and 'type "build_release.log"' not in (root / "01_建置程式.bat").read_text(encoding="ascii"),
    "build errors open the log in Notepad": 'notepad.exe "build_release.log"' in (root / "01_建置程式.bat").read_text(encoding="ascii"),
    "one-click build-run does not pause between build and launch": (lambda b: "dotnet build" in b and 'start "PersonalWiki ExcelLab"' in b and "pause" not in b[b.index("dotnet build"):b.index('start "PersonalWiki ExcelLab"')])((root / "03_建置並啟動.bat").read_text(encoding="ascii")),
}
for name, ok in checks.items():
    print(f"{'PASS' if ok else 'FAIL'}  {name}")
print(f"\n{sum(checks.values())}/{len(checks)} checks passed")
raise SystemExit(0 if all(checks.values()) else 1)
