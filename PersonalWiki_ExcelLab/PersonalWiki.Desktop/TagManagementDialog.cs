using PersonalWiki.ExcelLab.Services;
using System.Text;

namespace PersonalWiki.Desktop;

/// <summary>標籤清單、重新命名/合併、刪除及 CSV 匯出。</summary>
internal sealed class TagManagementDialog : Form
{
    private readonly string _databasePath;
    private readonly TagManagementService _service = new();
    private readonly ListView _list = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = false };
    private readonly Label _status = new() { AutoSize = true, Text = "標籤管理不會刪除條目或原始圖片。" };

    public TagManagementDialog(string databasePath)
    {
        _databasePath = databasePath;
        DpiLayout.EnableDpiAutoScale(this);
        Text = "標籤管理"; StartPosition = FormStartPosition.CenterParent;
        Width = 760; Height = 560; MinimumSize = new Size(520, 360);
        _list.Columns.Add("標籤", 420); _list.Columns.Add("使用條目數", 130);
        var rename = new Button { Text = "重新命名／合併", AutoSize = true };
        var delete = new Button { Text = "刪除標籤", AutoSize = true };
        var export = new Button { Text = "匯出 CSV", AutoSize = true };
        var usageReport = new Button { Text = "使用統計報告", AutoSize = true };
        var refresh = new Button { Text = "重新整理", AutoSize = true };
        var close = new Button { Text = "關閉", AutoSize = true, DialogResult = DialogResult.OK };
        rename.Click += (_, _) => RenameSelected(); delete.Click += (_, _) => DeleteSelected(); export.Click += (_, _) => Export();
        usageReport.Click += (_, _) => ExportUsageReport(); refresh.Click += (_, _) => RefreshList();
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(6), WrapContents = true, AutoScroll = true };
        actions.Controls.AddRange(new Control[] { rename, delete, export, usageReport, refresh, close });
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 30, Padding = new Padding(8, 4, 4, 2) }; footer.Controls.Add(_status);
        Controls.Add(_list); Controls.Add(footer); Controls.Add(actions);
        AcceptButton = close; RefreshList();
        DpiLayout.KeepInsideWorkingArea(this);
    }

    private string? SelectedTag() => _list.SelectedItems.Count == 0 ? null : _list.SelectedItems[0].Text;

    private void RefreshList()
    {
        _list.BeginUpdate(); _list.Items.Clear();
        foreach (var item in _service.List(_databasePath))
        { var row = new ListViewItem(item.Tag); row.SubItems.Add(item.EntryCount.ToString()); _list.Items.Add(row); }
        _list.EndUpdate(); _status.Text = $"共 {_list.Items.Count} 個標籤。重新命名相同標準化名稱時會安全合併。";
    }

    private void RenameSelected()
    {
        var oldTag = SelectedTag(); if (oldTag is null) { MessageBox.Show(this, "請先選取標籤。", "標籤管理"); return; }
        using var prompt = new Form { Text = "重新命名標籤", Width = 430, Height = 160, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MinimizeBox = false, MaximizeBox = false };
        DpiLayout.EnableDpiAutoScale(prompt);
        var input = new TextBox { Left = 18, Top = 18, Width = 370, Text = oldTag, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        var ok = new Button { Text = "確定", DialogResult = DialogResult.OK, Left = 230, Top = 62, Width = 75 };
        var cancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Left = 313, Top = 62, Width = 75 };
        prompt.Controls.AddRange(new Control[] { input, ok, cancel }); prompt.AcceptButton = ok; prompt.CancelButton = cancel;
        DpiLayout.KeepInsideWorkingArea(prompt);
        if (prompt.ShowDialog(this) != DialogResult.OK) return;
        try { var count = _service.Rename(_databasePath, oldTag, input.Text); RefreshList(); _status.Text = $"已處理 {count} 筆標籤關聯；遇到目標標籤重複時已合併。"; }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "重新命名失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private void DeleteSelected()
    {
        var tag = SelectedTag(); if (tag is null) { MessageBox.Show(this, "請先選取標籤。", "標籤管理"); return; }
        if (MessageBox.Show(this, $"確定刪除標籤「{tag}」？\n只會移除標籤與條目的關聯，不會刪除條目、筆記或圖片檔。", "確認刪除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { var count = _service.Delete(_databasePath, tag); RefreshList(); _status.Text = $"已移除 {count} 筆標籤關聯；條目及原始檔案未刪除。"; }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "刪除失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private void ExportUsageReport()
    {
        using var save = new SaveFileDialog
        {
            Title = "匯出標籤使用統計（唯讀 JSON 報告）",
            Filter = "JSON 報告|*.json",
            FileName = $"PersonalWiki-tag-usage-{DateTime.Now:yyyyMMdd-HHmmss}.json",
            OverwritePrompt = false
        };
        if (save.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var report = new DatabaseTagUsageReportService().Create(_databasePath, save.FileName);
            _status.Text = $"統計報告已匯出：{report.UniqueNormalizedTagCount} 個標籤、{report.TaggedEntryCount} 筆含標籤條目；資料庫未變更。";
            MessageBox.Show(this,
                $"標籤使用統計已完成。\n\n標籤數：{report.UniqueNormalizedTagCount}\n含標籤條目：{report.TaggedEntryCount}\n標籤關聯總數：{report.TotalTagAssignments}\n只使用一次：{report.SingleUseTagCount}\n使用多次：{report.MultiUseTagCount}\n\n報告：{report.ReportPath}\n\n本功能以唯讀方式讀取資料庫，不會修改任何資料。",
                "標籤使用統計", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "匯出標籤統計失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void Export()
    {
        using var save = new SaveFileDialog { Title = "匯出標籤清單", Filter = "CSV 檔案|*.csv", FileName = "PersonalWiki-tags.csv", OverwritePrompt = false };
        if (save.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var csv = new StringBuilder("\"標籤\",\"使用條目數\"\r\n");
            foreach (var item in _service.List(_databasePath)) csv.Append('"').Append(item.Tag.Replace("\"", "\"\"")).Append("\",").Append(item.EntryCount).Append("\r\n");
            SafeFileOutput.WriteAllText(save.FileName, csv.ToString(), new UTF8Encoding(true)); _status.Text = "標籤 CSV 已匯出；資料庫未變更。";
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "匯出失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
}
