using System.Text.Json;
using PersonalWiki.ExcelLab.Services;

namespace PersonalWiki.Desktop;

internal sealed class WorkbookPreviewDialog : Form
{
    public WorkbookPreviewDialog(ImportWorkbookPreview preview)
    {
        DpiLayout.EnableDpiAutoScale(this);
        Text = "Excel 匯入精靈－第 1 步：預覽來源資料";
        Width = 1180; Height = 820; MinimumSize = new Size(720, 460);
        StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.Sizable;
        Font = new Font("Microsoft JhengHei UI", 10F);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var tabs = new TabControl { Dock = DockStyle.Fill };
        foreach (var sheet in preview.Sheets)
        {
            var page = new TabPage($"{sheet.Name}（{sheet.DataRowCount} 列）");
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(6) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var description = new Label
            {
                Text = $"工作表狀態：{sheet.State}　標題列：{sheet.HeaderRow}　資料列：{sheet.DataRowCount}　空白列：{sheet.BlankRowCount}\r\n{sheet.HeaderDetection}\r\n此畫面只預覽 Excel，不會修改 Excel，也尚未寫入知識庫。",
                AutoSize = true, Padding = new Padding(8)
            };
            DpiLayout.ConstrainLabelWidth(description, layout);
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                // DataGridView defaults the unused client area to AppWorkspace grey.
                // Use the normal window surface so a short sample does not leave a
                // large grey block below its last data row.
                BackgroundColor = SystemColors.Window,
                GridColor = SystemColors.ControlLight,
                // Fill uses the whole preview pane instead of sizing columns only
                // to their short sample text (which left a large unused grey area).
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersWidth = DpiLayout.Scale(this, 55),
                RowTemplate = { Height = DpiLayout.Scale(this, 36), MinimumHeight = DpiLayout.Scale(this, 32) },
                Font = new Font("Microsoft JhengHei UI", 9.5F),
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True, BackColor = SystemColors.Window, ForeColor = SystemColors.WindowText, Padding = new Padding(DpiLayout.Scale(this, 5), DpiLayout.Scale(this, 4), DpiLayout.Scale(this, 5), DpiLayout.Scale(this, 4)) },
                ColumnHeadersDefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            };
            var rowNumberWidth = MeasurePreviewColumnWidth("Excel 列", sheet.PreviewRows.Select(row => row.ExcelRow.ToString()), grid.Font);
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ExcelRow",
                HeaderText = "Excel 列",
                FillWeight = rowNumberWidth,
                MinimumWidth = Math.Min(rowNumberWidth, 120),
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            foreach (var col in sheet.Columns)
            {
                var sampleValues = sheet.PreviewRows
                    .Select(row => col.Index > 0 && col.Index <= row.Values.Count
                        ? row.Values[col.Index - 1]
                        : string.Empty);
                var preferredWidth = MeasurePreviewColumnWidth(col.Name, sampleValues, grid.Font);
                grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = $"C{col.Index}",
                    HeaderText = col.Name,
                    FillWeight = preferredWidth,
                    MinimumWidth = Math.Min(preferredWidth, 180),
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
            }
            foreach (var row in sheet.PreviewRows)
                grid.Rows.Add(new object[] { row.ExcelRow }.Concat(row.Values.Cast<object>().ToArray()).ToArray());
            // Fill-mode columns now occupy the available width. Recalculate row
            // heights after the dialog is laid out so wrapped text uses final widths.
            grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders);
            grid.SizeChanged += (_, _) =>
            {
                if (!grid.IsDisposed && grid.Rows.Count > 0)
                    grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders);
            };
            layout.Controls.Add(description, 0, 0);
            layout.Controls.Add(grid, 0, 1);
            page.Controls.Add(layout);
            tabs.TabPages.Add(page);
        }

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft, WrapContents = true,
            Padding = new Padding(4, 8, 4, 4)
        };
        var next = CreateWizardButton("下一步：設定欄位用途", DialogResult.OK);
        var cancel = CreateWizardButton("取消（不匯入）", DialogResult.Cancel);
        bottom.Controls.AddRange(new Control[] { cancel, next });
        root.Controls.Add(tabs, 0, 0);
        root.Controls.Add(bottom, 0, 1);
        Controls.Add(root);
        AcceptButton = next; CancelButton = cancel;
        DpiLayout.KeepInsideWorkingArea(this);
    }

    private static int MeasurePreviewColumnWidth(string header, IEnumerable<string> values, Font font)
    {
        var maximum = TextRenderer.MeasureText(header ?? string.Empty, font).Width;
        foreach (var value in values)
        {
            if (string.IsNullOrEmpty(value)) continue;
            // Cap long samples so one paragraph cannot consume the entire grid;
            // wrapped cell text remains available by increasing the row height.
            var sample = value.Length > 36 ? value[..36] : value;
            maximum = Math.Max(maximum, TextRenderer.MeasureText(sample, font).Width);
        }
        return Math.Clamp(maximum + 28, 90, 480);
    }

    private static Button CreateWizardButton(string text, DialogResult result) => new()
    {
        Text = text, DialogResult = result, AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        MinimumSize = new Size(0, 40), Padding = new Padding(12, 7, 12, 7),
        Margin = new Padding(4)
    };
}

