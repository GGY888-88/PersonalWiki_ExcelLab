using PersonalWiki.ExcelLab.Services;

namespace PersonalWiki.Desktop;

/// <summary>Read-only preview of a related entry suggestion.</summary>
internal sealed class RelatedEntryPreviewDialog : Form
{
    public RelatedEntryPreviewDialog(RelatedEntryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        DpiLayout.EnableDpiAutoScale(this);
        Text = "相關條目摘要（唯讀）";
        StartPosition = FormStartPosition.CenterParent;
        Width = 720;
        Height = 520;
        MinimumSize = new Size(460, 340);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(12),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddValueRow(layout, 0, "條目名稱", item.Title);
        AddValueRow(layout, 1, "類型", item.EntryType);
        AddValueRow(layout, 2, "更新時間", item.UpdatedAt);

        var summary = new TextBox
        {
            Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical, WordWrap = true,
            Text = string.IsNullOrWhiteSpace(item.Summary) ? "（沒有摘要）" : item.Summary
        };
        layout.Controls.Add(new Label { Text = "摘要", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 8, 0, 0) }, 0, 3);
        layout.Controls.Add(summary, 1, 3);

        var evidence = new TextBox
        {
            Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical, WordWrap = true,
            Text = $"共用標籤數：{item.SharedTagCount}{Environment.NewLine}" +
                   $"共用標籤：{(string.IsNullOrWhiteSpace(item.SharedTags) ? "（無）" : item.SharedTags)}{Environment.NewLine}" +
                   $"相同類型：{(item.SameType ? "是" : "否")}{Environment.NewLine}" +
                   $"推薦分數：{item.Score}"
        };
        layout.Controls.Add(new Label { Text = "關聯依據", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 8, 0, 0) }, 0, 4);
        layout.Controls.Add(evidence, 1, 4);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft, WrapContents = true
        };
        var close = new Button { Text = "關閉", AutoSize = true, DialogResult = DialogResult.OK };
        var copyTitle = new Button { Text = "複製條目名稱", AutoSize = true };
        copyTitle.Click += (_, _) =>
        {
            try { Clipboard.SetText(item.Title); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "複製失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        actions.Controls.Add(close);
        actions.Controls.Add(copyTitle);
        layout.Controls.Add(actions, 0, 5);
        layout.SetColumnSpan(actions, 2);

        Controls.Add(layout);
        AcceptButton = close;
        DpiLayout.KeepInsideWorkingArea(this);
    }

    private static void AddValueRow(TableLayoutPanel layout, int row, string label, string value)
    {
        layout.Controls.Add(new Label
        {
            Text = label, AutoSize = true, Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 8, 0, 8)
        }, 0, row);
        layout.Controls.Add(new Label
        {
            Text = value, AutoSize = true, MaximumSize = new Size(520, 0),
            Anchor = AnchorStyles.Left, Padding = new Padding(0, 8, 0, 8)
        }, 1, row);
    }
}
