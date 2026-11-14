#!/usr/bin/env python3
"""Static regression checks for the 300% viewport-space layout fixes."""
from pathlib import Path

root = Path(__file__).resolve().parents[1]
main = (root / "PersonalWiki.Desktop/MainForm.cs").read_text(encoding="utf-8-sig")
imports = (root / "PersonalWiki.Desktop/ImportDialogs.cs").read_text(encoding="utf-8")
checks = {
    "main toolbar uses a single explicit pixel layout pass":
        "private void LayoutMainChrome()" in main and "_mainTopLayout.SetBounds(0, 0, width, y)" in main,
    "database, keyword, type, archive and tag controls are in row one":
        'searchRowOne.Controls.Add(CreateSearchFilterGroup("資料庫："' in main
        and 'searchRowOne.Controls.Add(CreateSearchFilterGroup("全文搜尋："' in main
        and 'searchRowOne.Controls.Add(CreateSearchFilterGroup("類型："' in main
        and 'searchRowOne.Controls.Add(CreateSearchFilterGroup("封存狀態："' in main
        and 'searchRowOne.Controls.Add(CreateSearchFilterGroup("標籤："' in main,
    "both date ranges and sort are in row two":
        'searchRowTwo.Controls.Add(CreateSearchFilterGroup("建立日期起："' in main
        and 'searchRowTwo.Controls.Add(CreateSearchFilterGroup("更新日期起："' in main
        and 'searchRowTwo.Controls.Add(CreateSearchFilterGroup("排序："' in main,
    "filter rows remain horizontally scrollable without splitting caption/input groups":
        main.count("AutoScroll = true") >= 2 and "private Panel CreateSearchFilterGroup(string caption, Control input)" in main,
    "action buttons wrap as complete controls rather than being clipped":
        "AutoScroll = false" in main and "ArrangeActionButtonRows(_mainActionBar, width)" in main and "x + outerWidth > rightLimit" in main,
    "query actions occupy a dedicated visible row":
        "ArrangeHorizontalRow(_mainQueryBar, width)" in main and "_mainQueryBar.SetBounds(0, y, width, queryHeight)" in main,
    "ListView columns allocate minimum widths before distributing available width":
        "var minimumTotal = minimums.Sum();" in main and "var remaining = available - minimumTotal;" in main,
    "ListView columns reserve a native border inset to avoid tiny horizontal overflow":
        "list.ClientSize.Width - DpiLayout.Scale(list, 8)" in main,
    "Excel preview DataGridView is explicitly dock-filled":
        "Dock = DockStyle.Fill, ReadOnly = true" in imports,
    "Excel preview unused client area uses the normal white window background":
        "BackgroundColor = SystemColors.Window" in imports and "BackColor = SystemColors.Window" in imports,
    "Excel preview retains fill columns, wrapped text and automatic row heights":
        "AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill" in imports
        and "WrapMode = DataGridViewTriState.True" in imports
        and "AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders" in imports,
    "wizard footer remains separate from the preview table":
        "root.Controls.Add(tabs, 0, 0)" in imports and "root.Controls.Add(bottom, 0, 1)" in imports,
}
for label, passed in checks.items():
    print(("PASS" if passed else "FAIL") + " - " + label)
if not all(checks.values()):
    raise SystemExit(1)
print(f"PASS: {sum(checks.values())}/{len(checks)} viewport-space checks")
