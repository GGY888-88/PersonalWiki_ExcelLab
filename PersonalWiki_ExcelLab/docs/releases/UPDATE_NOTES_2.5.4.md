# PersonalWiki ExcelLab 2.5.4 更新說明

## 修正目標
2.5.3 以 `PreferredSize` 在視窗建構階段估算主畫面工具列列高，但該時點尚未完成實際 DPI／字型配置；之後按鈕尺寸及容器固定列高未同步更新，造成「新增條目」「編輯條目」等主畫面功能按鈕仍可能裁字。原有「輸入視窗按鈕大小」設定只控制新增／編輯對話框中的「確定儲存／取消」，本來就不會改變主畫面工具列。

## 本版修改
- 新增獨立選項「主畫面功能按鈕大小...」。
- 使用與原有輸入視窗按鈕相同的寬度／高度設定介面，數值以 96 DPI 為基準。
- 預設主畫面工具列功能按鈕為 220 × 64；可調整範圍寬 100–360、高 36–160。
- 所有主畫面工具列按鈕統一套用設定，儲存後立即生效。
- 在 `Shown` 取得實際 DPI 後換算按鈕大小，並依最終按鈕高度重新設定 TableLayoutPanel 第一列及整體高度。
- 監聽 `DpiChanged`，跨螢幕／縮放變更時重新套用尺寸。
- 工具列維持單列水平捲動，不因按鈕加寬而換行或把按鈕擠到下一列。
- 原有「輸入視窗按鈕大小...」設定完全獨立，仍只控制「確定儲存／取消」。
- 新增設定欄位 `MainActionButtonWidth`、`MainActionButtonHeight`；舊版 JSON 沒有這兩欄時使用預設值，不需要搬移或重建設定檔。

## 修改檔案
- `PersonalWiki.Desktop/MainForm.cs`
- `PersonalWiki.Desktop/DesktopSettingsService.cs`
- CLI／桌面 csproj、CLI 版本輸出及 acceptance test pack 版號更新至 2.5.4。
- `README.md` 與 `docs/releases/` 版本文件更新。

## 資料與相容性
- SQLite Schema `user_version=1`，沒有資料庫變更。
- 不新增 NuGet 套件。
- 設定仍存放 `%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`。
- 不修改資料庫路徑、資料表或既有資料。
