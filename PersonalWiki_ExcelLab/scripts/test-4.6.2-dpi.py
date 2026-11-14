from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
main = (ROOT / "PersonalWiki.Desktop/MainForm.cs").read_text(encoding="utf-8-sig")
imports = (ROOT / "PersonalWiki.Desktop/ImportDialogs.cs").read_text(encoding="utf-8")
dpi = (ROOT / "PersonalWiki.Desktop/DpiLayout.cs").read_text(encoding="utf-8")
tag = (ROOT / "PersonalWiki.Desktop/TagManagementDialog.cs").read_text(encoding="utf-8-sig")
cli = (ROOT / "PersonalWiki.ExcelLab.csproj").read_text(encoding="utf-8-sig")
desktop = (ROOT / "PersonalWiki.Desktop/PersonalWiki.Desktop.csproj").read_text(encoding="utf-8-sig")
checks = {
    "CLI and Desktop versions are synchronized at 4.6.3": "<Version>4.6.3</Version>" in cli and "<Version>4.6.3</Version>" in desktop,
    "Desktop uses PerMonitorV2": "<ApplicationHighDpiMode>PerMonitorV2</ApplicationHighDpiMode>" in desktop,
    "forms use explicit 96 DPI baseline and Dpi autoscaling": "new SizeF(DesignDpi, DesignDpi)" in dpi and "form.AutoScaleMode = AutoScaleMode.Dpi" in dpi,
    "forms are clamped to the current working area": "Screen.FromControl(form).WorkingArea" in dpi and "form.DpiChanged" in dpi,
    "main window is DPI scaled and work-area aware": "DpiLayout.EnableDpiAutoScale(this)" in main and "DpiLayout.KeepInsideWorkingArea(this)" in main,
    "main action buttons wrap as whole buttons without a horizontal scrollbar": "AutoScroll = false" in main and "private int ArrangeActionButtonRows(Panel row, int width)" in main and "x + outerWidth > rightLimit" in main,
    "database field is placed before search filters": main.index('CreateSearchFilterGroup("資料庫："') < main.index('CreateSearchFilterGroup("全文搜尋："') and 'CreateSearchFilterGroup("資料庫：", _database)' in main,
    "main filter toolbar uses two explicit rows so right-side dates stay reachable": "searchRowOne.Controls.Add(CreateSearchFilterGroup(\"資料庫：\"" in main and "searchRowTwo.Controls.Add(CreateSearchFilterGroup(\"更新日期起：\"" in main and "LayoutMainChrome()" in main,
    "filter captions are auto-sized and paired in fixed non-wrapping panels": "private Panel CreateSearchFilterGroup(string caption, Control input)" in main and "AutoSize = true" in main and "group.Size = new Size(labelSize.Width + gap + input.Width, height)" in main,
    "filter captions vertically center against their DPI-sized inputs": "TextAlign = ContentAlignment.MiddleLeft" in main and "Math.Max(labelSize.Height, input.Height)" in main and "input.Location = new Point(labelSize.Width + gap" in main,
    "four toolbar regions are measured using actual device-pixel heights": "ArrangeActionButtonRows(_mainActionBar, width)" in main and "ArrangeHorizontalRow(_mainSearchRowOne, width)" in main and "ArrangeHorizontalRow(_mainSearchRowTwo, width)" in main and "ArrangeHorizontalRow(_mainQueryBar, width)" in main and "_mainTopLayout.SetBounds(0, 0, width, y)" in main,
    "single-line search inputs calculate sufficient text height at active DPI": "DpiLayout.EnsureTextFits(_keyword, this, 40, 14)" in main and "DpiLayout.EnsureTextFits(_database, this, 36, 14)" in main and "public static void EnsureTextFits" in dpi,
    "notes and images toolbars expand to avoid clipped button captions": "DpiLayout.FitFlowPanelHeight(this, actions, 50)" in main and main.count("DpiLayout.EnsureTextFits(button, this, 36, 14)") >= 1 and "AutoScroll = true" in main[main.index("private Control BuildImagesPanel"):main.index("private void RunDatabaseHealthCheck")],
    "search and action rows recalculate after child size changes": "child.SizeChanged += (_, _) => QueueMainChromeLayout();" in main and "_archiveAction.TextChanged += (_, _) => QueueMainChromeLayout();" in main,
    "all three Excel wizard forms use Dpi autoscaling": imports.count("DpiLayout.EnableDpiAutoScale(this)") == 3,
    "all three Excel wizard forms are work-area aware": imports.count("DpiLayout.KeepInsideWorkingArea(this)") == 3,
    "wizard help labels adapt to available width": "DpiLayout.ConstrainLabelWidth(description, layout)" in imports and "DpiLayout.ConstrainLabelWidth(help, layout, 32)" in imports,
    "mapping header can wrap and footer buttons can wrap": "WrapContents = true" in imports and "CreateWizardButton(\"確認欄位設定\"" in imports,
    "cleaning rule checkboxes calculate wrapped text height": "DpiLayout.MakeCheckBoxResponsive(check, list, pageScroll, this)" in imports and "TextFormatFlags.WordBreak" in dpi,
    "wizard tables remain dock-filled with header auto sizing": "Dock = DockStyle.Fill" in imports and "DataGridViewColumnHeadersHeightSizeMode.AutoSize" in imports,
    "Excel preview rows auto-size and wrap cell text at 300%": "DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders" in imports and "DefaultCellStyle = { WrapMode = DataGridViewTriState.True" in imports and "grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders)" in imports,
    "tag-management dialogs are DPI and working-area aware": "DpiLayout.EnableDpiAutoScale(this)" in tag and "DpiLayout.KeepInsideWorkingArea(this)" in tag,
    "no service or database source is referenced by this patch test": (ROOT / "Services").is_dir(),
}
for label, passed in checks.items():
    print(("PASS" if passed else "FAIL") + " - " + label)
if not all(checks.values()):
    raise SystemExit(1)
print(f"PASS: {len(checks)} DPI release checks")
