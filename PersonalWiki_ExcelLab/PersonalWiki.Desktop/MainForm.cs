using PersonalWiki.ExcelLab.Services;
using System.Reflection;

namespace PersonalWiki.Desktop;

internal sealed class MainForm : Form
{
    private readonly VerticallyCenteredTextBox _database = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly VerticallyCenteredTextBox _keyword = new() { Width = 360, Height = 26, AutoSize = false, PlaceholderText = "搜尋名稱、摘要、筆記、自訂欄位、標籤及圖片說明" };
    private readonly ComboBox _typeFilter = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDown, Height = 34 };
    private readonly ComboBox _archiveFilter = new() { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList, Height = 34 };
    private readonly VerticallyCenteredTextBox _tagFilter = new() { Width = 140, Height = 26, AutoSize = false, PlaceholderText = "標籤" };
    private readonly ComboBox _sortFilter = new() { Width = 155, DropDownStyle = ComboBoxStyle.DropDownList, Height = 34 };
    // The date-enable checkbox is a separate control from DateTimePicker.
    // DateTimePicker.ShowCheckBox uses an internal native layout that can crowd
    // the date text at 300% DPI; DateFilterControl guarantees distinct bounds.
    private readonly DateFilterControl _createdFrom = new();
    private readonly DateFilterControl _createdTo = new();
    private readonly DateFilterControl _updatedFrom = new();
    private readonly DateFilterControl _updatedTo = new();
    private readonly Button _clearSearch = new() { Text = "清除條件", AutoSize = true };
    private readonly Button _copySelectedResults = new() { Text = "複製選取摘要", AutoSize = true, Enabled = false };
    private readonly Button _archiveAction = new() { Text = "封存選取條目", AutoSize = true, Enabled = false };
    private readonly ToolTip _tips = new();
    private readonly Button _search = new() { Text = "搜尋" };
    private readonly ListView _results = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = true };
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly TextBox _summary = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
    private readonly ListView _fields = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true };
    private readonly ListView _notes = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false };
    private readonly ListView _tags = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false };
    private readonly ListView _sources = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false };
    private readonly ListView _related = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false };
    private readonly ImageThumbnailCache _thumbnailCache = new(48);
    private readonly FlowLayoutPanel _images = new() { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true };
    private readonly TextBox _imageKeyword = new() { Width = 220, Height = 32, PlaceholderText = "篩選檔名或圖片說明" };
    private readonly ComboBox _imageSort = new() { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _imageCount = new() { AutoSize = true, Padding = new Padding(6, 8, 0, 0) };
    private List<object> _currentImages = new();
    private readonly Label _status = new() { AutoSize = true, Text = "請先建立或開啟知識庫資料庫。" };
    private string? _db;
    private string? _selectedEntryId;
    private string? _selectedImageId;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private const int PageSize = 100;
    private readonly DesktopSettingsService _settings = new();
    private readonly Label _pageInfo = new() { AutoSize = true, Padding = new Padding(6, 8, 6, 0), Text = "第 1 / 1 頁" };
    private readonly List<Button> _mainActionButtons = new();
    private Panel? _mainActionBar;
    private Panel? _mainTopLayout;
    private Panel? _mainSearchRowOne;
    private Panel? _mainSearchRowTwo;
    private Panel? _mainQueryBar;
    private bool _isApplyingMainActionLayout;

    public MainForm()
    {
        DpiLayout.EnableDpiAutoScale(this);
        Text = "PersonalWiki 個人知識庫";
        Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        Width = 1440; Height = 900; MinimumSize = new Size(860, 560);
        _keyword.Font = new Font(Font.FontFamily, 11F);
        _typeFilter.Font = new Font(Font.FontFamily, 10.5F);
        _archiveFilter.Font = new Font(Font.FontFamily, 10.5F);
        DpiLayout.EnsureTextFits(_keyword, this, 26, 4);
        DpiLayout.EnsureTextFits(_typeFilter, this, 38, 14);
        DpiLayout.EnsureTextFits(_archiveFilter, this, 38, 14);
        DpiLayout.EnsureTextFits(_tagFilter, this, 26, 4);
        DpiLayout.EnsureTextFits(_createdFrom, this, 38, 14);
        DpiLayout.EnsureTextFits(_createdTo, this, 38, 14);
        DpiLayout.EnsureTextFits(_updatedFrom, this, 38, 14);
        DpiLayout.EnsureTextFits(_updatedTo, this, 38, 14);
        RefreshDateFilterLayouts();
        _sortFilter.Items.AddRange(new object[] { "更新時間（新到舊）", "名稱（A 到 Z）", "名稱（Z 到 A）", "建立時間（新到舊）", "建立時間（舊到新）" }); _sortFilter.SelectedIndex = 0;
        _tips.SetToolTip(_tagFilter, "輸入完整標籤名稱，與其他搜尋條件採 AND 交集。");
        _tips.SetToolTip(_tagFilter.Editor, "輸入完整標籤名稱，與其他搜尋條件採 AND 交集。");
        _tips.SetToolTip(_createdFrom, "勾選日期欄位後才會套用建立日期起日。");
        _tips.SetToolTip(_createdTo, "勾選日期欄位後才會套用建立日期迄日（包含當日）。");
        _tips.SetToolTip(_updatedFrom, "勾選日期欄位後才會套用更新日期起日。");
        _tips.SetToolTip(_updatedTo, "勾選日期欄位後才會套用更新日期迄日（包含當日）。");
        _tips.SetToolTip(_sortFilter, "只改變結果顯示順序，不會修改資料庫內容。");
        _search.Height = 34; _search.AutoSize = true;
        _results.Columns.Add("名稱", 230); _results.Columns.Add("類型", 100); _results.Columns.Add("摘要", 360); _results.Columns.Add("更新時間", 150); _results.Columns.Add("狀態", 70);
        ConfigureAdaptiveListViewColumns(
            _results,
            new[] { 26F, 13F, 28F, 20F, 13F },
            new[] { 94, 58, 110, 88, 58 },
            expandColumnIndex: 2);
        _fields.Columns.Add("欄位", 180); _fields.Columns.Add("內容", 520); _fields.Columns.Add("類型", 100);
        _notes.Columns.Add("筆記內容", 650); _notes.Columns.Add("建立時間", 160); _notes.Columns.Add("更新時間", 160);
        _archiveFilter.Items.AddRange(new object[] { "未封存條目", "全部條目", "已封存條目（還原用）" }); _archiveFilter.SelectedIndex = 0;
        _archiveFilter.SelectionChangeCommitted += (_, _) => SearchEntries();
        _typeFilter.Items.Add("全部類型"); _typeFilter.SelectedIndex = 0;
        _tips.SetToolTip(_typeFilter, "可輸入清單中顯示的類型名稱，再按搜尋。留在「全部類型」代表不依類型篩選。");

        var open = new Button { Text = "開啟資料庫", AutoSize = true }; open.Click += (_, _) => OpenDatabase();
        var createDb = new Button { Text = "建立知識庫", AutoSize = true }; createDb.Click += (_, _) => CreateDatabase();
        var newEntry = new Button { Text = "新增條目", AutoSize = true }; newEntry.Click += (_, _) => CreateEntry();
        var editEntry = new Button { Text = "編輯條目", AutoSize = true }; editEntry.Click += (_, _) => EditEntry();
        _archiveAction.Click += (_, _) => ToggleArchive();
        var import = new Button { Text = "Excel 匯入精靈", AutoSize = true }; import.Click += (_, _) => ImportExcel();
        var uiSettings = new Button { Text = "輸入視窗按鈕大小", AutoSize = true }; uiSettings.Click += (_, _) => ConfigureEntryButtons();
        var mainUiSettings = new Button { Text = "主畫面功能按鈕大小", AutoSize = true }; mainUiSettings.Click += (_, _) => ConfigureMainActionButtons();
        var duplicateReport = new Button { Text = "重複條目檢查", AutoSize = true }; duplicateReport.Click += (_, _) => ShowDuplicateReport();
        var tagManagement = new Button { Text = "標籤管理", AutoSize = true }; tagManagement.Click += (_, _) => ShowTagManagement();
        var importHistory = new Button { Text = "Excel 匯入歷史", AutoSize = true }; importHistory.Click += (_, _) => ShowImportBatchHistory();
        var importCompare = new Button { Text = "Excel 批次差異比較", AutoSize = true }; importCompare.Click += (_, _) => ShowImportBatchCompare();
        var batchArchive = new Button { Text = "批次封存／還原", AutoSize = true }; batchArchive.Click += (_, _) => BatchToggleArchive();
        var imageAudit = new Button { Text = "圖片完整性檢查", AutoSize = true }; imageAudit.Click += (_, _) => RunImageIntegrityAudit();
        var imageDuplicates = new Button { Text = "重複圖片檢查", AutoSize = true }; imageDuplicates.Click += (_, _) => RunImageDuplicateAudit();
        var databaseHealth = new Button { Text = "資料庫健康檢查", AutoSize = true }; databaseHealth.Click += (_, _) => RunDatabaseHealthCheck();
        var databaseBackup = new Button { Text = "立即備份資料庫", AutoSize = true }; databaseBackup.Click += (_, _) => CreateDatabaseBackup();
        _tips.SetToolTip(databaseHealth, "以唯讀方式檢查 SQLite quick_check、外鍵關聯及 Schema 版本，不會修改資料庫。");
        _tips.SetToolTip(databaseBackup, "建立新的 SQLite 一致性備份；不會覆蓋已存在的檔案。");
        _tips.SetToolTip(batchArchive, "可在左側清單按住 Ctrl 或 Shift 多選條目，再批次封存或還原。只會變更封存狀態，不會刪除資料。");
        _tips.SetToolTip(duplicateReport, "唯讀檢查名稱相同的條目；不會自動合併、刪除或修改資料。");
        _tips.SetToolTip(_archiveFilter, "還原方式：切換到「已封存條目（還原用）」，選取要恢復的條目，再按「還原選取條目」。");
        _tips.SetToolTip(_archiveAction, "封存會從一般清單隱藏；還原請先在篩選選單顯示已封存條目。");
        _tips.SetToolTip(import, "將 Excel 資料匯入目前開啟的 SQLite 知識庫；原始 Excel 不會被修改。");
        var reopen = new Button { Text = "開啟上次資料庫", AutoSize = true }; reopen.Click += (_, _) => OpenLastDatabase();
        _search.Click += (_, _) => SearchEntries();
        _clearSearch.Click += (_, _) =>
        {
            _keyword.Clear(); _tagFilter.Clear(); _typeFilter.SelectedIndex = 0; _typeFilter.Text = "全部類型";
            _archiveFilter.SelectedIndex = 0; _sortFilter.SelectedIndex = 0;
            _createdFrom.Checked = false; _createdTo.Checked = false; _updatedFrom.Checked = false; _updatedTo.Checked = false;
            SearchEntries();
        };
        _keyword.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { SearchEntries(); e.SuppressKeyPress = true; } };
        var previousPage = new Button { Text = "上一頁", AutoSize = true }; previousPage.Click += (_, _) => ChangePage(-1);
        var nextPage = new Button { Text = "下一頁", AutoSize = true }; nextPage.Click += (_, _) => ChangePage(1);
        // 主畫面上方工具列改用單一、明確的像素配置器。
        // 不再混用 TableLayoutPanel 的 DPI 自動列高與程式手動列高，
        // 避免 300% 縮放時父容器高度小於實際子控制項高度。
        var actionBar = new Panel
        {
            // 功能按鈕由 ArrangeActionButtonRows 明確換行，不使用水平捲動。
            AutoScroll = false,
            Padding = new Padding(4),
            Margin = Padding.Empty,
            TabStop = false
        };
        var actionButtons = new Control[] { open, createDb, reopen, newEntry, editEntry, _archiveAction, import, uiSettings, mainUiSettings, duplicateReport, tagManagement, importHistory, importCompare, batchArchive, imageAudit, imageDuplicates, databaseHealth, databaseBackup };
        _mainActionButtons.AddRange(actionButtons.OfType<Button>());
        foreach (var button in _mainActionButtons)
        {
            button.AutoSize = true;
            button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button.UseCompatibleTextRendering = true;
            button.UseMnemonic = false;
            button.AutoEllipsis = false;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Margin = new Padding(4);
            button.Padding = new Padding(2, 1, 2, 1);
            button.Font = new Font(Font.FontFamily, 9F, FontStyle.Regular, GraphicsUnit.Point);
        }
        actionBar.Controls.AddRange(actionButtons);

        var top = new Panel
        {
            Dock = DockStyle.Top,
            Height = 220,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            TabStop = false
        };
        _mainActionBar = actionBar;
        _mainTopLayout = top;

        _database.Dock = DockStyle.None;
        _database.Width = 280;
        _database.Height = 26;
        _database.MinimumSize = new Size(180, 26);
        _database.Font = new Font(Font.FontFamily, 9.5F);
        _tips.SetToolTip(_database, "目前資料庫完整路徑。點選後可使用 Ctrl+A 全選、Ctrl+C 複製；若路徑較長，可使用左右方向鍵檢視。");
        _tips.SetToolTip(_database.Editor, "目前資料庫完整路徑。點選後可使用 Ctrl+A 全選、Ctrl+C 複製；若路徑較長，可使用左右方向鍵檢視。");
        DpiLayout.EnsureTextFits(_database, this, 26, 4);
        DpiLayout.EnsureTextFits(_sortFilter, this, 38, 14);
        _database.MouseDoubleClick += (_, _) => { _database.SelectAll(); _database.Copy(); };
        DpiLayout.EnsureTextFits(_search, this, 36, 14);
        DpiLayout.EnsureTextFits(_clearSearch, this, 36, 14);
        DpiLayout.EnsureTextFits(_copySelectedResults, this, 36, 14);
        DpiLayout.EnsureTextFits(previousPage, this, 36, 14);
        DpiLayout.EnsureTextFits(nextPage, this, 36, 14);

        Panel CreateHorizontalRow() => new()
        {
            AutoScroll = true,
            Padding = new Padding(4, 2, 4, 2),
            Margin = Padding.Empty,
            TabStop = false
        };

        var searchRowOne = CreateHorizontalRow();
        var searchRowTwo = CreateHorizontalRow();
        _mainSearchRowOne = searchRowOne;
        _mainSearchRowTwo = searchRowTwo;

        searchRowOne.Controls.Add(CreateSearchFilterGroup("資料庫：", _database));
        searchRowOne.Controls.Add(CreateSearchFilterGroup("全文搜尋：", _keyword));
        searchRowOne.Controls.Add(CreateSearchFilterGroup("類型：", _typeFilter));
        searchRowOne.Controls.Add(CreateSearchFilterGroup("封存狀態：", _archiveFilter));
        searchRowOne.Controls.Add(CreateSearchFilterGroup("標籤：", _tagFilter));

        searchRowTwo.Controls.Add(CreateSearchFilterGroup("建立日期起：", _createdFrom));
        searchRowTwo.Controls.Add(CreateSearchFilterGroup("迄：", _createdTo));
        searchRowTwo.Controls.Add(CreateSearchFilterGroup("更新日期起：", _updatedFrom));
        searchRowTwo.Controls.Add(CreateSearchFilterGroup("迄：", _updatedTo));
        searchRowTwo.Controls.Add(CreateSearchFilterGroup("排序：", _sortFilter));

        _copySelectedResults.Click += (_, _) => CopySelectedResultSummaries();
        _tips.SetToolTip(_copySelectedResults, "將目前頁面中 Ctrl／Shift 多選的條目摘要複製成文字；不會修改資料庫。");
        var queryBar = CreateHorizontalRow();
        _mainQueryBar = queryBar;
        queryBar.Controls.AddRange(new Control[] { _search, _clearSearch, _copySelectedResults, previousPage, _pageInfo, nextPage });
        DpiLayout.EnsureTextFits(_pageInfo, this, 36, 14);

        top.Controls.Add(queryBar);
        top.Controls.Add(searchRowTwo);
        top.Controls.Add(searchRowOne);
        top.Controls.Add(actionBar);

        // 任何一列的內容因 DPI、字型或文字改變尺寸時，重新計算整個上方區域。
        foreach (var row in new[] { actionBar, searchRowOne, searchRowTwo, queryBar })
        {
            foreach (Control child in row.Controls)
                child.SizeChanged += (_, _) => QueueMainChromeLayout();
        }
        _archiveAction.TextChanged += (_, _) => QueueMainChromeLayout();

        var details = new TabPage("條目內容"); details.Controls.Add(_summary);
        var fields = new TabPage("自訂欄位"); fields.Controls.Add(_fields);
        var notesPage = new TabPage("筆記"); notesPage.Controls.Add(BuildNotesPanel());
        var imagesPage = new TabPage("圖片"); imagesPage.Controls.Add(BuildImagesPanel());
        var tagsPage = new TabPage("標籤"); tagsPage.Controls.Add(BuildTagsPanel());
        var sourcesPage = new TabPage("來源追溯"); sourcesPage.Controls.Add(BuildSourcesPanel());
        var relatedPage = new TabPage("相關條目"); relatedPage.Controls.Add(BuildRelatedPanel());
        _tabs.TabPages.AddRange(new[] { details, fields, notesPage, imagesPage, tagsPage, sourcesPage, relatedPage });
        FormClosed += (_, _) => _thumbnailCache.Dispose();
        // SplitContainer 尚未取得實際版面寬度前不可設定 MinSize / SplitterDistance，
        // 否則初始化時 Width 仍是預設值，可能觸發 InvalidOperationException。
        var right = new SplitContainer { Dock = DockStyle.Fill };
        right.Panel1.Controls.Add(_results); right.Panel2.Controls.Add(_tabs);
        var bottom = new StatusStrip(); bottom.Items.Add(new ToolStripControlHost(_status));
        Controls.Add(right); Controls.Add(top); Controls.Add(bottom);
        Resize += (_, _) => QueueMainChromeLayout();
        Shown += (_, _) =>
        {
            RefreshDateFilterLayouts();
            ApplyMainActionButtonSize();
            LayoutMainChrome();
            // Shown 時 Dock 版面已完成，依實際可用寬度設定面板限制。
            right.Panel1MinSize = 300;
            right.Panel2MinSize = 400;
            var maxDistance = right.Width - right.Panel2MinSize;
            if (maxDistance >= right.Panel1MinSize)
                right.SplitterDistance = Math.Clamp(470, right.Panel1MinSize, maxDistance);
        };
        DpiChanged += (_, _) =>
        {
            try
            {
                BeginInvoke(new Action(() =>
                {
                    RefreshDateFilterLayouts();
                    ApplyMainActionButtonSize();
                    LayoutMainChrome();
                }));
            }
            catch (InvalidOperationException) { }
        };
        _results.SelectedIndexChanged += (_, _) =>
        {
            _copySelectedResults.Enabled = _results.SelectedItems.Count > 0;
            if (_results.SelectedItems.Count > 0) LoadEntry(_results.SelectedItems[0].Tag?.ToString());
            else { _selectedEntryId = null; _archiveAction.Enabled = false; _archiveAction.Text = "封存選取條目"; }
        };
        var last = _settings.Load();
        if (!string.IsNullOrWhiteSpace(last)) _status.Text = File.Exists(last) ? $"上次使用資料庫：{last}" : "找不到上次使用的資料庫；可重新選擇資料庫。";
        DpiLayout.KeepInsideWorkingArea(this);
    }


    private void RefreshDateFilterLayouts()
    {
        _createdFrom.RefreshDpiLayout();
        _createdTo.RefreshDpiLayout();
        _updatedFrom.RefreshDpiLayout();
        _updatedTo.RefreshDpiLayout();
    }

    /// <summary>
    /// Reflows ListView columns to use the available client width. ListView
    /// ColumnHeader widths are not consistently DPI-autoscaled by WinForms,
    /// so calculate against the live client width and enforce readable minima.
    /// When the pane is narrower than the combined minima, horizontal scrolling
    /// is preferred over squeezing headings and cell text into unusable widths.
    /// </summary>
    private void ConfigureAdaptiveListViewColumns(
        ListView list,
        float[] weights,
        int[] minimumWidthsAt96Dpi,
        int expandColumnIndex)
    {
        if (list.Columns.Count != weights.Length || weights.Length != minimumWidthsAt96Dpi.Length)
            throw new ArgumentException("ListView 欄位、權重及最小寬度設定數量必須一致。");
        if (expandColumnIndex < 0 || expandColumnIndex >= weights.Length)
            throw new ArgumentOutOfRangeException(nameof(expandColumnIndex));

        void Apply()
        {
            if (IsDisposed || list.IsDisposed || !list.IsHandleCreated || list.ClientSize.Width <= 0)
                return;

            var minimums = minimumWidthsAt96Dpi
                .Select(width => DpiLayout.Scale(list, width))
                .ToArray();
            // Leave a small native-layout inset for ListView borders and themed
            // header rounding; otherwise a mathematically exact fit can still
            // create a 1-2 px horizontal scrollbar at 300% DPI.
            var available = Math.Max(1, list.ClientSize.Width - DpiLayout.Scale(list, 8));
            var minimumTotal = minimums.Sum();
            var weightTotal = weights.Sum();
            if (weightTotal <= 0) return;

            var widths = new int[weights.Length];
            if (available < minimumTotal)
            {
                // 真正小於所有可讀最小欄寬時才保留水平捲動。
                Array.Copy(minimums, widths, minimums.Length);
            }
            else
            {
                // 先配置最小寬度，再分配剩餘空間；避免逐欄 Math.Max
                // 導致多個欄位同時被墊高，總寬度超出面板數個像素。
                var remaining = available - minimumTotal;
                var distributed = 0;
                for (var i = 0; i < widths.Length; i++)
                {
                    var extra = (int)Math.Floor(remaining * weights[i] / weightTotal);
                    widths[i] = minimums[i] + extra;
                    distributed += extra;
                }

                // 將整數除法產生的餘數交給文字較多的欄位。
                widths[expandColumnIndex] += remaining - distributed;
            }

            for (var i = 0; i < widths.Length; i++)
                list.Columns[i].Width = widths[i];
        }

        void QueueApply()
        {
            if (IsDisposed || list.IsDisposed || !list.IsHandleCreated) return;
            try
            {
                // Always queue this pass. On Shown / DpiChanged the parent and
                // native ListView may still be completing their layout transaction.
                list.BeginInvoke(new Action(Apply));
            }
            catch (InvalidOperationException) { /* 表單正在關閉時略過版面更新。 */ }
        }

        list.Resize += (_, _) => Apply();
        list.HandleCreated += (_, _) => QueueApply();
        Shown += (_, _) => QueueApply();
        DpiChanged += (_, _) => QueueApply();
        QueueApply();
    }

    private Panel CreateSearchFilterGroup(string caption, Control input)
    {
        // Use a fixed, non-wrapping panel for each caption/input pair. A Panel is
        // deliberate here: unlike a nested FlowLayoutPanel it cannot reflow or
        // compress the caption into one character per line at 300% DPI.
        var group = new Panel
        {
            AutoSize = false,
            Margin = new Padding(4, 2, 6, 2),
            Padding = Padding.Empty,
            TabStop = false
        };
        var label = new Label
        {
            Text = caption,
            AutoSize = true,
            Font = Font,
            Margin = Padding.Empty,
            Padding = new Padding(2, 0, 4, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = false
        };
        input.Margin = Padding.Empty;
        group.Controls.Add(label);
        group.Controls.Add(input);

        void LayoutGroup()
        {
            if (group.IsDisposed || label.IsDisposed || input.IsDisposed) return;
            label.PerformLayout();
            var labelSize = label.GetPreferredSize(Size.Empty);
            var gap = DpiLayout.Scale(this, 5);
            var height = Math.Max(labelSize.Height, input.Height);
            label.Location = new Point(0, Math.Max(0, (height - labelSize.Height) / 2));
            input.Location = new Point(labelSize.Width + gap, Math.Max(0, (height - input.Height) / 2));
            group.Size = new Size(labelSize.Width + gap + input.Width, height);
        }

        input.SizeChanged += (_, _) => LayoutGroup();
        input.FontChanged += (_, _) => LayoutGroup();
        label.FontChanged += (_, _) => LayoutGroup();
        DpiChanged += (_, _) =>
        {
            if (IsDisposed || group.IsDisposed) return;
            try { BeginInvoke(new Action(LayoutGroup)); }
            catch (InvalidOperationException) { }
        };
        LayoutGroup();
        return group;
    }

    private Control BuildTagsPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, WrapContents = false, Padding = new Padding(6, 5, 4, 4) };
        var add = new Button { Text = "新增標籤", AutoSize = true }; add.Click += (_, _) => AddEntryTag();
        var remove = new Button { Text = "移除選取標籤", AutoSize = true }; remove.Click += (_, _) => RemoveEntryTag();
        _tags.Columns.Add("標籤", 260);
        actions.Controls.AddRange(new Control[] { add, remove });
        DpiLayout.EnsureTextFits(add, this, 36, 14); DpiLayout.EnsureTextFits(remove, this, 36, 14);
        DpiLayout.FitFlowPanelHeight(this, actions, 48);
        panel.Controls.Add(_tags); panel.Controls.Add(actions); return panel;
    }

    private Control BuildSourcesPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, WrapContents = false, Padding = new Padding(6, 5, 4, 4) };
        var copy = new Button { Text = "複製來源檔案路徑", AutoSize = true };
        copy.Click += (_, _) =>
        {
            if (_sources.SelectedItems.Count == 0) { _status.Text = "請先選取來源紀錄。"; return; }
            var path = _sources.SelectedItems[0].Tag?.ToString();
            if (!string.IsNullOrWhiteSpace(path)) { Clipboard.SetText(path); _status.Text = "已複製來源檔案路徑。"; }
        };
        _sources.Columns.Add("來源檔案", 340); _sources.Columns.Add("工作表", 140); _sources.Columns.Add("列", 70); _sources.Columns.Add("來源表格", 150); _sources.Columns.Add("匯入時間", 180);
        actions.Controls.Add(copy);
        DpiLayout.EnsureTextFits(copy, this, 36, 14);
        DpiLayout.FitFlowPanelHeight(this, actions, 48);
        panel.Controls.Add(_sources); panel.Controls.Add(actions); return panel;
    }

    private Control BuildRelatedPanel()
    {
        _related.Columns.Add("相關條目", 220); _related.Columns.Add("類型", 100); _related.Columns.Add("共用標籤數", 90); _related.Columns.Add("共用標籤", 260); _related.Columns.Add("相同類型", 90); _related.Columns.Add("更新時間", 160);
        ConfigureAdaptiveListViewColumns(
            _related,
            new[] { 25F, 12F, 13F, 22F, 12F, 16F },
            new[] { 110, 75, 90, 120, 80, 105 },
            expandColumnIndex: 3);
        var preview = new Button { Text = "檢視相關條目摘要", AutoSize = true, Enabled = false };
        preview.Click += (_, _) => ShowRelatedPreview();
        _related.SelectedIndexChanged += (_, _) => preview.Enabled = _related.SelectedItems.Count > 0;
        _related.DoubleClick += (_, _) => ShowRelatedPreview();
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(6), WrapContents = true, AutoScroll = true
        };
        actions.Controls.Add(preview);
        DpiLayout.EnsureTextFits(preview, this, 36, 14);
        DpiLayout.FitFlowPanelHeight(this, actions, 48);
        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(_related); panel.Controls.Add(actions);
        _tips.SetToolTip(_related, "顯示共用標籤名稱與相同類型依據；雙擊或按「檢視相關條目摘要」可唯讀預覽，不會建立或修改條目關係。");
        return panel;
    }

    private void ShowRelatedPreview()
    {
        if (_related.SelectedItems.Count == 0) return;
        if (_related.SelectedItems[0].Tag is not RelatedEntryItem item) return;
        using var dialog = new RelatedEntryPreviewDialog(item);
        dialog.ShowDialog(this);
    }

    private void LoadTags(string id)
    {
        _tags.BeginUpdate(); _tags.Items.Clear();
        foreach (var tag in (System.Collections.IEnumerable)new EntryTagService().List(_db!, id))
        { var value = tag?.ToString() ?? ""; if (value.Length == 0) continue; var row = new ListViewItem(value); row.Tag = value; _tags.Items.Add(row); }
        _tags.EndUpdate();
    }

    private void LoadSources(string id)
    {
        _sources.BeginUpdate(); _sources.Items.Clear();
        foreach (var source in (System.Collections.IEnumerable)new EntryContentService().ListSources(_db!, id))
        {
            var t = source!.GetType();
            string Get(string name) => t.GetProperty(name)?.GetValue(source)?.ToString() ?? "";
            var path = Get("SourceFile"); var row = new ListViewItem(path);
            row.SubItems.Add(Get("Sheet")); row.SubItems.Add(Get("Row")); row.SubItems.Add(Get("Table")); row.SubItems.Add(Get("ImportedAt")); row.Tag = path; _sources.Items.Add(row);
        }
        _sources.EndUpdate();
    }

    private void LoadRelated(string id)
    {
        _related.BeginUpdate(); _related.Items.Clear();
        foreach (var item in new RelatedEntryService().Get(_db!, id))
        {
            var row = new ListViewItem(item.Title); row.SubItems.Add(item.EntryType); row.SubItems.Add(item.SharedTagCount.ToString());
            row.SubItems.Add(item.SharedTags); row.SubItems.Add(item.SameType ? "是" : "否"); row.SubItems.Add(item.UpdatedAt); row.Tag = item; _related.Items.Add(row);
        }
        _related.EndUpdate();
    }

    private void AddEntryTag()
    {
        if (!RequireSelection()) return;
        var tag = Prompt("新增標籤", "標籤名稱："); if (tag is null) return;
        try { new EntryTagService().Add(_db!, _selectedEntryId!, tag); LoadTags(_selectedEntryId!); SearchEntries(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void RemoveEntryTag()
    {
        if (!RequireSelection() || _tags.SelectedItems.Count == 0) return;
        var tag = _tags.SelectedItems[0].Tag?.ToString(); if (string.IsNullOrWhiteSpace(tag)) return;
        if (MessageBox.Show(this, $"確定移除此條目的標籤「{tag}」？不會刪除條目或其他資料。", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try { new EntryTagService().Remove(_db!, _selectedEntryId!, tag); LoadTags(_selectedEntryId!); SearchEntries(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private Control BuildNotesPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, WrapContents = false, Padding = new Padding(6, 5, 4, 4) };
        var add = new Button { Text = "新增筆記", AutoSize = true, Height = 36 }; add.Click += (_, _) => AddNote();
        var edit = new Button { Text = "編輯選取筆記", AutoSize = true, Height = 36 }; edit.Click += (_, _) => EditNote();
        var delete = new Button { Text = "刪除選取筆記", AutoSize = true, Height = 36 }; delete.Click += (_, _) => DeleteNote();
        actions.Controls.AddRange(new Control[] { add, edit, delete });
        foreach (var button in new[] { add, edit, delete }) DpiLayout.EnsureTextFits(button, this, 36, 14);
        DpiLayout.FitFlowPanelHeight(this, actions, 50);
        _notes.Dock = DockStyle.Fill; _notes.Font = new Font(Font.FontFamily, 10F);
        panel.Controls.Add(_notes); panel.Controls.Add(actions); return panel;
    }

    private Control BuildImagesPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, WrapContents = false, AutoScroll = true, Padding = new Padding(6, 5, 4, 4) };
        var add = new Button { Text = "新增圖片關聯", AutoSize = true, Height = 36 }; add.Click += (_, _) => AddImage();
        var editCaption = new Button { Text = "編輯圖片說明", AutoSize = true, Height = 36 }; editCaption.Click += (_, _) => EditImageCaption();
        var verifyImage = new Button { Text = "檢查圖片檔案", AutoSize = true, Height = 36 }; verifyImage.Click += (_, _) => VerifySelectedImage();
        var remove = new Button { Text = "移除選取圖片關聯", AutoSize = true, Height = 36 }; remove.Click += (_, _) => RemoveImage();
        foreach (var button in new[] { add, editCaption, verifyImage, remove }) DpiLayout.EnsureTextFits(button, this, 36, 14);
        DpiLayout.EnsureTextFits(_imageKeyword, this, 36, 14);
        DpiLayout.EnsureTextFits(_imageSort, this, 36, 14);
        _imageSort.Items.AddRange(new object[] { "原始順序", "檔名 A 到 Z", "檔名 Z 到 A", "最新加入優先", "最早加入優先" });
        _imageSort.SelectedIndex = 0;
        _imageKeyword.TextChanged += (_, _) => RenderImages();
        _imageSort.SelectedIndexChanged += (_, _) => RenderImages();
        actions.Controls.AddRange(new Control[] { add, editCaption, verifyImage, remove, new Label { Text = "篩選：", AutoSize = true, Padding = new Padding(5, 8, 0, 0) }, _imageKeyword, new Label { Text = "排序：", AutoSize = true, Padding = new Padding(5, 8, 0, 0) }, _imageSort, _imageCount });
        _images.Dock = DockStyle.Fill;
        DpiLayout.FitFlowPanelHeight(this, actions, 50);
        panel.Controls.Add(_images); panel.Controls.Add(actions); return panel;
    }

    private void RunDatabaseHealthCheck()
    {
        if (string.IsNullOrWhiteSpace(_db) || !File.Exists(_db))
        {
            MessageBox.Show(this, "請先開啟知識庫資料庫。", "資料庫健康檢查", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var result = new DatabaseHealthService().Check(_db);
            var details = string.Join(Environment.NewLine, result.QuickCheck);
            var message = $"資料庫：{result.Database}{Environment.NewLine}" +
                $"Schema 版本：{result.SchemaVersion}{Environment.NewLine}" +
                $"SQLite quick_check：{(result.QuickCheckPassed ? "通過" : "未通過")}{Environment.NewLine}" +
                $"外鍵問題：{result.ForeignKeyIssueCount}{Environment.NewLine}" +
                $"檢查模式：唯讀{Environment.NewLine}{Environment.NewLine}" +
                $"結果：{(result.IsHealthy ? "健康" : "發現問題，請先備份並停止寫入")}{Environment.NewLine}{details}";
            MessageBox.Show(this, message, "資料庫健康檢查", MessageBoxButtons.OK,
                result.IsHealthy ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "資料庫健康檢查失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CreateDatabaseBackup()
    {
        if (string.IsNullOrWhiteSpace(_db) || !File.Exists(_db))
        {
            MessageBox.Show(this, "請先開啟知識庫資料庫。", "資料庫備份", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var suggestedName = $"PersonalWiki_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
        using var dialog = new SaveFileDialog
        {
            Title = "建立新的資料庫備份",
            Filter = "SQLite 資料庫 (*.db)|*.db",
            FileName = suggestedName,
            AddExtension = true,
            DefaultExt = "db",
            OverwritePrompt = false,
            CheckPathExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var result = new DatabaseBackupService().Backup(_db, dialog.FileName);
            var json = System.Text.Json.JsonSerializer.Serialize(result,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            MessageBox.Show(this, $"備份已建立並通過完整性檢查：{Environment.NewLine}{dialog.FileName}{Environment.NewLine}{Environment.NewLine}{json}",
                "資料庫備份完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"備份未完成。原資料庫未被修改。{Environment.NewLine}{ex.Message}",
                "資料庫備份失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenDatabase()
    {
        using var dialog = new OpenFileDialog { Title = "選擇 PersonalWiki 資料庫", Filter = "SQLite 資料庫 (*.db;*.sqlite)|*.db;*.sqlite|所有檔案 (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { _ = new WikiDatabaseService().Inspect(dialog.FileName); _db = dialog.FileName; _database.Text = _db; _settings.SaveLastDatabase(_db); _status.Text = "資料庫已開啟。"; SearchEntries(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void OpenLastDatabase()
    {
        var path = _settings.Load();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            MessageBox.Show(this, "找不到上次使用的資料庫，請使用「開啟資料庫」重新選擇。", "PersonalWiki", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        try
        {
            _ = new WikiDatabaseService().Inspect(path);
            _db = path; _database.Text = path; _settings.SaveLastDatabase(path);
            _status.Text = "已重新開啟上次使用的資料庫。"; SearchEntries();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void CreateDatabase()
    {
        using var dialog = new SaveFileDialog { Title = "建立新的知識庫", Filter = "SQLite 資料庫 (*.db)|*.db", FileName = "PersonalWiki.db", OverwritePrompt = false };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (File.Exists(dialog.FileName) && MessageBox.Show(this, "檔案已存在。初始化可能影響既有資料，確定繼續？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { new WikiDatabaseService().Initialize(dialog.FileName); _db = dialog.FileName; _database.Text = _db; _settings.SaveLastDatabase(_db); _status.Text = "知識庫已建立／初始化。"; SearchEntries(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void ShowImportBatchCompare()
    {
        if (string.IsNullOrWhiteSpace(_db) || !File.Exists(_db))
        {
            MessageBox.Show(this, "請先開啟知識庫。", "Excel 批次差異比較", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new ExcelImportBatchCompareDialog(_db);
        dialog.ShowDialog(this);
    }

    private void ShowImportBatchHistory()
    {
        if (string.IsNullOrWhiteSpace(_db) || !File.Exists(_db))
        {
            MessageBox.Show(this, "請先開啟知識庫。", "Excel 匯入歷史", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new ExcelImportBatchHistoryDialog(_db);
        dialog.ShowDialog(this);
    }

    private void ShowTagManagement()
    {
        if (!RequireDatabase()) return;
        try
        {
            using var dialog = new TagManagementDialog(_db!);
            dialog.ShowDialog(this);
            SearchEntries();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void ShowDuplicateReport()
    {
        if (!RequireDatabase()) return;
        try
        {
            var groups = new DuplicateEntryReportService().Find(_db!);
            using var dialog = new DuplicateEntryReportDialog(groups);
            dialog.ShowDialog(this);
            _status.Text = groups.Count == 0 ? "重複條目檢查完成：沒有發現名稱重複。" : $"重複條目檢查完成：發現 {groups.Count} 組名稱重複。此檢查未修改資料。";
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void RunImageIntegrityAudit()
    {
        if (!RequireDatabase()) return;
        using var dialog = new SaveFileDialog
        {
            Title = "儲存圖片完整性檢查報告（唯讀）",
            Filter = "JSON 報告|*.json",
            FileName = $"PersonalWiki_ImageIntegrity_{DateTime.Now:yyyyMMdd_HHmmss}.json",
            OverwritePrompt = false
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var report = new DatabaseImageFileIntegrityAuditService().Create(_db!, dialog.FileName);
            _status.Text = $"圖片檢查完成：共 {report.TotalReferences} 筆；存在 {report.ExistingFiles}、遺失 {report.MissingFiles}、無效路徑 {report.InvalidPaths}、雜湊不符 {report.HashMismatches}。資料庫及原圖未修改。";
            MessageBox.Show(this, $"圖片完整性檢查完成。\n\n圖片關聯：{report.TotalReferences}\n檔案存在：{report.ExistingFiles}\n檔案遺失：{report.MissingFiles}\n無效路徑：{report.InvalidPaths}\nSHA-256 不符：{report.HashMismatches}\n無法讀取雜湊：{report.HashUnavailable}\n\n報告已儲存至：\n{report.ReportPath}\n\n此功能只讀取資料庫及圖片檔，不會修改或刪除任何內容。", "圖片完整性報告", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void RunImageDuplicateAudit()
    {
        if (!RequireDatabase()) return;
        using var dialog = new SaveFileDialog
        {
            Title = "儲存重複圖片檢查報告（唯讀）",
            Filter = "JSON 報告|*.json",
            FileName = $"PersonalWiki_ImageDuplicates_{DateTime.Now:yyyyMMdd_HHmmss}.json",
            OverwritePrompt = false
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var report = new DatabaseImageDuplicateReportService().Create(_db!, dialog.FileName);
            _status.Text = $"重複圖片檢查完成：{report.DuplicateHashGroupCount} 組、{report.DuplicateReferenceCount} 筆關聯。資料未修改。";
            MessageBox.Show(this, $"重複圖片檢查完成。\n\n相同 SHA-256 群組：{report.DuplicateHashGroupCount}\n涉及圖片關聯：{report.DuplicateReferenceCount}\n\n報告已儲存至：\n{report.ReportPath}\n\n注意：只依資料庫已記錄的 SHA-256 比對；沒有雜湊值的圖片不會納入。此功能不會自動合併關聯或刪除原圖。", "重複圖片報告", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void ConfigureEntryButtons()
    {
        var current = _settings.LoadEntryButtonSize();
        using var dialog = new EntryButtonSettingsDialog(current.Width, current.Height,
            "輸入視窗按鈕大小設定", "新增／編輯條目視窗內的「確定儲存／取消」按鈕");
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _settings.SaveEntryButtonSize(dialog.ButtonWidth, dialog.ButtonHeight);
            _status.Text = $"已儲存輸入視窗按鈕大小：寬 {dialog.ButtonWidth}、高 {dialog.ButtonHeight}（96 DPI 基準值）。";
        }
    }

    private void ConfigureMainActionButtons()
    {
        var current = _settings.LoadMainActionButtonSize();
        using var dialog = new EntryButtonSettingsDialog(current.Width, current.Height,
            "主畫面功能按鈕大小設定", "主畫面工具列的新增條目、編輯條目等功能按鈕",
            minimumWidth: 40, minimumHeight: 20, widthIncrement: 5, heightIncrement: 2);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _settings.SaveMainActionButtonSize(dialog.ButtonWidth, dialog.ButtonHeight);
            ApplyMainActionButtonSize();
            _status.Text = $"已儲存主畫面功能按鈕大小：寬 {dialog.ButtonWidth}、高 {dialog.ButtonHeight}（96 DPI 基準值）。";
        }
    }

    private void ApplyMainActionButtonSize()
    {
        if (_mainActionBar is null || _mainTopLayout is null || _mainActionButtons.Count == 0 || _isApplyingMainActionLayout) return;
        _isApplyingMainActionLayout = true;
        try
        {
            var configured = _settings.LoadMainActionButtonSize();
            var scale = Math.Max(0.5F, DeviceDpi / 96F);
            var minimumWidth = Math.Max(1, (int)Math.Ceiling(configured.Width * scale));
            var height = Math.Max(1, (int)Math.Ceiling(configured.Height * scale));
            foreach (var button in _mainActionButtons)
            {
                button.AutoSize = true;
                button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                button.MinimumSize = new Size(minimumWidth, height);
                button.MaximumSize = Size.Empty;
                button.Height = height;
                button.PerformLayout();
            }
        }
        finally
        {
            _isApplyingMainActionLayout = false;
        }

        LayoutMainChrome();
    }

    private void QueueMainChromeLayout()
    {
        if (IsDisposed || _isApplyingMainActionLayout) return;
        if (!IsHandleCreated)
        {
            LayoutMainChrome();
            return;
        }

        try { BeginInvoke(new Action(LayoutMainChrome)); }
        catch (InvalidOperationException) { /* 表單正在關閉時略過。 */ }
    }

    /// <summary>
    /// Positions the four main toolbar rows using actual control sizes in device
    /// pixels. This avoids mixing DPI-scaled TableLayoutPanel row styles with
    /// manually scaled pixel heights, which previously clipped the query buttons.
    /// </summary>
    private void LayoutMainChrome()
    {
        if (_mainTopLayout is null || _mainActionBar is null || _mainSearchRowOne is null ||
            _mainSearchRowTwo is null || _mainQueryBar is null || _isApplyingMainActionLayout || IsDisposed)
            return;

        _isApplyingMainActionLayout = true;
        try
        {
            var width = Math.Max(1, ClientSize.Width);
            var y = DpiLayout.Scale(this, 3);
            var gap = DpiLayout.Scale(this, 2);

            var actionHeight = ArrangeActionButtonRows(_mainActionBar, width);
            _mainActionBar.SetBounds(0, y, width, actionHeight);
            y += actionHeight + gap;

            var firstHeight = ArrangeHorizontalRow(_mainSearchRowOne, width);
            _mainSearchRowOne.SetBounds(0, y, width, firstHeight);
            y += firstHeight + gap;

            var secondHeight = ArrangeHorizontalRow(_mainSearchRowTwo, width);
            _mainSearchRowTwo.SetBounds(0, y, width, secondHeight);
            y += secondHeight + gap;

            var queryHeight = ArrangeHorizontalRow(_mainQueryBar, width);
            _mainQueryBar.SetBounds(0, y, width, queryHeight);
            y += queryHeight + DpiLayout.Scale(this, 3);

            _mainTopLayout.SetBounds(0, 0, width, y);
            _mainTopLayout.PerformLayout();
            PerformLayout();
        }
        finally
        {
            _isApplyingMainActionLayout = false;
        }
    }

    private int ArrangeActionButtonRows(Panel row, int width)
    {
        // 主功能按鈕數量較多，在 300% DPI 或窄視窗下以完整按鈕換行，
        // 不再讓最後幾個功能被裁切，也不顯示水平捲軸。
        row.Width = width;
        row.AutoScroll = false;
        row.AutoScrollMinSize = Size.Empty;

        var controls = row.Controls.Cast<Control>()
            .Where(control => control.Visible)
            .ToArray();
        if (controls.Length == 0)
            return DpiLayout.Scale(this, 42);

        var rightLimit = Math.Max(row.Padding.Left + 1, width - row.Padding.Right);
        var rowGap = DpiLayout.Scale(this, 2);
        var x = row.Padding.Left;
        var y = row.Padding.Top;
        var rowHeight = 0;
        var currentRow = new List<(Control Control, int X)>();

        void CommitRow()
        {
            foreach (var item in currentRow)
            {
                var top = y + item.Control.Margin.Top +
                    Math.Max(0, (rowHeight - item.Control.Height - item.Control.Margin.Vertical) / 2);
                item.Control.Location = new Point(item.X, top);
            }
        }

        foreach (var control in controls)
        {
            control.PerformLayout();
            var outerWidth = control.Width + control.Margin.Horizontal;
            if (currentRow.Count > 0 && x + outerWidth > rightLimit)
            {
                CommitRow();
                y += rowHeight + rowGap;
                x = row.Padding.Left;
                rowHeight = 0;
                currentRow.Clear();
            }

            var controlX = x + control.Margin.Left;
            currentRow.Add((control, controlX));
            x += outerWidth;
            rowHeight = Math.Max(rowHeight, control.Height + control.Margin.Vertical);
        }

        CommitRow();
        var totalHeight = y + rowHeight + row.Padding.Bottom;
        return Math.Max(DpiLayout.Scale(this, 42), totalHeight + DpiLayout.Scale(this, 2));
    }

    private int ArrangeHorizontalRow(Panel row, int width)
    {
        row.Width = width;
        row.AutoScrollPosition = Point.Empty;
        var controls = row.Controls.Cast<Control>().Where(control => control.Visible).ToArray();
        foreach (var control in controls)
            control.PerformLayout();

        // Measure actual current bounds only. GetPreferredSize on a non-AutoSize
        // Panel can return a generic default size and reintroduce false blank rows.
        var contentHeight = controls
            .Select(control => Math.Max(1, control.Height) + control.Margin.Vertical)
            .DefaultIfEmpty(0)
            .Max();

        var x = row.Padding.Left;
        foreach (var control in controls)
        {
            x += control.Margin.Left;
            var top = row.Padding.Top + control.Margin.Top + Math.Max(0, (contentHeight - control.Height - control.Margin.Vertical) / 2);
            control.Location = new Point(x, top);
            x += control.Width + control.Margin.Right;
        }

        var contentWidth = x + row.Padding.Right;
        row.AutoScrollMinSize = new Size(contentWidth, 0);
        var needsHorizontalScroll = contentWidth > width;
        var scrollHeight = needsHorizontalScroll ? SystemInformation.HorizontalScrollBarHeight : 0;
        return Math.Max(DpiLayout.Scale(this, 42), contentHeight + row.Padding.Vertical + scrollHeight + DpiLayout.Scale(this, 2));
    }

    private void CreateEntry()
    {
        if (!RequireDatabase()) return;
        using var form = new EntryEditDialog("新增條目", "", "General", "", _settings.LoadEntryButtonSize());
        if (form.ShowDialog(this) != DialogResult.OK) return;
        try { var item = new EntryService().Create(_db!, form.EntryTitle, form.EntryType, form.EntrySummary); SearchEntries(); SelectEntry(item.Id); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void EditEntry()
    {
        if (!RequireSelection()) return;
        try
        {
            var selectedId = _selectedEntryId!;
            var detail = new EntryDetailService().Get(_db!, selectedId); var t = detail.GetType();
            string Get(string name) => t.GetProperty(name)?.GetValue(detail)?.ToString() ?? "";
            using var form = new EntryEditDialog("編輯條目", Get("Title"), Get("EntryType"), Get("Summary"), _settings.LoadEntryButtonSize());
            if (form.ShowDialog(this) != DialogResult.OK) return;
            new EntryDetailService().Update(_db!, selectedId, form.EntryTitle, form.EntryType, form.EntrySummary); SearchEntries(); SelectEntry(selectedId);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void ToggleArchive()
    {
        if (!RequireSelection()) return;
        try
        {
            var selectedId = _selectedEntryId!;
            var detail = new EntryDetailService().Get(_db!, selectedId); var archived = (bool)(detail.GetType().GetProperty("IsArchived")?.GetValue(detail) ?? false);
            var action = archived ? "還原" : "封存";
            if (MessageBox.Show(this, $"確定{action}這筆條目？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            new EntryDetailService().SetArchived(_db!, selectedId, !archived);
            // 操作後切到全部條目，讓使用者立即看見狀態變化，不會因篩選而誤以為資料消失。
            _archiveFilter.SelectedIndex = 1;
            SearchEntries(); SelectEntry(selectedId);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void BatchToggleArchive()
    {
        if (!RequireDatabase()) return;
        var ids = _results.SelectedItems.Cast<ListViewItem>()
            .Select(x => x.Tag?.ToString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .Cast<string>()
            .ToList();
        if (ids.Count == 0)
        {
            MessageBox.Show(this, "請先在左側清單選取一筆或多筆條目。可按住 Ctrl 多選，或按住 Shift 選取連續項目。", "批次封存／還原", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var detailService = new EntryDetailService();
            var states = ids.Select(id =>
            {
                var detail = detailService.Get(_db!, id);
                var archived = (bool)(detail.GetType().GetProperty("IsArchived")?.GetValue(detail) ?? false);
                return (Id: id, Archived: archived);
            }).ToList();
            var archiveCount = states.Count(x => !x.Archived);
            var restoreCount = states.Count(x => x.Archived);
            var message = $"即將處理 {states.Count} 筆選取條目：\n封存：{archiveCount} 筆\n還原：{restoreCount} 筆\n\n只會變更封存狀態，不會刪除條目、筆記或圖片。是否繼續？";
            if (MessageBox.Show(this, message, "確認批次操作", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            // 每筆使用既有安全服務更新；不執行刪除，也不觸碰關聯資料。
            foreach (var item in states)
                detailService.SetArchived(_db!, item.Id, !item.Archived);

            _archiveFilter.SelectedIndex = 1;
            SearchEntries();
            _status.Text = $"批次操作完成：封存 {archiveCount} 筆、還原 {restoreCount} 筆。資料內容及圖片未刪除。";
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void CopySelectedResultSummaries()
    {
        var selected = _results.SelectedItems.Cast<ListViewItem>().ToList();
        if (selected.Count == 0)
        {
            _status.Text = "請先選取一筆或多筆搜尋結果。";
            return;
        }

        var lines = selected.Select(row => string.Join(Environment.NewLine, new[]
        {
            $"名稱：{row.Text}",
            $"類型：{row.SubItems[1].Text}",
            $"摘要：{row.SubItems[2].Text}",
            $"更新時間：{row.SubItems[3].Text}",
            $"狀態：{row.SubItems[4].Text}",
            $"ID：{row.Tag?.ToString() ?? string.Empty}"
        }));
        try
        {
            Clipboard.SetText(string.Join(Environment.NewLine + Environment.NewLine + "--------------------" + Environment.NewLine + Environment.NewLine, lines));
            _status.Text = $"已複製 {selected.Count} 筆條目摘要；資料庫未變更。";
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void SearchEntries()
    {
        _currentPage = 1; SearchEntriesPage();
    }

    private void ChangePage(int delta)
    {
        var target = _currentPage + delta;
        if (target < 1 || target > _totalPages) return;
        _currentPage = target; SearchEntriesPage();
    }

    private void SearchEntriesPage()
    {
        if (_db is null) { _status.Text = "請先開啟或建立知識庫。"; return; }
        try
        {
            var type = string.IsNullOrWhiteSpace(_typeFilter.Text) || _typeFilter.Text == "全部類型" ? null : _typeFilter.Text.Trim();
            var archive = _archiveFilter.SelectedIndex switch { 1 => "include", 2 => "only", _ => "exclude" };
            var sort = _sortFilter.SelectedIndex switch { 1 => "title-asc", 2 => "title-desc", 3 => "created-desc", 4 => "created-asc", _ => "updated-desc" };
            string? DateValue(DateFilterControl picker) => picker.Checked ? picker.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : null;
            var result = new EntryQueryService().Query(_db, _keyword.Text, type, _tagFilter.Text, archive,
                _currentPage, PageSize, sort, DateValue(_createdFrom), DateValue(_createdTo), DateValue(_updatedFrom), DateValue(_updatedTo));
            _totalPages = Math.Max(1, result.TotalPages);
            if (_currentPage > _totalPages) { _currentPage = _totalPages; SearchEntriesPage(); return; }
            var previouslySelectedId = _selectedEntryId;
            _results.BeginUpdate(); _results.Items.Clear();
            foreach (var item in result.Items)
            { var row = new ListViewItem(item.Title); row.SubItems.Add(item.EntryType); row.SubItems.Add(item.Summary ?? ""); row.SubItems.Add(item.UpdatedAt); row.SubItems.Add(item.IsArchived ? "已封存" : "使用中"); row.Tag = item.Id; _results.Items.Add(row); }
            _results.EndUpdate(); _pageInfo.Text = $"第 {_currentPage} / {_totalPages} 頁";
            _status.Text = result.TotalCount == 0
                ? "找不到符合的條目。可試試名稱的一部分，或確認封存狀態篩選。"
                : $"共 {result.TotalCount} 筆，本頁 {result.Items.Count} 筆。";
            if (_results.Items.Count > 0 && previouslySelectedId is not null) SelectEntry(previouslySelectedId);
            else if (_results.Items.Count == 0) { _selectedEntryId = null; _archiveAction.Enabled = false; }
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void LoadEntry(string? id)
    {
        if (_db is null || string.IsNullOrWhiteSpace(id)) return; _selectedEntryId = id;
        try
        {
            var detail = new EntryDetailService().Get(_db, id); _summary.Text = FormatObject(detail);
            var isArchived = (bool)(detail.GetType().GetProperty("IsArchived")?.GetValue(detail) ?? false);
            _archiveAction.Text = isArchived ? "還原選取條目" : "封存選取條目";
            _archiveAction.Enabled = true;
            // 同一工作表的所有匯入條目共用穩定欄位版面；欄位順序依來源欄位排序。
            var detailService = new EntryDetailService();
            var importedFields = ((System.Collections.IEnumerable)detailService.ListFields(_db, id))
                .Cast<object>().Select(field =>
                {
                    var fieldType = field.GetType();
                    string Get(string n) => fieldType.GetProperty(n)?.GetValue(field)?.ToString() ?? "";
                    return (Key: Get("Key"), Label: Get("Label"), Value: Get("Value"),
                        ValueType: Get("ValueType"), SourceSheet: Get("SourceSheet"));
                }).ToList();
            _fields.BeginUpdate();
            _fields.Items.Clear();
            _fields.Columns.Clear();
            var sourceSheet = importedFields.Select(x => x.SourceSheet).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            if (!string.IsNullOrWhiteSpace(sourceSheet))
            {
                var sheetLayout = ((System.Collections.IEnumerable)detailService.ListSheetLayout(_db, sourceSheet))
                    .Cast<object>().Select(field =>
                    {
                        var fieldType = field.GetType();
                        string Get(string n) => fieldType.GetProperty(n)?.GetValue(field)?.ToString() ?? "";
                        return (Key: Get("Key"), Label: Get("Label"));
                    }).ToList();
                var values = importedFields.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
                _fields.Columns.Add("工作表欄位", 180);
                foreach (var field in sheetLayout)
                    _fields.Columns.Add(string.IsNullOrWhiteSpace(field.Label) ? "未命名欄位" : field.Label, 180);
                var row = new ListViewItem("欄位值");
                foreach (var field in sheetLayout)
                    row.SubItems.Add(values.TryGetValue(field.Key, out var value) ? value ?? "" : "");
                _fields.Items.Add(row);
                _tips.SetToolTip(_fields, $"此條目來自工作表「{sourceSheet}」。欄位版面由同一工作表的匯入欄位統一決定；沒有值的欄位會留白。");
            }
            else if (importedFields.Count > 0)
            {
                _fields.Columns.Add("工作表欄位", 180);
                foreach (var field in importedFields)
                    _fields.Columns.Add(string.IsNullOrWhiteSpace(field.Label) ? "未命名欄位" : field.Label, 180);
                var row = new ListViewItem("欄位值");
                foreach (var field in importedFields) row.SubItems.Add(field.Value);
                _fields.Items.Add(row);
                _tips.SetToolTip(_fields, "此條目沒有來源工作表資訊，依條目現有欄位顯示。");
            }
            else
            {
                _fields.Columns.Add("欄位", 180); _fields.Columns.Add("內容", 520); _fields.Columns.Add("類型", 100);
            }
            _fields.EndUpdate();
            LoadNotes(id); LoadImages(id); LoadTags(id); LoadSources(id); LoadRelated(id);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void LoadNotes(string id)
    {
        _notes.Items.Clear();
        foreach (var note in (System.Collections.IEnumerable)new EntryContentService().ListNotes(_db!, id))
        { var t = note!.GetType(); var row = new ListViewItem(t.GetProperty("Text")?.GetValue(note)?.ToString() ?? ""); row.SubItems.Add(t.GetProperty("CreatedAt")?.GetValue(note)?.ToString() ?? ""); row.SubItems.Add(t.GetProperty("UpdatedAt")?.GetValue(note)?.ToString() ?? ""); row.Tag = t.GetProperty("Id")?.GetValue(note)?.ToString(); _notes.Items.Add(row); }
    }

    private void LoadImages(string id)
    {
        _selectedImageId = null;
        foreach (Control c in _images.Controls)
            if (c is Panel oldPanel)
                foreach (var oldPicture in oldPanel.Controls.OfType<PictureBox>())
                {
                    var old = oldPicture.Image; oldPicture.Image = null; old?.Dispose();
                }
        _images.Controls.Clear();
        _currentImages = ((System.Collections.IEnumerable)new EntryContentService().ListImages(_db!, id))
            .Cast<object>().ToList();
        RenderImages();
    }

    private void RenderImages()
    {
        foreach (Control c in _images.Controls)
            if (c is Panel oldPanel)
                foreach (var oldPicture in oldPanel.Controls.OfType<PictureBox>())
                {
                    var old = oldPicture.Image; oldPicture.Image = null; old?.Dispose();
                }
        _images.Controls.Clear();
        var query = _imageKeyword.Text.Trim();
        IEnumerable<object> items = _currentImages;
        if (query.Length > 0)
            items = items.Where(img =>
            {
                var t = img.GetType();
                var path = t.GetProperty("Path")?.GetValue(img)?.ToString() ?? "";
                var caption = t.GetProperty("Caption")?.GetValue(img)?.ToString() ?? "";
                return Path.GetFileName(path).Contains(query, StringComparison.OrdinalIgnoreCase)
                    || caption.Contains(query, StringComparison.OrdinalIgnoreCase);
            });
        items = _imageSort.SelectedIndex switch
        {
            1 => items.OrderBy(img => Path.GetFileName(img.GetType().GetProperty("Path")?.GetValue(img)?.ToString() ?? ""), StringComparer.OrdinalIgnoreCase),
            2 => items.OrderByDescending(img => Path.GetFileName(img.GetType().GetProperty("Path")?.GetValue(img)?.ToString() ?? ""), StringComparer.OrdinalIgnoreCase),
            3 => items.OrderByDescending(img => img.GetType().GetProperty("CreatedAt")?.GetValue(img)?.ToString()),
            4 => items.OrderBy(img => img.GetType().GetProperty("CreatedAt")?.GetValue(img)?.ToString()),
            _ => items
        };
        var visible = items.ToList();
        _imageCount.Text = $"顯示 {visible.Count} / {_currentImages.Count} 張";
        foreach (var img in visible)
        {
            var t = img.GetType();
            var path = t.GetProperty("Path")?.GetValue(img)?.ToString() ?? "";
            var imageId = t.GetProperty("Id")?.GetValue(img)?.ToString();
            if (string.IsNullOrWhiteSpace(imageId)) continue;
            var exists = File.Exists(path);
            var picture = new PictureBox
            {
                Width = 170, Height = 145, SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle, BackColor = SystemColors.ControlLight, Tag = imageId
            };
            var canPreview = false;
            if (exists)
            {
                try { picture.Image = _thumbnailCache.Get(path); canPreview = picture.Image is not null; }
                catch { /* 保留關聯卡片；格式不支援預覽時不可讓使用者以為圖片未加入。 */ }
            }
            var captionText = t.GetProperty("Caption")?.GetValue(img)?.ToString();
            var description = !exists ? $"原圖不存在：{Path.GetFileName(path)}"
                : canPreview ? (string.IsNullOrWhiteSpace(captionText) ? Path.GetFileName(path) : captionText)
                : $"已建立關聯，但目前無法預覽：{Path.GetFileName(path)}";
            var caption = new Label { Text = description, Dock = DockStyle.Bottom, Height = 42, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleCenter };
            var host = new Panel { Width = 184, Height = 196, Padding = new Padding(4), BorderStyle = BorderStyle.FixedSingle, Tag = imageId };
            host.Controls.Add(picture); host.Controls.Add(caption);
            void SelectThisImage()
            {
                _selectedImageId = imageId;
                foreach (var card in _images.Controls.OfType<Panel>())
                {
                    card.BackColor = card.Tag?.ToString() == imageId ? SystemColors.Highlight : SystemColors.Control;
                    foreach (var candidate in card.Controls.OfType<PictureBox>())
                        candidate.BorderStyle = card.Tag?.ToString() == imageId ? BorderStyle.Fixed3D : BorderStyle.FixedSingle;
                }
            }
            picture.Click += (_, _) => SelectThisImage();
            caption.Click += (_, _) => SelectThisImage();
            host.Click += (_, _) => SelectThisImage();
            _images.Controls.Add(host);
        }
    }

    private void AddNote()
    {
        if (!RequireSelection()) return; var text = Prompt("新增筆記", "筆記內容："); if (text is null) return;
        try { new EntryContentService().AddNote(_db!, _selectedEntryId!, text); LoadNotes(_selectedEntryId!); }
        catch (Exception ex) { ShowError(ex); }
    }
    private void EditNote()
    {
        if (!RequireSelection() || _notes.SelectedItems.Count == 0) return; var row = _notes.SelectedItems[0]; var id = row.Tag?.ToString(); if (id is null) return;
        var text = Prompt("編輯筆記", "筆記內容：", row.Text); if (text is null) return;
        try { new EntryContentService().UpdateNote(_db!, id, text); LoadNotes(_selectedEntryId!); }
        catch (Exception ex) { ShowError(ex); }
    }
    private void DeleteNote()
    {
        if (!RequireSelection() || _notes.SelectedItems.Count == 0) return; var id = _notes.SelectedItems[0].Tag?.ToString(); if (id is null) return;
        if (MessageBox.Show(this, "確定刪除此筆記？此操作無法復原。", "確認刪除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { new EntryContentService().DeleteNote(_db!, id); LoadNotes(_selectedEntryId!); } catch (Exception ex) { ShowError(ex); }
    }
    private void AddImage()
    {
        if (!RequireSelection()) return; using var dialog = new OpenFileDialog { Title = "選擇圖片（只建立資料庫關聯，不複製原圖）", Filter = "圖片|*.jpg;*.jpeg;*.png;*.gif;*.webp;*.bmp;*.svg|所有檔案|*.*" }; if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var caption = Prompt("圖片說明", "可輸入圖片說明（可留空）：", ""); if (caption is null) return;
        try
        {
            var result = new EntryContentService().AddImage(_db!, _selectedEntryId!, dialog.FileName, caption);
            var existed = (bool)(result.GetType().GetProperty("AlreadyExists")?.GetValue(result) ?? false);
            LoadImages(_selectedEntryId!);
            _status.Text = existed
                ? "這張圖片原本已關聯到此條目；沒有新增重複項目。非空白的新說明已更新，留空則保留原說明。"
                : "圖片關聯已新增；原始檔案未複製或修改。";
        }
        catch (Exception ex) { ShowError(ex); }
    }
    private void EditImageCaption()
    {
        if (!RequireSelection()) return;
        if (string.IsNullOrWhiteSpace(_selectedImageId)) { _status.Text = "請先點選要編輯說明的圖片。"; return; }
        var selected = _currentImages.FirstOrDefault(x => x.GetType().GetProperty("Id")?.GetValue(x)?.ToString() == _selectedImageId);
        var current = selected?.GetType().GetProperty("Caption")?.GetValue(selected)?.ToString() ?? "";
        var caption = Prompt("編輯圖片說明", "圖片說明（最多 2,000 字；留空代表清除說明）：", current);
        if (caption is null) return;
        try
        {
            new EntryContentService().UpdateImageCaption(_db!, _selectedImageId, caption);
            LoadImages(_selectedEntryId!);
            _status.Text = "圖片說明已更新；原始圖片檔案未修改。";
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void VerifySelectedImage()
    {
        if (!RequireSelection()) return;
        if (string.IsNullOrWhiteSpace(_selectedImageId)) { _status.Text = "請先點選要檢查的圖片。"; return; }
        try
        {
            var result = new EntryContentService().VerifyImageFile(_db!, _selectedImageId);
            var type = result.GetType();
            var exists = (bool)(type.GetProperty("FileExists")?.GetValue(result) ?? false);
            var hashMatches = type.GetProperty("HashMatches")?.GetValue(result) as bool?;
            var path = type.GetProperty("Path")?.GetValue(result)?.ToString() ?? "";
            var message = !exists ? "找不到原始圖片檔案。" : hashMatches == true ? "檔案 SHA256 與建立關聯時一致。" : "檔案存在，但 SHA256 與建立關聯時不同；原圖可能已被替換或修改。";
            MessageBox.Show(this, $"{message}{Environment.NewLine}{Environment.NewLine}{path}", "圖片檔案檢查", MessageBoxButtons.OK, exists && hashMatches == true ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            _status.Text = message;
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void RemoveImage()
    {
        if (!RequireSelection()) return;
        if (string.IsNullOrWhiteSpace(_selectedImageId)) { _status.Text = "請先點選要移除的圖片。"; return; }
        var id = _selectedImageId;
        if (MessageBox.Show(this, "只移除資料庫中的圖片關聯，不會刪除硬碟原圖。確定？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { new EntryContentService().RemoveImage(_db!, id); _selectedImageId = null; LoadImages(_selectedEntryId!); } catch (Exception ex) { ShowError(ex); }
    }

    private void ImportExcel()
    {
        if (!RequireDatabase()) return;
        var guide = $"匯入目的地：\r\n{_db}\r\n\r\n操作流程：\r\n1. 選擇來源 Excel；程式只讀取原檔，不會修改它。\r\n2. 預覽工作表與資料列，確認選到正確檔案。\r\n3. 每張要匯入的工作表，選一欄「條目名稱」；可選「主要內容」與「屬性欄位」，不要的欄位選「忽略」。整張工作表不匯入時，將該表所有欄位都設為「忽略」。\r\n4. 儲存兩份 JSON 設定檔（欄位用途與資料檢查設定），只是保存設定，不是匯入資料。\r\n5. 選擇一個新的 .db 檔案作為匯入前備份；不可使用已存在的檔案。\r\n6. 最後確認後才會備份並匯入目前開啟的資料庫。\r\n\r\n安全提醒：名稱空白、重複名稱、前後空白、控制字元或 Excel 錯誤值若被啟用的檢查規則發現，匯入會暫停，不會自動改寫資料。整個流程不會匯入圖片，也不會修改原始 Excel。\r\n\r\n按「是」開始選擇 Excel；按「否」取消。";
        if (MessageBox.Show(this, guide, "Excel 匯入精靈－操作說明", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes) return;
        using var workbook = new OpenFileDialog { Title = "第 1 步：選擇來源 Excel（唯讀，不會修改）", Filter = "Excel 活頁簿|*.xlsx;*.xlsm" };
        if (workbook.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var preview = ImportPreviewService.Analyze(workbook.FileName, 8);
            using var previewDialog = new WorkbookPreviewDialog(preview);
            if (previewDialog.ShowDialog(this) != DialogResult.OK) return;

            var profile = ImportMappingService.CreateTemplate(workbook.FileName);
            using var mappingDialog = new MappingEditorDialog(profile);
            if (mappingDialog.ShowDialog(this) != DialogResult.OK) return;

            using var mappingSave = new SaveFileDialog { Title = "儲存欄位用途設定（JSON 設定檔，不是資料庫）", Filter = "JSON 設定檔|*.json", FileName = "personalwiki-mapping.json", OverwritePrompt = false };
            if (mappingSave.ShowDialog(this) != DialogResult.OK) return;
            mappingDialog.Save(mappingSave.FileName);

            var check = ImportMappingService.Validate(workbook.FileName, mappingSave.FileName);
            var missingTitleSheets = mappingDialog.GetSheetsWithoutTitle();
            if (!mappingDialog.HasAnyImportSheet())
            {
                MessageBox.Show(this, "目前所有工作表都設定為忽略，沒有任何資料要匯入。請至少選擇一張工作表並指定「條目名稱」。", "尚未選擇匯入資料", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!check.IsValid || missingTitleSheets.Count > 0)
            {
                var errors = check.Issues.Where(x => x.Severity == "Error").Select(x => $"[{x.SheetName}] {x.Message}")
                    .Concat(missingTitleSheets.Select(name => $"[{name}] 尚未指定 Title（條目名稱）欄位。"));
                MessageBox.Show(this, "欄位對應未通過驗證：\n" + string.Join(Environment.NewLine, errors), "停止匯入", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var warnings = check.Issues.Where(x => x.Severity == "Warning").Select(x => $"[{x.SheetName}] {x.Message}").ToArray();
            if (warnings.Length > 0 && MessageBox.Show(this, "欄位對應有以下提醒：\n" + string.Join(Environment.NewLine, warnings) + "\n\n仍要繼續並建立清理規則嗎？", "欄位對應提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            using var rulesSave = new SaveFileDialog { Title = "儲存資料檢查設定（JSON 設定檔，不是資料庫）", Filter = "JSON 設定檔|*.json", FileName = "personalwiki-clean-rules.json", OverwritePrompt = false };
            if (rulesSave.ShowDialog(this) != DialogResult.OK) return;
            ImportCleaningRulesService.SaveTemplate(workbook.FileName, mappingSave.FileName, rulesSave.FileName);
            var rulesProfile = System.Text.Json.JsonSerializer.Deserialize<ImportCleaningRulesProfile>(File.ReadAllText(rulesSave.FileName))
                ?? throw new InvalidDataException("資料檢查設定檔無法讀取。");
            using (var rulesDialog = new CleaningRulesEditorDialog(rulesProfile))
            {
                if (rulesDialog.ShowDialog(this) != DialogResult.OK) return;
                rulesDialog.Save(rulesSave.FileName);
            }

            while (true)
            {
                var dryRun = ImportDryRunService.Analyze(workbook.FileName, mappingSave.FileName, rulesSave.FileName);
                if (dryRun.RowsRequiringReview == 0) break;
                using var issueDialog = new ExcelImportDryRunDialog(dryRun);
                if (issueDialog.ShowDialog(this) != DialogResult.OK) return;
                var updatedRules = System.Text.Json.JsonSerializer.Deserialize<ImportCleaningRulesProfile>(File.ReadAllText(rulesSave.FileName))
                    ?? throw new InvalidDataException("資料檢查設定檔無法讀取。");
                using var reviseRules = new CleaningRulesEditorDialog(updatedRules);
                if (reviseRules.ShowDialog(this) != DialogResult.OK) return;
                reviseRules.Save(rulesSave.FileName);
            }

            using var backup = new SaveFileDialog { Title = "第 5 步：指定匯入前備份（選擇新檔案，不可覆蓋現有檔案）", Filter = "SQLite 備份資料庫|*.db", FileName = $"wiki-before-import-{DateTime.Now:yyyyMMdd-HHmmss}.db", OverwritePrompt = false };
            if (backup.ShowDialog(this) != DialogResult.OK) return;
            if (File.Exists(backup.FileName)) { MessageBox.Show(this, "備份目的檔已存在，請選擇新檔名；不會覆蓋。", "停止匯入", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (MessageBox.Show(this, $"即將匯入至目前資料庫：\r\n{_db}\r\n\r\n匯入前會建立並驗證備份：\r\n{backup.FileName}\r\n\r\n來源 Excel 以唯讀方式處理；資料使用單一交易寫入。確定開始？", "最後確認 Excel 匯入", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            var result = new ExcelImportWorkflowService().Execute(_db!, workbook.FileName, mappingSave.FileName, rulesSave.FileName, backup.FileName);
            MessageBox.Show(this, $"匯入完成。\n匯入列數：{result.ImportedRows}\n新增自訂欄位：{result.ImportedFields}\n備份資料庫：{result.BackupDatabase}\n批次識別碼：{result.BatchId}\n\nExcel 原始檔未修改。", "匯入完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SearchEntries();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void SelectEntry(string id)
    {
        foreach (ListViewItem item in _results.Items)
            if (item.Tag?.ToString() == id) { item.Selected = true; item.Focused = true; item.EnsureVisible(); return; }
        _selectedEntryId = null; _archiveAction.Enabled = false;
    }
    private bool RequireDatabase() { if (_db is not null) return true; _status.Text = "請先開啟或建立知識庫。"; return false; }
    private bool RequireSelection() { if (RequireDatabase() && _selectedEntryId is not null && _results.SelectedItems.Count > 0) return true; _status.Text = "請先在左側條目清單點選一筆條目。"; return false; }
    private string? Prompt(string title, string label, string initial = "")
    {
        using var form = new Form { Text = title, Width = 700, Height = 430, MinimumSize = new Size(520, 340), StartPosition = FormStartPosition.CenterParent, MinimizeBox = false, MaximizeBox = true, FormBorderStyle = FormBorderStyle.Sizable, Font = new Font("Microsoft JhengHei UI", 10F) };
        DpiLayout.EnableDpiAutoScale(form);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var caption = new Label { Text = label, AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 2, 0, 8) };
        var input = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Text = initial, Font = new Font("Microsoft JhengHei UI", 11F) };
        DpiLayout.ConstrainLabelWidth(caption, layout);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.RightToLeft, WrapContents = true, Padding = new Padding(0, 8, 0, 0) };
        var ok = new Button { Text = "確定", AutoSize = true, MinimumSize = new Size(100, 40), Padding = new Padding(12, 6, 12, 6), DialogResult = DialogResult.OK }; var cancel = new Button { Text = "取消", AutoSize = true, MinimumSize = new Size(100, 40), Padding = new Padding(12, 6, 12, 6), DialogResult = DialogResult.Cancel };
        buttons.Controls.AddRange(new Control[] { cancel, ok }); layout.Controls.Add(caption, 0, 0); layout.Controls.Add(input, 0, 1); layout.Controls.Add(buttons, 0, 2);
        form.Controls.Add(layout); form.AcceptButton = ok; form.CancelButton = cancel;
        DpiLayout.KeepInsideWorkingArea(form);
        return form.ShowDialog(this) == DialogResult.OK ? input.Text : null;
    }
    private static string CleaningRuleLabel(string id) => id.ToLowerInvariant() switch
    {
        "missing-title" => "條目名稱空白",
        "duplicate-title" => "名稱重複",
        "edge-whitespace" => "欄位前後空白",
        "control-characters" => "欄位含控制字元",
        "excel-error" => "Excel 錯誤值",
        _ => id
    };
    private static string FormatObject(object value) => string.Join(Environment.NewLine, value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => $"{p.Name}: {p.GetValue(value) ?? ""}"));
    private void ShowError(Exception ex) { _status.Text = "操作失敗。"; MessageBox.Show(this, ex.Message, "PersonalWiki", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
}

internal sealed class EntryButtonSettingsDialog : Form
{
    private readonly NumericUpDown _width = new() { Minimum = 100, Maximum = 360, Increment = 10, Width = 120 };
    private readonly NumericUpDown _height = new() { Minimum = 36, Maximum = 160, Increment = 4, Width = 120 };
    public int ButtonWidth => (int)_width.Value;
    public int ButtonHeight => (int)_height.Value;

    public EntryButtonSettingsDialog(int width, int height, string title = "輸入視窗按鈕大小設定", string applyTarget = "新增／編輯條目視窗內的「確定儲存／取消」按鈕", int minimumWidth = 100, int minimumHeight = 36, int widthIncrement = 10, int heightIncrement = 4)
    {
        _width.Minimum = minimumWidth;
        _height.Minimum = minimumHeight;
        _width.Increment = widthIncrement;
        _height.Increment = heightIncrement;
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false; MaximizeBox = false;
        DpiLayout.EnableDpiAutoScale(this);
        Font = new Font("Microsoft JhengHei UI", 10F);
        ClientSize = new Size(520, 250);
        _width.Value = Math.Clamp(width, (int)_width.Minimum, (int)_width.Maximum);
        _height.Value = Math.Clamp(height, (int)_height.Minimum, (int)_height.Maximum);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.Controls.Add(new Label { Text = "按鈕寬度（96 DPI 基準）：", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        layout.Controls.Add(_width, 1, 0);
        layout.Controls.Add(new Label { Text = "按鈕高度（96 DPI 基準）：", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        layout.Controls.Add(_height, 1, 1);
        layout.Controls.Add(new Label { Text = $"尺寸以 96 DPI 為基準，會依螢幕縮放比例調整。\r\n儲存後套用至：{applyTarget}。", AutoSize = true, Dock = DockStyle.Fill }, 0, 2);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 2)!, 2);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
        var save = new Button { Text = "儲存", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.AddRange(new Control[] { cancel, save });
        layout.Controls.Add(buttons, 0, 3); layout.SetColumnSpan(buttons, 2);
        Controls.Add(layout); AcceptButton = save; CancelButton = cancel;
        DpiLayout.KeepInsideWorkingArea(this);
    }
}

internal sealed class EntryEditDialog : Form
{
    private readonly TextBox _title = new() { Dock = DockStyle.Fill, Multiline = false, Margin = new Padding(4, 8, 4, 8) };
    private readonly TextBox _type = new() { Dock = DockStyle.Fill, Multiline = false, Margin = new Padding(4, 8, 4, 8) };
    private readonly TextBox _summary = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = new Padding(4, 8, 4, 8) };
    private readonly TableLayoutPanel _layout;
    public string EntryTitle => _title.Text.Trim();
    public string EntryType => _type.Text.Trim();
    public string? EntrySummary => string.IsNullOrWhiteSpace(_summary.Text) ? null : _summary.Text.Trim();

    public EntryEditDialog(string caption, string title, string type, string summary, (int Width, int Height) buttonSize)
    {
        Text = caption;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimizeBox = false; MaximizeBox = true;
        // 不讓 WinForms 再對已配置好的 TableLayoutPanel 做第二次自動縮放。
        // 尺寸在視窗 Handle 建立後依實際 DPI 明確設定，避免 AutoScaleDimensions
        // 與執行環境 DPI 不一致時整個對話框被縮成狹窄小窗。
        AutoScaleMode = AutoScaleMode.None;
        Font = new Font("Microsoft JhengHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        _title.Text = title; _type.Text = type; _summary.Text = summary;
        // 編輯既有長名稱時，預設從開頭顯示，避免游標停在末端造成前段被水平捲出。
        _title.SelectionStart = 0; _title.SelectionLength = 0;
        _title.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        _type.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        _summary.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        _layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4,
            Padding = new Padding(14), GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            Margin = Padding.Empty
        };
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82)); // Shown 後依實際按鈕高度重新計算，避免 300% DPI 下按鈕被固定列高裁切。
        AddLabel("條目名稱：", 0, ContentAlignment.MiddleLeft);
        _layout.Controls.Add(_title, 1, 0);
        AddLabel("類型：", 1, ContentAlignment.MiddleLeft);
        _layout.Controls.Add(_type, 1, 1);
        AddLabel("摘要／介紹：", 2, ContentAlignment.TopLeft);
        _layout.Controls.Add(_summary, 1, 2);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 4, 4, 4), Margin = Padding.Empty, AutoSize = false };
        var buttonFont = new Font("Microsoft JhengHei UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        var ok = new Button { Text = "確定儲存", Font = buttonFont, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(150, 64), Padding = new Padding(16, 10, 16, 10), Margin = new Padding(8), UseCompatibleTextRendering = true, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "取消", Font = buttonFont, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(120, 64), Padding = new Padding(16, 10, 16, 10), Margin = new Padding(8), UseCompatibleTextRendering = true, DialogResult = DialogResult.Cancel };
        buttons.Controls.AddRange(new Control[] { cancel, ok });
        _layout.Controls.Add(buttons, 0, 3); _layout.SetColumnSpan(buttons, 2);
        Controls.Add(_layout); AcceptButton = ok; CancelButton = cancel;
        ok.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_title.Text))
            {
                MessageBox.Show(this, "條目名稱不可空白。", "資料未完整", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.None;
            }
        };
        void ApplyButtonLayoutForCurrentDpi()
        {
            var dpiScale = Math.Max(0.5F, DeviceDpi / 96F);
            // 使用者設定以 96 DPI 為基準；在每次 DPI 變更後重新套用。
            var configuredWidth = (int)Math.Ceiling(buttonSize.Width * dpiScale);
            var configuredHeight = (int)Math.Ceiling(buttonSize.Height * dpiScale);
            ok.AutoSize = false; cancel.AutoSize = false;
            ok.Size = new Size(configuredWidth, configuredHeight);
            cancel.Size = new Size(configuredWidth, configuredHeight);
            ok.MinimumSize = cancel.MinimumSize = new Size(configuredWidth, configuredHeight);
            var buttonPadding = (int)Math.Ceiling(8 * dpiScale);
            var buttonMargin = (int)Math.Ceiling(6 * dpiScale);
            ok.Padding = cancel.Padding = new Padding(buttonPadding);
            ok.Margin = cancel.Margin = new Padding(buttonMargin);
            // 按鈕列高度包含按鈕本體、上下 Margin 與 FlowLayoutPanel 內距。
            var buttonRowHeight = configuredHeight + buttonMargin * 2 + buttonPadding * 2 + (int)Math.Ceiling(12 * dpiScale);
            _layout.RowStyles[3].SizeType = SizeType.Absolute;
            _layout.RowStyles[3].Height = buttonRowHeight;
            buttons.Padding = new Padding(0, buttonPadding / 2, buttonPadding / 2, buttonPadding / 2);
            buttons.AutoScroll = true;
        }

        Shown += (_, _) =>
        {
            ApplyRuntimeLayout();
            ApplyButtonLayoutForCurrentDpi();
            // 編輯既有名稱時，TextBox 原生游標可能停在字串尾端；
            // 明確將初始視窗位置移回開頭，讓使用者一開啟就看見完整名稱的前段。
            BeginInvoke(new Action(() =>
            {
                _title.SelectionStart = 0;
                _title.SelectionLength = 0;
                _title.ScrollToCaret();
            }));
        };
        DpiChanged += (_, _) =>
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed) return;
                    ApplyRuntimeLayout();
                    ApplyButtonLayoutForCurrentDpi();
                }));
            }
            catch (InvalidOperationException) { /* 視窗關閉時略過 DPI 更新。 */ }
        };
        DpiLayout.KeepInsideWorkingArea(this);
    }

    private void AddLabel(string text, int row, ContentAlignment alignment)
    {
        _layout.Controls.Add(new Label
        {
            Text = text, AutoSize = false, Dock = DockStyle.Fill,
            TextAlign = alignment, AutoEllipsis = false,
            Margin = new Padding(4, 8, 8, 8), UseMnemonic = false
        }, 0, row);
    }

    private void ApplyRuntimeLayout()
    {
        var dpiScale = Math.Max(1F, DeviceDpi / 96F);
        var area = Screen.FromControl(this).WorkingArea;
        var targetWidth = Math.Min((int)(900 * dpiScale), (int)(area.Width * 0.88));
        var targetHeight = Math.Min((int)(660 * dpiScale), (int)(area.Height * 0.86));
        targetWidth = Math.Max(Math.Min(targetWidth, area.Width - 40), Math.Min(680, area.Width - 20));
        targetHeight = Math.Max(Math.Min(targetHeight, area.Height - 60), Math.Min(480, area.Height - 20));
        ClientSize = new Size(targetWidth, targetHeight);
        MinimumSize = new Size(Math.Min((int)(680 * dpiScale), area.Width - 20), Math.Min((int)(480 * dpiScale), area.Height - 40));
        _layout.ColumnStyles[0].Width = Math.Min(170 * dpiScale, targetWidth * 0.28F);
        _layout.RowStyles[0].Height = 58 * dpiScale;
        _layout.RowStyles[1].Height = 58 * dpiScale;
        _layout.RowStyles[3].Height = 112 * dpiScale;
        CenterToParent();
        try
        {
            var diagnosticDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PersonalWiki", "diagnostics");
            Directory.CreateDirectory(diagnosticDir);
            var diagnosticPath = Path.Combine(diagnosticDir, $"ui-{DateTime.UtcNow:yyyyMMddHHmmssfff}.txt");
            SafeFileOutput.WriteAllText(diagnosticPath,
                $"Version=2.0.2{Environment.NewLine}DPI={DeviceDpi}{Environment.NewLine}FormClient={ClientSize.Width}x{ClientSize.Height}{Environment.NewLine}WorkingArea={area.Width}x{area.Height}{Environment.NewLine}TitleBounds={_title.Bounds}{Environment.NewLine}TypeBounds={_type.Bounds}{Environment.NewLine}SummaryBounds={_summary.Bounds}{Environment.NewLine}LabelColumn={_layout.ColumnStyles[0].Width}");
        }
        catch { /* 診斷檔寫入失敗不影響新增／編輯功能 */ }
    }
}


internal sealed class DuplicateEntryReportDialog : Form
{
    public DuplicateEntryReportDialog(IReadOnlyList<DuplicateEntryGroup> groups)
    {
        DpiLayout.EnableDpiAutoScale(this);
        Text = "重複條目檢查（唯讀）"; StartPosition = FormStartPosition.CenterParent;
        Width = 1100; Height = 720; MinimumSize = new Size(700, 460);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(10) };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.Controls.Add(new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
            Text = groups.Count == 0 ? "沒有發現重複名稱。資料庫未被修改。" : $"發現 {groups.Count} 組重複名稱，共涉及 {groups.Sum(x => x.Count)} 筆條目。此報告只供檢查，不會自動合併或刪除。" }, 0, 0);
        var list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
        list.Columns.Add("條目名稱", 260); list.Columns.Add("類型", 140); list.Columns.Add("狀態", 100); list.Columns.Add("更新時間", 190); list.Columns.Add("條目 ID", 280);
        foreach (var group in groups)
        {
            var heading = new ListViewItem($"【重複 {group.Count} 筆】{group.Title}"); heading.BackColor = SystemColors.Info; list.Items.Add(heading);
            foreach (var entry in group.Entries)
            { var row = new ListViewItem(entry.Title); row.SubItems.Add(entry.EntryType); row.SubItems.Add(entry.IsArchived ? "已封存" : "使用中"); row.SubItems.Add(entry.UpdatedAt); row.SubItems.Add(entry.Id); list.Items.Add(row); }
        }
        layout.Controls.Add(list, 0, 1);
        var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.RightToLeft, WrapContents = true, Padding = new Padding(4) };
        var close = new Button { Text = "關閉", AutoSize = true, MinimumSize = new Size(0, 40), DialogResult = DialogResult.OK }; var export = new Button { Text = "匯出 CSV 報告", AutoSize = true, MinimumSize = new Size(0, 40) };
        export.Click += (_, _) => { using var save = new SaveFileDialog { Filter = "CSV 檔案|*.csv", FileName = "PersonalWiki-duplicate-report.csv", OverwritePrompt = false }; if (save.ShowDialog(this) != DialogResult.OK) return;
            var sb = new System.Text.StringBuilder("\"條目名稱\",\"類型\",\"狀態\",\"更新時間\",\"條目 ID\"\r\n");
            foreach (var group in groups) foreach (var e in group.Entries) { string Q(string v) => "\"" + v.Replace("\"", "\"\"") + "\""; sb.Append(string.Join(",", new[] { Q(e.Title), Q(e.EntryType), Q(e.IsArchived ? "已封存" : "使用中"), Q(e.UpdatedAt), Q(e.Id) })).Append("\r\n"); }
            SafeFileOutput.WriteAllText(save.FileName, "\uFEFF" + sb.ToString(), new System.Text.UTF8Encoding(true)); MessageBox.Show(this, "CSV 報告已匯出；資料庫沒有變更。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information); };
        footer.Controls.Add(close); footer.Controls.Add(export); layout.Controls.Add(footer, 0, 2); Controls.Add(layout); AcceptButton = close;
        DpiLayout.KeepInsideWorkingArea(this);
    }
}
