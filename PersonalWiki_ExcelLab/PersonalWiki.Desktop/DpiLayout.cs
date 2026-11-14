namespace PersonalWiki.Desktop;

/// <summary>
/// Shared WinForms DPI and working-area helpers. All design dimensions use 96 DPI
/// logical pixels; WinForms performs the per-monitor scaling for the controls.
/// </summary>
internal static class DpiLayout
{
    private const float DesignDpi = 96F;

    public static void EnableDpiAutoScale(Form form)
    {
        // Explicit baseline is important for code-built forms (there is no designer
        // generated AutoScaleDimensions for these windows).
        form.AutoScaleDimensions = new SizeF(DesignDpi, DesignDpi);
        form.AutoScaleMode = AutoScaleMode.Dpi;
    }

    public static int Scale(Control control, int logicalPixels)
    {
        if (logicalPixels <= 0) return 0;
        var factor = Math.Max(0.5F, control.DeviceDpi / DesignDpi);
        return Math.Max(1, (int)Math.Ceiling(logicalPixels * factor));
    }

    public static void KeepInsideWorkingArea(Form form, int marginAt96Dpi = 16)
    {
        Size? minimumAt96Dpi = null;

        void Fit()
        {
            if (form.IsDisposed || !form.IsHandleCreated) return;
            var scale = Math.Max(0.5F, form.DeviceDpi / DesignDpi);
            minimumAt96Dpi ??= new Size(
                (int)Math.Round(form.MinimumSize.Width / scale),
                (int)Math.Round(form.MinimumSize.Height / scale));
            FitToWorkingArea(form, marginAt96Dpi, minimumAt96Dpi.Value);
        }

        form.Shown += (_, _) => Fit();
        form.DpiChanged += (_, _) =>
        {
            if (form.IsDisposed || !form.IsHandleCreated) return;
            try
            {
                form.BeginInvoke(new Action(Fit));
            }
            catch (InvalidOperationException)
            {
                // The form may be closing while Windows is delivering the DPI event.
            }
        };
    }

    private static void FitToWorkingArea(Form form, int marginAt96Dpi, Size minimumAt96Dpi)
    {
        var workingArea = Screen.FromControl(form).WorkingArea;
        if (workingArea.Width <= 0 || workingArea.Height <= 0) return;

        var margin = Scale(form, marginAt96Dpi);
        var maxWidth = Math.Max(160, workingArea.Width - margin * 2);
        var maxHeight = Math.Max(160, workingArea.Height - margin * 2);

        // A high-DPI scaled MinimumSize must never make the form larger than the
        // monitor's usable area. The main and wizard content remains scrollable.
        form.MinimumSize = new Size(
            Math.Min(Scale(form, minimumAt96Dpi.Width), maxWidth),
            Math.Min(Scale(form, minimumAt96Dpi.Height), maxHeight));

        var width = Math.Min(form.Width, maxWidth);
        var height = Math.Min(form.Height, maxHeight);
        if (form.Width != width || form.Height != height)
            form.Size = new Size(width, height);

        var minLeft = workingArea.Left + margin;
        var minTop = workingArea.Top + margin;
        var maxLeft = Math.Max(minLeft, workingArea.Right - margin - form.Width);
        var maxTop = Math.Max(minTop, workingArea.Bottom - margin - form.Height);
        form.Location = new Point(
            Math.Clamp(form.Left, minLeft, maxLeft),
            Math.Clamp(form.Top, minTop, maxTop));
    }


