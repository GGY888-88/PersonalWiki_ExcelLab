# PersonalWiki ExcelLab 1.1.1（開發版）

## 新增
- 桌面程式記錄最近一次成功開啟／建立的資料庫路徑。
- 新增「開啟上次資料庫」操作，重新啟動後可快速回到上次使用的知識庫。
- 設定存放於 Windows 使用者 LocalAppData\PersonalWiki\desktop-settings.json，與資料庫分離。
- 設定檔損毀或無法讀取時，程式仍可正常啟動並手動開啟資料庫。

## 安全與相容性
- 不變更 SQLite Schema Version 1。
- 不新增 NuGet 套件。
- 不會自動建立、匯入或修改知識庫；只有使用者按下「開啟上次資料庫」才會重新開啟。
- 設定檔只保存最近使用的資料庫路徑，不保存資料庫內容或筆記。

## 測試狀態
- 已完成靜態檢查及 ZIP 完整性檢查。
- 本建置環境沒有 dotnet CLI，未執行 Windows Build／桌面功能測試。
