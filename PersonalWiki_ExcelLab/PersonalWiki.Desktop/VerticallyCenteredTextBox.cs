using System.ComponentModel;

namespace PersonalWiki.Desktop;

/// <summary>
/// Keeps the native single-line EDIT control at its preferred font height and
/// centers that editor inside a fixed-height, sunken container. A single-line
/// Win32 EDIT control does not honor EM_SETRECTNP; using the native preferred
/// height avoids top-aligned text without changing the input into a multiline
/// editor or replacing the native cue banner.
/// </summary>
internal sealed class VerticallyCenteredTextBox : UserControl
{
    private readonly TextBox _editor;

    public VerticallyCenteredTextBox()
    {
        BorderStyle = BorderStyle.Fixed3D;
        BackColor = SystemColors.Window;
        Padding = Padding.Empty;
        TabStop = false;
        Cursor = Cursors.IBeam;

        _editor = new TextBox
        {
            AutoSize = true,
            Multiline = false,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Window,
            TabStop = true,
            Margin = Padding.Empty,
            Dock = DockStyle.None
        };
        Controls.Add(_editor);

        _editor.TextChanged += (_, _) => SyncTextValue();
        _editor.KeyDown += (_, e) => OnKeyDown(e);
        _editor.MouseDoubleClick += (_, e) => OnMouseDoubleClick(e);
        _editor.FontChanged += (_, _) => LayoutEditor();
        _editor.HandleCreated += (_, _) => LayoutEditor();
        _editor.BackColor = BackColor;
        _editor.ForeColor = ForeColor;

        BackColorChanged += (_, _) => _editor.BackColor = BackColor;
        ForeColorChanged += (_, _) => _editor.ForeColor = ForeColor;
        SizeChanged += (_, _) => LayoutEditor();
        HandleCreated += (_, _) => LayoutEditor();
        LayoutEditor();
    }

    /// <summary>The text value exposed by the inner native TextBox.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new string Text
    {
        get => _editor.Text;
        set => _editor.Text = value ?? string.Empty;
    }

    /// <summary>Uses the native single-line EDIT cue banner.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string PlaceholderText
    {
        get => _editor.PlaceholderText;
        set => _editor.PlaceholderText = value ?? string.Empty;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ReadOnly
    {
        get => _editor.ReadOnly;
        set => _editor.ReadOnly = value;
    }

    /// <summary>Exposes the native editor for controls such as ToolTip.</summary>
    public TextBox Editor => _editor;

    public void Clear() => _editor.Clear();

    public void SelectAll() => _editor.SelectAll();

    public void Copy() => _editor.Copy();

    public new bool Focus() => _editor.Focus();

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && !_editor.Focused)
            _editor.Focus();
        base.OnMouseDown(e);
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        if (_editor is null) return;
        _editor.Font = Font;
        LayoutEditor();
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        LayoutEditor();
    }

    private void SyncTextValue()
    {
        if (!string.Equals(base.Text, _editor.Text, StringComparison.Ordinal))
            base.Text = _editor.Text;
    }

    private void LayoutEditor()
    {
        if (_editor is null || _editor.IsDisposed || IsDisposed)
            return;

        var availableWidth = Math.Max(1, ClientSize.Width - Padding.Horizontal);
        var availableHeight = Math.Max(1, ClientSize.Height - Padding.Vertical);
        var preferredHeight = _editor.GetPreferredSize(new Size(availableWidth, 0)).Height;
        var editorHeight = Math.Clamp(preferredHeight, 1, availableHeight);
        var top = Padding.Top + Math.Max(0, (availableHeight - editorHeight) / 2);

        _editor.SetBounds(
            Padding.Left,
            top,
            availableWidth,
            editorHeight);
    }
}
