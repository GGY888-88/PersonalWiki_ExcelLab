#!/usr/bin/env python3
"""Static checks for the 4.6.2 WinForms build-error correction."""
from pathlib import Path

root = Path(__file__).resolve().parents[1]
dpi = (root / "PersonalWiki.Desktop/DpiLayout.cs").read_text(encoding="utf-8-sig")
query = (root / "Services/EntryQueryService.cs").read_text(encoding="utf-8-sig")
plan = (root / "Services/EntryQueryPlanService.cs").read_text(encoding="utf-8-sig")
date_filter = (root / "PersonalWiki.Desktop/DateFilterControl.cs").read_text(encoding="utf-8-sig")

checks = {
    "text sizing DPI owner is a Form": "EnsureTextFits(Control control, Form dpiOwner" in dpi,
    "toolbar DPI owner is a Form": "FitFlowPanelHeight(Form dpiOwner" in dpi,
    "wrapped toolbar height is measured across available width": "RequiredWrappedFlowPanelHeight(Control dpiOwner, FlowLayoutPanel panel" in dpi and "usedWidth + outerWidth > availableWidth" in dpi,
    "ControlAdded safely handles nullable Control": "if (e.Control is not { } addedControl) return;" in dpi,
    "new control events attach to the non-null local": all(
        token in dpi for token in [
            "addedControl.SizeChanged += (_, _) => Update();",
            "addedControl.FontChanged += (_, _) => Update();",
            "addedControl.TextChanged += (_, _) => Update();",
        ]
    ),
    "DateFilterControl.Checked is explicitly excluded from designer serialization":
        "[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]\n    public bool Checked" in date_filter,
    "DateFilterControl.Value is explicitly excluded from designer serialization":
        "[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]\n    public DateTime Value" in date_filter,
    "DateFilterControl imports designer serialization metadata":
        "using System.ComponentModel;" in date_filter,
    "query sort is normalized before lookup and reuse":
        "var normalizedSort = (sort ?? string.Empty).Trim();" in query
        and "SortSql.TryGetValue(normalizedSort, out var orderBy)" in query
        and "sort = normalizedSort.ToLowerInvariant();" in query,
    "query-plan sort is normalized before lookup and reuse":
        "var normalizedSort = (sort ?? string.Empty).Trim();" in plan
        and "SortSql.TryGetValue(normalizedSort, out var orderBy)" in plan
        and "sort = normalizedSort.ToLowerInvariant();" in plan,
}

for label, passed in checks.items():
    print(("PASS" if passed else "FAIL") + " - " + label)
if not all(checks.values()):
    raise SystemExit(1)
print(f"PASS: {len(checks)}/{len(checks)} build-fix static checks")