internal sealed class MappingEditorDialog : Form
{
    private readonly ImportMappingProfile _profile;
    private readonly Dictionary<string, (DataGridView Grid, NumericUpDown FirstDataRow)> _editors = new(StringComparer.Ordinal);

    private const string UnmappedLabel = "尚未設定（請選用途）";
    private const string TitleLabel = "條目名稱（每張表必選一欄）";
    private const string ContentLabel = "主要內容（摘要／介紹文字）";
    private const string AttributeLabel = "屬性欄位（保存為自訂欄位）";
    private const string IgnoreLabel = "忽略此欄（不匯入）";

    public MappingEditorDialog(ImportMappingProfile profile)
    {
        _profile = profile;
        DpiLayout.EnableDpiAutoScale(this);
        Text = "Excel 匯入精靈－第 2 步：設定欄位用途";
        Width = 1240; Height = 850; MinimumSize = new Size(720, 480);
        StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.Sizable;
        Font = new Font("Microsoft JhengHei UI", 10F);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var tabs = new TabControl { Dock = DockStyle.Fill };
        foreach (var sheet in profile.Sheets)
        {
            var page = new TabPage(sheet.SheetName);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(8) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var help = new Label
            {
                Text = "請逐欄選擇用途：\r\n• 條目名稱：每張要匯入的工作表必須選且只能選一欄，作為每筆資料的名稱。\r\n• 主要內容：會合併成條目的摘要／介紹文字。\r\n• 屬性欄位：保存在條目的「自訂欄位」頁籤；欄位代碼由程式自動建立，不必輸入。\r\n• 忽略此欄：確定不要匯入的欄位。整張工作表不匯入時，請將該表每一欄都設為忽略。尚未設定的欄位會在下一步提醒。",
                AutoSize = true, Padding = new Padding(4, 2, 4, 10)
            };
            DpiLayout.ConstrainLabelWidth(help, layout, 32);

            var firstRow = new NumericUpDown
            {
                Minimum = 1, Maximum = 1048576,
                Value = Math.Max(1, sheet.FirstDataRow), Width = 130,
                Height = 34, Margin = new Padding(4, 2, 8, 2)
            };
            var header = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(4), Margin = Padding.Empty,
                WrapContents = true, FlowDirection = FlowDirection.LeftToRight
            };
            header.Controls.Add(new Label
            {
                Text = $"Excel 標題列：第 {sheet.HeaderRow} 列", AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 6, 8, 2)
            });
            header.Controls.Add(new Label
            {
                Text = "第一筆資料列：", AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 6, 0, 2)
            });
            header.Controls.Add(firstRow);
            header.Controls.Add(new Label
            {
                Text = "（通常是標題列的下一列）", AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(4, 6, 0, 2)
            });

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersVisible = false, RowTemplate = { Height = 38 },
                Font = new Font("Microsoft JhengHei UI", 10F),
                EditMode = DataGridViewEditMode.EditOnEnter
            };
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Index", HeaderText = "Excel 欄號", ReadOnly = true, FillWeight = 14 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Source", HeaderText = "Excel 原欄位名稱", ReadOnly = true, FillWeight = 32 });
            var mode = new DataGridViewComboBoxColumn
            {
                Name = "Mode", HeaderText = "此欄要怎麼匯入？", FillWeight = 54,
                FlatStyle = FlatStyle.Flat
            };
            mode.Items.AddRange(new object[] { UnmappedLabel, TitleLabel, ContentLabel, AttributeLabel, IgnoreLabel });
            grid.Columns.Add(mode);
            foreach (var col in sheet.Columns) grid.Rows.Add(col.SourceIndex, col.SourceName, UnmappedLabel);

            _editors[sheet.SheetName] = (grid, firstRow);
            layout.Controls.Add(help, 0, 0);
            layout.Controls.Add(header, 0, 1);
            layout.Controls.Add(grid, 0, 2);
            page.Controls.Add(layout);
            tabs.TabPages.Add(page);
        }

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft, WrapContents = true,
            Padding = new Padding(4, 8, 4, 4)
        };
        var save = CreateWizardButton("確認欄位設定", DialogResult.OK);
        var cancel = CreateWizardButton("取消（不匯入）", DialogResult.Cancel);
        bottom.Controls.AddRange(new Control[] { cancel, save });
        root.Controls.Add(tabs, 0, 0);
        root.Controls.Add(bottom, 0, 1);
        Controls.Add(root);
        AcceptButton = save; CancelButton = cancel;
        DpiLayout.KeepInsideWorkingArea(this);
    }

    private static Button CreateWizardButton(string text, DialogResult result) => new()
    {
        Text = text, DialogResult = result, AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        MinimumSize = new Size(0, 40), Padding = new Padding(12, 7, 12, 7),
        Margin = new Padding(4)
    };

    public bool HasAnyImportSheet()
    {
        return _editors.Values.Any(editor =>
        {
            editor.Grid.EndEdit();
            var rows = editor.Grid.Rows.Cast<DataGridViewRow>().ToList();
            return rows.Count > 0 && rows.Any(row => !string.Equals(Convert.ToString(row.Cells["Mode"].Value), IgnoreLabel, StringComparison.Ordinal));
        });
    }

    public IReadOnlyList<string> GetSheetsWithoutTitle()
    {
        var missing = new List<string>();
        foreach (var pair in _editors)
        {
            pair.Value.Grid.EndEdit();
            var rows = pair.Value.Grid.Rows.Cast<DataGridViewRow>().ToList();
            var allIgnored = rows.Count > 0 && rows.All(row => string.Equals(Convert.ToString(row.Cells["Mode"].Value), IgnoreLabel, StringComparison.Ordinal));
            var count = rows.Count(row => string.Equals(Convert.ToString(row.Cells["Mode"].Value), TitleLabel, StringComparison.Ordinal));
            if (!allIgnored && count != 1) missing.Add(pair.Key);
        }
        return missing;
    }

    public void Save(string path)
    {
        var sheets = new List<ImportMappingSheet>();
        foreach (var original in _profile.Sheets)
        {
            var (grid, firstDataRow) = _editors[original.SheetName];
            grid.EndEdit();
            var columns = grid.Rows.Cast<DataGridViewRow>().Select(row =>
            {
                var index = Convert.ToInt32(row.Cells["Index"].Value);
                var displayMode = Convert.ToString(row.Cells["Mode"].Value) ?? UnmappedLabel;
                var mode = displayMode switch
                {
                    TitleLabel => "Title",
                    ContentLabel => "Content",
                    AttributeLabel => "Attribute",
                    IgnoreLabel => "Ignore",
                    _ => "Unmapped"
                };
                // 欄位代碼是資料庫內部識別值，交由程式按 Excel 欄號產生，避免使用者必須理解技術代碼。
                var targetField = mode == "Attribute" ? $"field_{index}" : null;
                return new ImportMappingColumn(index, Convert.ToString(row.Cells["Source"].Value) ?? "", targetField, mode, null);
            }).ToList();
            sheets.Add(original with { FirstDataRow = (uint)firstDataRow.Value, Columns = columns });
        }
        var updated = _profile with { Sheets = sheets };
        SafeFileOutput.WriteAllTextAtomicallyReplacing(path,
            JsonSerializer.Serialize(updated, new JsonSerializerOptions { WriteIndented = true }));
    }
}

