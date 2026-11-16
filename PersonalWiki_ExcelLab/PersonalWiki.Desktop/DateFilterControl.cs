using System.ComponentModel;

namespace PersonalWiki.Desktop;

/// <summary>
/// A date filter made from two independent native controls: an enable checkbox
/// and a date picker. Keeping them separate prevents DateTimePicker.ShowCheckBox
/// from squeezing/overlapping the date text at high Windows display scaling.
/// </summary>
internal sealed class DateFilterControl : Panel
{
    private readonly CheckBox _enabled = new()
    {
        Text = string.Empty,
        AutoSize = true,
        UseVisualStyleBackColor = true,
        AccessibleName = "啟用日期篩選",
        TabStop = true,
        Margin = Padding.Empty
    };

    private readonly DateTimePicker _date = new()
    {
        Width = 150,
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "yyyy-MM-dd",
        ShowCheckBox = false,
        Margin = Padding.Empty
    };

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Checked
    {
        get => _enabled.Checked;
        set => _enabled.Checked = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DateTime Value
    {
        get => _date.Value;
        set => _date.Value = value;
    }

    public DateFilterControl()
    {
        AutoSize = false;
        Padding = Padding.Empty;
        Margin = Padding.Empty;
        TabStop = false;
        AccessibleRole = AccessibleRole.Grouping;
        Controls.Add(_enabled);
        Controls.Add(_date);

        _enabled.CheckedChanged += (_, _) => _date.Enabled = _enabled.Checked;
        _date.Enabled = false;
        RefreshDpiLayout();
    }

    /// <summary>
    /// Recomputes bounds using the active monitor DPI. The checkbox and date
    /// picker are positioned as separate siblings with a physical gap between
    /// them; neither control shares the other's client area.
    /// </summary>
    public void RefreshDpiLayout()
    {
        if (IsDisposed || _enabled.IsDisposed || _date.IsDisposed) return;

        var dpi = Math.Max(96, DeviceDpi);
        var scale = dpi / 96F;
        var desiredDateWidth = Math.Max(150, (int)Math.Ceiling(150 * scale));
        if (_date.Width != desiredDateWidth)
            _date.Width = desiredDateWidth;

        var checkSize = _enabled.GetPreferredSize(Size.Empty);
        var dateSize = _date.GetPreferredSize(Size.Empty);
        var gap = Math.Max(5, (int)Math.Ceiling(6 * scale));
        var contentHeight = Math.Max(checkSize.Height, Math.Max(_date.Height, dateSize.Height));
        var panelHeight = Math.Max(Height, contentHeight);

        _enabled.Location = new Point(0, Math.Max(0, (panelHeight - checkSize.Height) / 2));
        _enabled.Size = checkSize;
        _date.Location = new Point(checkSize.Width + gap, Math.Max(0, (panelHeight - _date.Height) / 2));

        var requiredWidth = checkSize.Width + gap + _date.Width;
        if (Width != requiredWidth) Width = requiredWidth;
        if (Height != panelHeight) Height = panelHeight;
        MinimumSize = new Size(requiredWidth, contentHeight);
        PerformLayout();
    }
}
