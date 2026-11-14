# PersonalWiki ExcelLab 1.5.1 測試指南

## 1. 準備
- Windows 10/11 x64，安裝 .NET 10 SDK。
- 解壓至新資料夾，使用測試資料庫；首次測試不要選唯一正式資料庫。
- 執行 `啟動 PersonalWiki.bat`，開啟測試資料庫。

## 2. 資料庫健康檢查
1. 按「資料庫健康檢查」。
2. 確認視窗顯示資料庫路徑、Schema 版本、quick_check、外鍵問題數及唯讀模式。
3. 正常資料庫應顯示 quick_check 通過、外鍵問題 0、結果健康。
4. 此操作不得改變資料庫檔案內容或資料筆數。

## 3. 立即備份
1. 按「立即備份資料庫...」，選擇一個全新的 `.db` 路徑。
2. 確認顯示備份完成、檔案大小及完整性結果。
3. 再次備份到同一目的路徑，應拒絕覆蓋並保留原備份。
4. 將目的路徑改為來源資料庫本身，服務應拒絕相同路徑。
5. 用 SQLite 工具對備份執行 `PRAGMA integrity_check;` 及 `PRAGMA foreign_key_check;`，確認完整性。

## 4. 回歸與安全
- 再執行 `TEST_GUIDE_1.5.0.md` 的 CLI 驗收。
- 確認原始 Excel 唯讀；移除圖片關聯不刪除原始圖片。
- 不以正式資料庫做破壞性測試；所有測試輸出使用新檔名及新資料夾。

## 限制
本交付環境沒有 .NET 10 SDK、PowerShell Core 或 Windows，因此未執行編譯、CLI acceptance 或 WinForms 實機測試。請以 Windows/.NET 10 實機結果作為最終驗收依據。
