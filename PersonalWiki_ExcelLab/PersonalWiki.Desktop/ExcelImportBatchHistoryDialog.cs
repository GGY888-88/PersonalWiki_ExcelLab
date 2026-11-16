using PersonalWiki.ExcelLab.Services;

namespace PersonalWiki.Desktop;

/// <summary>唯讀檢視 Excel 匯入批次歷史及批次內容摘要。</summary>
internal sealed class ExcelImportBatchHistoryDialog : Form
{
    private readonly string _databasePath;
    private readonly ExcelImportBatchManagementService _service = new();
    private readonly ListView _batches = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = false };
    private readonly TextBox _details = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8), Text = "此視窗以唯讀方式檢視匯入紀錄，不會修改資料庫。" };

    public ExcelImportBatchHistoryDialog(string databasePath)
    {
        _databasePath = databasePath;
        DpiLayout.EnableDpiAutoScale(this);
        Text = "Excel 匯入歷史（唯讀）";
        StartPosition = FormStartPosition.CenterParent;
        Width = 1120; Height = 700; MinimumSize = new Size(760, 480);
        _batches.Columns.Add("開始時間", 170);
        _batches.Columns.Add("狀態", 90);
        _batches.Columns.Add("來源檔案", 300);
        _batches.Columns.Add("來源列", 85);
        _batches.Columns.Add("匯入", 85);
        _batches.Columns.Add("拒絕", 85);
        _batches.Columns.Add("批次 ID", 220);
        _batches.SelectedIndexChanged += (_, _) => ShowSelectedBatch();
        var refresh = new Button { Text = "重新整理", AutoSize = true };
        var close = new Button { Text = "關閉", AutoSize = true, DialogResult = DialogResult.OK };
        refresh.Click += (_, _) => RefreshBatches();
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
        buttons.Controls.AddRange(new Control[] { refresh, close });
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300, Panel1MinSize = 150, Panel2MinSize = 120 };
        split.Panel1.Controls.Add(_batches);
        split.Panel2.Controls.Add(_details);
        Controls.Add(split); Controls.Add(_status); Controls.Add(buttons);
        AcceptButton = close;
        RefreshBatches();
        DpiLayout.KeepInsideWorkingArea(this);
    }

    private void RefreshBatches()
    {
        try
        {
            _batches.BeginUpdate();
            _batches.Items.Clear();
            foreach (var batch in _service.List(_databasePath, 500))
            {
                var row = new ListViewItem(batch.StartedAt);
                row.SubItems.Add(batch.Status);
                row.SubItems.Add(Path.GetFileName(batch.SourceFile));
                row.SubItems.Add(batch.SourceRows.ToString("N0"));
                row.SubItems.Add(batch.ImportedRows.ToString("N0"));
                row.SubItems.Add(batch.RejectedRows.ToString("N0"));
                row.SubItems.Add(batch.Id);
                row.Tag = batch.Id;
                _batches.Items.Add(row);
            }
            _status.Text = $"共 {_batches.Items.Count} 筆匯入批次。資料庫以唯讀模式開啟。";
            if (_batches.Items.Count > 0) _batches.Items[0].Selected = true;
            else _details.Text = "目前沒有 Excel 匯入批次紀錄。";
        }
        catch (Exception ex)
        {
            _status.Text = "讀取匯入歷史失敗。";
            MessageBox.Show(this, ex.Message, "Excel 匯入歷史", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally { _batches.EndUpdate(); }
    }

    private void ShowSelectedBatch()
    {
        if (_batches.SelectedItems.Count == 0) return;
        var id = Convert.ToString(_batches.SelectedItems[0].Tag);
        if (string.IsNullOrWhiteSpace(id)) return;
        try
        {
            var report = _service.Report(_databasePath, id);
            var b = report.Batch;
            _details.Text =
                $"批次 ID：{b.Id}\r\n來源檔案：{b.SourceFile}\r\nSHA-256：{b.SourceSha256}\r\n狀態：{b.Status}\r\n開始時間：{b.StartedAt}\r\n完成時間：{b.CompletedAt ?? "尚未完成"}\r\n\r\n來源列數：{b.SourceRows:N0}\r\n匯入列數：{b.ImportedRows:N0}\r\n拒絕列數：{b.RejectedRows:N0}\r\n\r\n條目：{report.EntryCount:N0}\r\n自訂欄位：{report.FieldCount:N0}\r\n筆記：{report.NoteCount:N0}\r\n圖片關聯：{report.ImageCount:N0}\r\n標籤關聯：{report.TagCount:N0}\r\n來源追溯：{report.SourceCount:N0}\r\n缺少全文索引：{report.MissingFtsCount:N0}\r\n外鍵違規：{report.ForeignKeyViolations:N0}\r\n\r\n備註：{b.Notes ?? "無"}\r\n\r\n本報告只讀取既有批次與關聯資料，不會修改或刪除任何內容。";
        }
        catch (Exception ex) { _details.Text = $"無法讀取批次摘要：{ex.Message}"; }
    }
}
