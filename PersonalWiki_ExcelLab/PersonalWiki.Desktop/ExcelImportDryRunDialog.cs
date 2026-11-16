using PersonalWiki.ExcelLab.Services;

namespace PersonalWiki.Desktop;

/// <summary>唯讀檢視 Excel Dry-run 問題，並依工作表／規則導覽列號樣本。</summary>
internal sealed class ExcelImportDryRunDialog : Form
{
    private sealed record IssueItem(string SheetName, ImportDryRunRuleResult Rule);

    private readonly ImportDryRunReport _report;
    private readonly ListView _issues = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = false };
    private readonly TextBox _details = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false };
    private readonly Label _summary = new() { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10), TextAlign = ContentAlignment.MiddleLeft };
    private readonly Button _copyRows = new() { Text = "複製 Excel 列號", AutoSize = true, Enabled = false };

    public ExcelImportDryRunDialog(ImportDryRunReport report)
    {
        _report = report ?? throw new ArgumentNullException(nameof(report));
        DpiLayout.EnableDpiAutoScale(this);
        Text = "Excel 匯入檢查問題明細（唯讀）";
        StartPosition = FormStartPosition.CenterParent;
        Width = 1120;
        Height = 720;
        MinimumSize = new Size(760, 500);
        Font = new Font("Microsoft JhengHei UI", 10F);

        _issues.Columns.Add("工作表", 180);
        _issues.Columns.Add("檢查項目", 230);
        _issues.Columns.Add("符合列數", 100);
        _issues.Columns.Add("列號樣本", 420);
        _issues.SelectedIndexChanged += (_, _) => ShowSelectedIssue();

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 350,
            Panel1MinSize = 160,
            Panel2MinSize = 120
        };
        split.Panel1.Controls.Add(_issues);
        split.Panel2.Controls.Add(_details);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            Padding = new Padding(8)
        };
        var cancel = new Button
        {
            Text = "取消本次匯入",
            AutoSize = true,
            MinimumSize = new Size(130, 40),
            DialogResult = DialogResult.Cancel
        };
        var revise = new Button
        {
            Text = "重新調整檢查項目",
            AutoSize = true,
            MinimumSize = new Size(170, 40),
            DialogResult = DialogResult.OK
        };
        _copyRows.Click += (_, _) => CopySelectedRows();
        bottom.Controls.AddRange(new Control[] { cancel, revise, _copyRows });

        _summary.Text = $"檢查發現 {_report.RowsRequiringReview:N0} 列需要人工確認；共 {_report.CandidateRows:N0} 列候選資料。請選取下方項目查看 Excel 列號。\r\n來源檔案：{_report.SourceFile}\r\n本視窗只顯示列號樣本，不讀取或保存儲存格內容，也不會修改 Excel 或資料庫。";
        Controls.Add(split);
        Controls.Add(bottom);
        Controls.Add(_summary);
        AcceptButton = revise;
        CancelButton = cancel;

        LoadIssues();
        DpiLayout.KeepInsideWorkingArea(this);
    }

    private void LoadIssues()
    {
        _issues.BeginUpdate();
        try
        {
            foreach (var sheet in _report.Sheets)
            foreach (var rule in sheet.Rules.Where(x => x.MatchedRows > 0))
            {
                var rowSamples = string.Join(", ", rule.RowSamples);
                if (rule.MatchedRows > rule.RowSamples.Count) rowSamples += "…";
                var item = new ListViewItem(sheet.SheetName);
                item.SubItems.Add(RuleLabel(rule.RuleId));
                item.SubItems.Add(rule.MatchedRows.ToString("N0"));
                item.SubItems.Add(rowSamples);
                item.Tag = new IssueItem(sheet.SheetName, rule);
                _issues.Items.Add(item);
            }
        }
        finally { _issues.EndUpdate(); }

        if (_issues.Items.Count > 0)
        {
            _issues.Items[0].Selected = true;
            _issues.Items[0].Focused = true;
        }
        else
        {
            _details.Text = "目前沒有符合條件的問題項目。";
        }
    }

    private void ShowSelectedIssue()
    {
        if (_issues.SelectedItems.Count == 0 || _issues.SelectedItems[0].Tag is not IssueItem selected)
        {
            _copyRows.Enabled = false;
            return;
        }

        var rule = selected.Rule;
        var rows = rule.RowSamples.Count == 0 ? "（沒有列號樣本）" : string.Join(", ", rule.RowSamples);
        var remaining = rule.MatchedRows > rule.RowSamples.Count
            ? $"\r\n另有 {rule.MatchedRows - rule.RowSamples.Count:N0} 列未列入樣本；每項規則最多顯示 20 個列號。"
            : string.Empty;
        _details.Text =
            $"工作表：{selected.SheetName}\r\n" +
            $"檢查項目：{RuleLabel(rule.RuleId)}（{rule.RuleId}）\r\n" +
            $"符合列數：{rule.MatchedRows:N0}\r\n\r\n" +
            $"Excel 列號樣本：\r\n{rows}{remaining}\r\n\r\n" +
            "處理方式：\r\n" +
            "1. 記下列號，回到來源 Excel 檢查對應資料。\r\n" +
            "2. 如需調整檢查規則，按「重新調整檢查項目」。\r\n" +
            "3. 如不打算繼續，按「取消本次匯入」。\r\n\r\n" +
            "安全限制：此報告只保存問題類型、計數及最多 20 個列號樣本，不包含儲存格原文；本視窗不會自動修正、略過或匯入問題資料。";
        _copyRows.Enabled = rule.RowSamples.Count > 0;
    }

    private void CopySelectedRows()
    {
        if (_issues.SelectedItems.Count == 0 || _issues.SelectedItems[0].Tag is not IssueItem selected || selected.Rule.RowSamples.Count == 0)
            return;
        Clipboard.SetText(string.Join(Environment.NewLine, selected.Rule.RowSamples));
    }

    private static string RuleLabel(string id) => id.ToLowerInvariant() switch
    {
        "missing-title" => "條目名稱空白",
        "duplicate-title" => "名稱重複",
        "edge-whitespace" => "欄位前後空白",
        "control-characters" => "欄位含控制字元",
        "excel-error" => "Excel 錯誤值",
        _ => id
    };
}
