from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
main = (ROOT / "PersonalWiki.Desktop/MainForm.cs").read_text(encoding="utf-8-sig")
imports = (ROOT / "PersonalWiki.Desktop/ImportDialogs.cs").read_text(encoding="utf-8")
cli = (ROOT / "PersonalWiki.ExcelLab.csproj").read_text(encoding="utf-8-sig")
desktop = (ROOT / "PersonalWiki.Desktop/PersonalWiki.Desktop.csproj").read_text(encoding="utf-8-sig")
checks = {
    "main form separates action buttons, filter rows, and query controls in a pixel-based layout": "LayoutMainChrome()" in main and "ArrangeActionButtonRows(_mainActionBar, width)" in main and "ArrangeHorizontalRow(_mainSearchRowOne, width)" in main and "ArrangeHorizontalRow(_mainSearchRowTwo, width)" in main and "ArrangeHorizontalRow(_mainQueryBar, width)" in main,
    "database path is grouped before the full-text search filter": 'CreateSearchFilterGroup("資料庫：", _database)' in main and main.index('CreateSearchFilterGroup("資料庫："') < main.index('CreateSearchFilterGroup("全文搜尋："'),
    "search filters retain fixed non-wrapping groups across the two explicit rows": 'searchRowOne.Controls.Add(CreateSearchFilterGroup("資料庫："' in main and 'searchRowTwo.Controls.Add(CreateSearchFilterGroup("建立日期起："' in main and 'searchRowTwo.Controls.Add(CreateSearchFilterGroup("更新日期起："' in main and "AutoScroll = true" in main and "CreateSearchFilterGroup(string caption, Control input)" in main,
    "filter labels are non-wrapping and paired with their input in fixed panels": "private Panel CreateSearchFilterGroup(string caption, Control input)" in main and "AutoSize = true" in main and "group.Size = new Size(labelSize.Width + gap + input.Width, height)" in main,
    "dedicated database bar removed": "databaseBar" not in main,
    "first data row label uses adaptive dimensions": 'Text = "第一筆資料列：", AutoSize = true' in imports and "WrapContents = true" in imports,
    "import mapping header uses auto-sized wrapping layout": "SizeType.AutoSize" in imports and "AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink" in imports,
    "CLI and Desktop project versions remain synchronized": re.search(r"<Version>([^<]+)</Version>", cli).group(1) == re.search(r"<Version>([^<]+)</Version>", desktop).group(1),
}
for label, passed in checks.items():
    print(("PASS" if passed else "FAIL") + " - " + label)
if not all(checks.values()):
    raise SystemExit(1)
print(f"PASS: {len(checks)} UI release checks")
