#!/usr/bin/env python3
"""Static regression checks for adaptive tables in the 300% DPI UI."""
from pathlib import Path

root = Path(__file__).resolve().parents[1]
main = (root / "PersonalWiki.Desktop/MainForm.cs").read_text(encoding="utf-8-sig")
imports = (root / "PersonalWiki.Desktop/ImportDialogs.cs").read_text(encoding="utf-8")
checks = {
    "main result list uses an adaptive width layout":
        "ConfigureAdaptiveListViewColumns(\n            _results" in main,
    "related entries list uses an adaptive width layout":
        "ConfigureAdaptiveListViewColumns(\n            _related" in main,
    "adaptive list columns use live client width":
        "list.ClientSize.Width - DpiLayout.Scale(list, 8)" in main,
    "adaptive list columns reserve a small native border inset":
        "list.ClientSize.Width - DpiLayout.Scale(list, 8)" in main,
    "adaptive list columns scale readable minimums to active DPI":
        ".Select(width => DpiLayout.Scale(list, width))" in main,
    "adaptive list columns respond to resize and monitor DPI changes":
        "list.Resize += (_, _) => Apply();" in main and "DpiChanged += (_, _) => QueueApply();" in main,
    "remaining width is distributed after readable column minimums are reserved":
        "var minimumTotal = minimums.Sum();" in main and "var remaining = available - minimumTotal;" in main,
    "column width rounding remainder expands the text-heavy column":
        "widths[expandColumnIndex] += remaining - distributed;" in main,
    "only panes narrower than all readable minimums need horizontal scrolling":
        "if (available < minimumTotal)" in main and "Array.Copy(minimums, widths, minimums.Length);" in main,
    "Excel preview grid fills the available horizontal space":
        "AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill" in imports,
    "unused preview grid viewport uses a white window background instead of AppWorkspace grey":
        "BackgroundColor = SystemColors.Window" in imports and "BackColor = SystemColors.Window" in imports,
    "Excel preview fill weights are based on header and sample text":
        "MeasurePreviewColumnWidth(col.Name, sampleValues, grid.Font)" in imports and "FillWeight = preferredWidth" in imports,
    "Excel preview columns keep useful minimum widths":
        "MinimumWidth = Math.Min(preferredWidth, 180)" in imports,
    "Excel preview keeps wrapped text and auto-sized rows":
        "AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders" in imports and "WrapMode = DataGridViewTriState.True" in imports and "grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders)" in imports,
    "preview width measurement caps long text to avoid one column consuming the grid":
        "value.Length > 36 ? value[..36] : value" in imports and "Math.Clamp(maximum + 28, 90, 480)" in imports,
}
for label, passed in checks.items():
    print(("PASS" if passed else "FAIL") + " - " + label)
if not all(checks.values()):
    raise SystemExit(1)
print(f"PASS: {sum(checks.values())}/{len(checks)} responsive-table checks")
