using System.Text.Json;
using PersonalWiki.ExcelLab.Services;

namespace PersonalWiki.Desktop;

/// <summary>兩次 Excel 匯入批次的唯讀差異檢視及 JSON 匯出。</summary>
internal sealed class ExcelImportBatchCompareDialog : Form
{
    private readonly string _databasePath;
    private readonly ExcelImportBatchManagementService _management = new();
    private readonly ExcelImportBatchDiffService _diff = new();
    private readonly ComboBox _earlier = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 390 };
    private readonly ComboBox _later = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 390 };
    private readonly ListView _changes = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
    private readonly TextBox _details = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8), Text = "唯讀比較；不會修改資料庫。" };
    private ExcelImportBatchDiffResult? _lastResult;

    public ExcelImportBatchCompareDialog(string databasePath)
    {
        _databasePath = databasePath;
        DpiLayout.EnableDpiAutoScale(this);
        Text = "Excel 匯入批次差異比較（唯讀）";
        StartPosition = FormStartPosition.CenterParent;
        Width = 1180; Height = 760; MinimumSize = new Size(820, 540);
        _changes.Columns.Add("變更", 90); _changes.Columns.Add("條目", 260); _changes.Columns.Add("類型", 110); _changes.Columns.Add("欄位", 180); _changes.Columns.Add("來源工作表／列", 220);
        var selectors = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(8) };
        selectors.Controls.Add(new Label { Text = "較早批次：", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }); selectors.Controls.Add(_earlier);
        selectors.Controls.Add(new Label { Text = "較新批次：", AutoSize = true, Padding = new Padding(8, 7, 0, 0) }); selectors.Controls.Add(_later);
        var compare = new Button { Text = "比較批次", AutoSize = true }; compare.Click += (_, _) => Compare();
        var export = new Button { Text = "匯出 JSON…", AutoSize = true }; export.Click += (_, _) => ExportJson();
        var close = new Button { Text = "關閉", AutoSize = true, DialogResult = DialogResult.OK };
        selectors.Controls.AddRange(new Control[] { compare, export, close });
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 390, Panel1MinSize = 170, Panel2MinSize = 120 };
        split.Panel1.Controls.Add(_changes); split.Panel2.Controls.Add(_details);
        Controls.Add(split); Controls.Add(_status); Controls.Add(selectors);
        AcceptButton = compare;
        LoadBatches();
        DpiLayout.KeepInsideWorkingArea(this);
    }

    private void LoadBatches()
    {
        try
        {
            var batches = _management.List(_databasePath, 500).ToList();
            _earlier.Items.AddRange(batches.Cast<object>().ToArray());
            _later.Items.AddRange(batches.Cast<object>().ToArray());
            _earlier.DisplayMember = nameof(ExcelImportBatchSummary.StartedAt);
            _later.DisplayMember = nameof(ExcelImportBatchSummary.StartedAt);
            if (batches.Count >= 2) { _earlier.SelectedIndex = batches.Count - 1; _later.SelectedIndex = 0; }
            else if (batches.Count == 1) { _earlier.SelectedIndex = 0; _later.SelectedIndex = 0; }
            _status.Text = $"載入 {batches.Count} 筆批次；下拉選單以開始時間顯示，最多 500 筆。";
        }
        catch (Exception ex) { _status.Text = "讀取批次清單失敗。"; MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private void Compare()
    {
        if (_earlier.SelectedItem is not ExcelImportBatchSummary before || _later.SelectedItem is not ExcelImportBatchSummary after) return;
        try
        {
            _lastResult = _diff.Compare(_databasePath, before.Id, after.Id);
            _changes.Items.Clear();
            foreach (var change in _lastResult.Changes)
            {
                var item = new ListViewItem(change.ChangeType);
                var snapshot = change.After ?? change.Before;
                item.SubItems.Add(snapshot?.Title ?? change.Key);
                item.SubItems.Add(snapshot?.EntryType ?? "");
                item.SubItems.Add(string.Join(", ", change.ChangedFields));
                item.SubItems.Add(snapshot is null ? "" : $"{snapshot.SourceSheet} / {snapshot.SourceRow}");
                item.Tag = change;
                _changes.Items.Add(item);
            }
            var r = _lastResult;
            _details.Text = $"較早批次：{r.EarlierBatchId}\r\n較新批次：{r.LaterBatchId}\r\n\r\n較早條目數：{r.EarlierCount:N0}\r\n較新條目數：{r.LaterCount:N0}\r\n新增條目：{r.AddedCount:N0}\r\n移除條目：{r.RemovedCount:N0}\r\n修改條目：{r.ChangedCount:N0}\r\n\r\n欄位新增：{r.AddedFieldCount:N0}\r\n欄位移除：{r.RemovedFieldCount:N0}\r\n欄位修改：{r.ChangedFieldCount:N0}\r\n\r\n欄位差異摘要：\r\n" + string.Join("\r\n", r.FieldSummaries.Select(x => $"{x.FieldLabel} ({x.FieldKey})：新增 {x.AddedCount:N0}、移除 {x.RemovedCount:N0}、修改 {x.ChangedCount:N0}"));
            _status.Text = $"比較完成：新增 {r.AddedCount:N0}、移除 {r.RemovedCount:N0}、修改 {r.ChangedCount:N0} 筆條目。資料庫唯讀。";
        }
        catch (Exception ex) { _lastResult = null; _status.Text = "批次比較失敗。"; MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private void ExportJson()
    {
        if (_lastResult is null) { MessageBox.Show(this, "請先完成批次比較。", Text, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        using var dialog = new SaveFileDialog { Title = "匯出批次差異 JSON", Filter = "JSON 檔案 (*.json)|*.json", FileName = $"ExcelBatchDiff_{DateTime.Now:yyyyMMdd_HHmmss}.json", AddExtension = true, DefaultExt = "json" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var json = JsonSerializer.Serialize(_lastResult, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dialog.FileName, json, new System.Text.UTF8Encoding(false));
            _status.Text = $"已匯出 JSON：{dialog.FileName}";
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "匯出失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
}