internal sealed class CleaningRulesEditorDialog : Form
{
    private readonly ImportCleaningRulesProfile _profile;
    private readonly Dictionary<(string Sheet, string Rule), CheckBox> _checks = new();

    private static readonly Dictionary<string, string> RuleLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["missing-title"] = "條目名稱空白：檢查每筆資料是否有名稱",
        ["duplicate-title"] = "名稱重複：檢查同一工作表中是否有重複名稱",
        ["edge-whitespace"] = "前後空白：檢查欄位內容前後是否有空格",
        ["control-characters"] = "控制字元：檢查欄位是否含不可見控制字元",
        ["excel-error"] = "Excel 錯誤值：檢查 #REF!、#VALUE! 等錯誤"
    };

    public CleaningRulesEditorDialog(ImportCleaningRulesProfile profile)
    {
        _profile = profile;
        DpiLayout.EnableDpiAutoScale(this);
        Text = "Excel 匯入精靈－第 3 步：資料安全檢查";
        Width = 900; Height = 700; MinimumSize = new Size(640, 440);
        StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.Sizable;
        Font = new Font("Microsoft JhengHei UI", 10F);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(10) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var intro = new Label
        {
            Text = "以下檢查預設全部啟用。只要某個啟用項目發現問題，匯入就會暫停，不會自動修改 Excel 或跳過問題資料。\r\n取消勾選代表不檢查該類問題；請先確認自己能接受原始資料直接匯入。建議第一次使用全部保持勾選。",
            AutoSize = true, Padding = new Padding(4, 4, 4, 12)
        };
        DpiLayout.ConstrainLabelWidth(intro, root, 32);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        foreach (var sheet in profile.Sheets)
        {
            var page = new TabPage(sheet.SheetName);
            var pageScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var list = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = false, AutoScroll = false,
                FlowDirection = FlowDirection.TopDown, WrapContents = false,
                Padding = new Padding(10), Margin = Padding.Empty
            };
            pageScroll.Controls.Add(list);
            foreach (var rule in sheet.Rules)
            {
                var label = RuleLabels.GetValueOrDefault(rule.RuleId, rule.Description ?? rule.RuleId);
                var check = new CheckBox
                {
                    Text = label, Checked = rule.Enabled, AutoSize = false,
                    UseCompatibleTextRendering = true,
                    Padding = new Padding(2, 4, 2, 4), Margin = new Padding(2, 4, 2, 4)
                };
                _checks[(sheet.SheetName, rule.RuleId)] = check;
                list.Controls.Add(check);
                DpiLayout.MakeCheckBoxResponsive(check, list, pageScroll, this);
            }
            pageScroll.ClientSizeChanged += (_, _) => list.PerformLayout();
            page.Controls.Add(pageScroll);
            tabs.TabPages.Add(page);
        }

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft, WrapContents = true,
            Padding = new Padding(4, 8, 4, 4)
        };
        var save = CreateWizardButton("確認檢查項目", DialogResult.OK);
        var cancel = CreateWizardButton("取消（停止匯入）", DialogResult.Cancel);
        bottom.Controls.AddRange(new Control[] { cancel, save });
        root.Controls.Add(intro, 0, 0);
        root.Controls.Add(tabs, 0, 1);
        root.Controls.Add(bottom, 0, 2);
        Controls.Add(root);
        AcceptButton = save; CancelButton = cancel;
        DpiLayout.KeepInsideWorkingArea(this);
    }

    private static Button CreateWizardButton(string text, DialogResult result) => new()
    {
        Text = text, DialogResult = result, AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        MinimumSize = new Size(0, 40), Padding = new Padding(12, 7, 12, 7),
        Margin = new Padding(4)
    };

    public void Save(string path)
    {
        var sheets = _profile.Sheets.Select(sheet => sheet with
        {
            Rules = sheet.Rules.Select(rule => rule with
            {
                Enabled = _checks.TryGetValue((sheet.SheetName, rule.RuleId), out var check) ? check.Checked : rule.Enabled
            }).ToList()
        }).ToList();
        var updated = _profile with { Sheets = sheets };
        SafeFileOutput.WriteAllTextAtomicallyReplacing(path, JsonSerializer.Serialize(updated, new JsonSerializerOptions { WriteIndented = true }));
    }
}
