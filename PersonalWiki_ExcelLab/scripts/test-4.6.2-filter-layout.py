#!/usr/bin/env python3
"""Static regression checks for stable 300% DPI toolbar layout."""
from pathlib import Path

root = Path(__file__).resolve().parents[1]
main = (root / "PersonalWiki.Desktop/MainForm.cs").read_text(encoding="utf-8-sig")
group_start = main.index("    private Panel CreateSearchFilterGroup(")
group_end = main.index("    private Control BuildTagsPanel()", group_start)
group_method = main[group_start:group_end]
layout_start = main.index("    private void LayoutMainChrome()")
layout_end = main.index("    private int ArrangeHorizontalRow(", layout_start)
layout_method = main[layout_start:layout_end]
wrap_start = main.index("    private int ArrangeActionButtonRows(")
wrap_end = main.index("    private int ArrangeHorizontalRow(", wrap_start)
wrap_method = main[wrap_start:wrap_end]
date_filter = (root / "PersonalWiki.Desktop/DateFilterControl.cs").read_text(encoding="utf-8")
checks = {
    "main toolbar uses separate action, filter and query panels":
        "top.Controls.Add(actionBar);" in main and "top.Controls.Add(searchRowOne);" in main and "top.Controls.Add(searchRowTwo);" in main and "top.Controls.Add(queryBar);" in main,
    "general search filters occupy the first fixed row":
        'searchRowOne.Controls.Add(CreateSearchFilterGroup("資料庫："' in main and 'searchRowOne.Controls.Add(CreateSearchFilterGroup("全文搜尋："' in main and 'searchRowOne.Controls.Add(CreateSearchFilterGroup("標籤："' in main,
    "date ranges and sorting occupy the second fixed row":
        'searchRowTwo.Controls.Add(CreateSearchFilterGroup("建立日期起："' in main and 'searchRowTwo.Controls.Add(CreateSearchFilterGroup("更新日期起："' in main and 'searchRowTwo.Controls.Add(CreateSearchFilterGroup("排序："' in main,
    "filter row groups remain horizontally scrollable as complete pairs":
        "private Panel CreateSearchFilterGroup(string caption, Control input)" in main and main.count("AutoScroll = true") >= 3,
    "filter labels cannot wrap into narrow vertical columns":
        "AutoSize = true" in group_method and "AutoEllipsis = false" in group_method,
    "caption and input use explicit positions in a fixed panel":
        "var group = new Panel" in group_method and "input.Location = new Point(labelSize.Width + gap" in group_method and "group.Size = new Size(labelSize.Width + gap + input.Width, height)" in group_method and "new FlowLayoutPanel" not in group_method,
    "query buttons and paging controls occupy a dedicated panel":
        "_mainQueryBar = queryBar" in main and "queryBar.Controls.AddRange" in main,
    "toolbar layout recalculates device-pixel heights independently":
        "ArrangeActionButtonRows(_mainActionBar, width)" in layout_method and "ArrangeHorizontalRow(_mainSearchRowOne, width)" in layout_method and "ArrangeHorizontalRow(_mainSearchRowTwo, width)" in layout_method and "ArrangeHorizontalRow(_mainQueryBar, width)" in layout_method,
    "action buttons wrap at available width without a horizontal scrollbar":
        "AutoScroll = false" in main and "x + outerWidth > rightLimit" in wrap_method and "row.AutoScrollMinSize = Size.Empty" in wrap_method,
    "action button rows preserve whole button bounds":
        "currentRow.Add((control, controlX))" in wrap_method and "item.Control.Location = new Point(item.X, top)" in wrap_method,
    "date filter uses an independent checkbox control":
        "private readonly CheckBox _enabled" in date_filter and "private readonly DateTimePicker _date" in date_filter,
    "native DateTimePicker checkbox is disabled to prevent internal overlap":
        "ShowCheckBox = false" in date_filter,
    "date picker and checkbox have explicitly separated bounds":
        "_date.Location = new Point(checkSize.Width + gap" in date_filter and "var requiredWidth = checkSize.Width + gap + _date.Width" in date_filter,
    "date filter preserves Checked and Value query semantics":
        "public bool Checked" in date_filter and "public DateTime Value" in date_filter and "DateValue(DateFilterControl picker)" in main,
    "date filter layouts refresh after DPI change":
        "RefreshDateFilterLayouts();" in main and "DpiChanged" in main,
}
for label, passed in checks.items():
    print(("PASS" if passed else "FAIL") + " - " + label)
if not all(checks.values()):
    raise SystemExit(1)
print(f"PASS: {sum(checks.values())}/{len(checks)} toolbar and date-filter checks")