    /// <summary>
    /// Ensures that a single-line input or button has enough physical height for
    /// its current font at the active monitor DPI. The logical baseline is kept
    /// separate so moving between monitors can shrink and grow the control again.
    /// </summary>
    public static void EnsureTextFits(Control control, Form dpiOwner, int baseHeightAt96Dpi, int verticalPaddingAt96Dpi = 12)
    {
        var originalMinimumHeight = control.MinimumSize.Height;
        var updating = false;

        void Update()
        {
            if (updating || control.IsDisposed || dpiOwner.IsDisposed) return;
            updating = true;
            try
            {
                var glyphHeight = TextRenderer.MeasureText(
                    "Hg", control.Font, Size.Empty,
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Height;
                var measuredHeight = glyphHeight + Scale(dpiOwner, verticalPaddingAt96Dpi);
                var baselineHeight = Scale(dpiOwner, baseHeightAt96Dpi);
                var originalMinimum = originalMinimumHeight > 0
                    ? Scale(dpiOwner, originalMinimumHeight)
                    : 0;
                var requiredHeight = Math.Max(Math.Max(baselineHeight, measuredHeight), originalMinimum);

                control.MinimumSize = new Size(control.MinimumSize.Width, requiredHeight);
                if (!control.AutoSize || control is TextBoxBase)
                    control.Height = requiredHeight;
                control.PerformLayout();
            }
            finally
            {
                updating = false;
            }
        }

        control.FontChanged += (_, _) => Update();
        control.TextChanged += (_, _) => Update();
        control.HandleCreated += (_, _) => Update();
        dpiOwner.DpiChanged += (_, _) =>
        {
            if (dpiOwner.IsDisposed) return;
            try { dpiOwner.BeginInvoke(new Action(Update)); }
            catch (InvalidOperationException) { }
        };
        dpiOwner.Shown += (_, _) =>
        {
            if (dpiOwner.IsDisposed) return;
            try { dpiOwner.BeginInvoke(new Action(Update)); }
            catch (InvalidOperationException) { }
        };
        Update();
    }

    /// <summary>Returns a DPI-aware height sufficient for the tallest toolbar item.</summary>
    public static int RequiredFlowPanelHeight(Control dpiOwner, FlowLayoutPanel panel, int minimumAt96Dpi = 48)
    {
        var tallestItem = panel.Controls.Cast<Control>()
            .Where(control => control.Visible)
            .Select(control => Math.Max(Math.Max(control.Height, control.MinimumSize.Height), control.GetPreferredSize(Size.Empty).Height) + control.Margin.Vertical)
            .DefaultIfEmpty(0)
            .Max();
        var contentHeight = tallestItem + panel.Padding.Vertical + Scale(dpiOwner, 4);
        return Math.Max(Scale(dpiOwner, minimumAt96Dpi), contentHeight);
    }

    /// <summary>
    /// Measures a wrapping FlowLayoutPanel row-by-row using its current client width.
    /// This is used by the main filter toolbar so controls remain fully visible at
    /// high DPI without a horizontal scrollbar.
    /// </summary>
    public static int RequiredWrappedFlowPanelHeight(Control dpiOwner, FlowLayoutPanel panel, int minimumAt96Dpi = 48)
    {
        var availableWidth = Math.Max(1, panel.ClientSize.Width - panel.Padding.Horizontal);
        var usedWidth = 0;
        var currentRowHeight = 0;
        var totalHeight = 0;

        foreach (Control child in panel.Controls)
        {
            if (!child.Visible) continue;

            var preferred = child.GetPreferredSize(Size.Empty);
            var childWidth = Math.Max(child.Width, Math.Max(child.MinimumSize.Width, preferred.Width));
            var childHeight = Math.Max(child.Height, Math.Max(child.MinimumSize.Height, preferred.Height));
            var outerWidth = childWidth + child.Margin.Horizontal;
            var outerHeight = childHeight + child.Margin.Vertical;

            if (usedWidth > 0 && usedWidth + outerWidth > availableWidth)
            {
                totalHeight += currentRowHeight;
                usedWidth = 0;
                currentRowHeight = 0;
            }

            usedWidth += outerWidth;
            currentRowHeight = Math.Max(currentRowHeight, outerHeight);
        }

        var measuredHeight = totalHeight + currentRowHeight + panel.Padding.Vertical + Scale(dpiOwner, 4);
        return Math.Max(Scale(dpiOwner, minimumAt96Dpi), measuredHeight);
    }

    /// <summary>
    /// Keeps a top-docked FlowLayoutPanel tall enough for its current controls,
    /// including after font or monitor-DPI changes.
    /// </summary>
    public static void FitFlowPanelHeight(Form dpiOwner, FlowLayoutPanel panel, int minimumAt96Dpi = 48)
    {
        var updating = false;
        void Update()
        {
            if (updating || panel.IsDisposed || dpiOwner.IsDisposed) return;
            updating = true;
            try
            {
                var required = RequiredFlowPanelHeight(dpiOwner, panel, minimumAt96Dpi);
                if (panel.Height != required) panel.Height = required;
                panel.PerformLayout();
            }
            finally
            {
                updating = false;
            }
        }

        foreach (Control child in panel.Controls)
        {
            child.SizeChanged += (_, _) => Update();
            child.FontChanged += (_, _) => Update();
            child.TextChanged += (_, _) => Update();
        }
        panel.ControlAdded += (_, e) =>
        {
            // ControlEventArgs.Control is nullable in the WinForms annotations.
            if (e.Control is not { } addedControl) return;
            addedControl.SizeChanged += (_, _) => Update();
            addedControl.FontChanged += (_, _) => Update();
            addedControl.TextChanged += (_, _) => Update();
            Update();
        };
        dpiOwner.DpiChanged += (_, _) =>
        {
            if (dpiOwner.IsDisposed) return;
            try { dpiOwner.BeginInvoke(new Action(Update)); }
            catch (InvalidOperationException) { }
        };
        dpiOwner.Shown += (_, _) =>
        {
            if (dpiOwner.IsDisposed) return;
            try { dpiOwner.BeginInvoke(new Action(Update)); }
            catch (InvalidOperationException) { }
        };
        Update();
    }

    public static void ConstrainLabelWidth(Label label, Control host, int horizontalMarginAt96Dpi = 24)
    {
        void UpdateWidth()
        {
            if (host.ClientSize.Width <= 0) return;
            var margin = Scale(host, horizontalMarginAt96Dpi);
            label.MaximumSize = new Size(Math.Max(160, host.ClientSize.Width - margin), 0);
        }

        host.ClientSizeChanged += (_, _) => UpdateWidth();
        host.HandleCreated += (_, _) => UpdateWidth();
        label.HandleCreated += (_, _) => UpdateWidth();
        UpdateWidth();
    }

    public static void MakeCheckBoxResponsive(CheckBox checkBox, FlowLayoutPanel list, Panel viewport, Control dpiOwner)
    {
        checkBox.AutoSize = false;
        checkBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        void UpdateLayout()
        {
            if (viewport.ClientSize.Width <= 0) return;

            var horizontalMargin = Scale(dpiOwner, 20);
            list.Width = Math.Max(180, viewport.ClientSize.Width - horizontalMargin);
            checkBox.Width = Math.Max(160, list.ClientSize.Width - Scale(dpiOwner, 8));

            var textWidth = Math.Max(120, checkBox.Width - Scale(dpiOwner, 38));
            var textSize = TextRenderer.MeasureText(
                checkBox.Text,
                checkBox.Font,
                new Size(textWidth, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            checkBox.Height = Math.Max(
                Scale(dpiOwner, 34),
                textSize.Height + Scale(dpiOwner, 12));

            list.Height = list.Padding.Vertical + list.Controls.Cast<Control>()
                .Sum(control => control.Height + control.Margin.Vertical);
            list.PerformLayout();
        }

        viewport.ClientSizeChanged += (_, _) => UpdateLayout();
        checkBox.FontChanged += (_, _) => UpdateLayout();
        checkBox.TextChanged += (_, _) => UpdateLayout();
        checkBox.SizeChanged += (_, _) =>
        {
            if (!list.IsDisposed) list.Height = list.Padding.Vertical + list.Controls.Cast<Control>()
                .Sum(control => control.Height + control.Margin.Vertical);
        };
        UpdateLayout();
    }
}
