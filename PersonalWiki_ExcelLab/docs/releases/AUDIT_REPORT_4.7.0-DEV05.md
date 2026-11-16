# 4.7.0 DEV05 靜態變更稽核

## 變更範圍
- 新增 `PersonalWiki.Desktop/RelatedEntryPreviewDialog.cs`。
- `PersonalWiki.Desktop/MainForm.cs`：相關條目頁增加摘要預覽按鈕、雙擊入口及目前列資料傳遞。
- 新增 `scripts/run-4.7.0-DEV05-python-tests.py`。
- 更新兩個專案版本標示、三個標準 BAT 版本文字及版本文件。

## 保護檢查（依程式修改範圍）
- 只使用既有 `RelatedEntryItem`；沒有修改 `RelatedEntryService` 或推薦 SQL。
- 預覽視窗不執行資料庫寫入、不建立或修改關係、不改目前搜尋條件。
- SQLite Schema、`user_version`、資料庫路徑、桌面設定路徑及 NuGet 版本未修改。
- Excel 匯入預檢、來源唯讀、匯入前備份、來源快照驗證及交易安全流程未修改。
- 不修改主功能按鈕 80×22 設定及其他正常輸入欄位。

## 未完成驗證
- 未執行 Python 靜態測試、.NET restore/build、CLI acceptance、程式啟動及 Windows 實機測試。
- 本文件只記錄靜態修改範圍，不代表功能驗收或回歸測試通過。
