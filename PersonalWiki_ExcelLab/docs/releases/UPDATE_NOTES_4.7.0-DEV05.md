# PersonalWiki ExcelLab 4.7.0 DEV05 更新說明

## 新增：相關條目摘要唯讀預覽
- 「相關條目」頁新增「檢視相關條目摘要」操作。
- 可雙擊清單項目，或選取項目後按按鈕開啟摘要預覽。
- 預覽顯示條目名稱、類型、更新時間、摘要及推薦依據（共用標籤數／名稱、相同類型、推薦分數）。
- 可複製條目名稱，方便使用者後續搜尋。
- 預覽不修改資料庫、不建立條目關係、不改變目前搜尋條件，也不會自動切換目前選取條目。

## 保護範圍
- 重用既有 `RelatedEntryService` 回傳的 `RelatedEntryItem`；預覽視窗不額外查詢資料庫。
- 不修改 SQLite Schema／`user_version`、資料庫路徑、桌面設定路徑及 NuGet 套件。
- 不修改既有搜尋、標籤交集、排序、分頁、Excel 匯入安全流程、備份／還原及圖片關聯。
- 既有相關條目排序及推薦演算法不變。

## 版本標示
- Desktop／CLI 專案版本：`4.7.0-DEV05`
- 三個標準 BAT 顯示版本同步為 4.7.0 DEV05。

## 測試狀態
- 已新增 `scripts/run-4.7.0-DEV05-python-tests.py` 靜態契約測試素材。
- 依先開發、後集中測試安排，尚未執行 Python 測試、.NET restore/build、CLI acceptance 或 Windows 實機測試。
