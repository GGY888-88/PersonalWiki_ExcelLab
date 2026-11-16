# 3.9.0 更新說明（整體穩定性與發行候選）

## 本批整合
- 同步 CLI 與 WinForms 專案版本為 3.9.0，修正 CLI 說明仍顯示 3.0.0 的版本字串。
- 發行靜態驗證器納入 3.9.0 交接、更新、測試及稽核文件檢查。
- 發行安全測試檢查 .NET 版本／套件鎖定、SQLite Schema v1、樣本 DB 完整性、Excel 樣本 ZIP 完整性、JSON 樣本格式、備份／還原安全契約、圖片關聯不刪原檔等條件。
- 發行打包工具建立 SHA256 檔案清單，ZIP 解壓根目錄固定為 `PersonalWiki_ExcelLab`，不覆寫既有 ZIP，並在建立前後執行 CRC 驗證。
- 保留 3.6.0、3.7.0、3.8.0 所有既有功能，不修改資料庫 Schema、DB 路徑、桌面設定路徑或 NuGet 套件。

## 安全限制
- 原始 Excel 維持唯讀；匯入安全流程不變。
- 備份、還原、匯出、報告及範本輸出拒絕覆寫既有目的檔。
- 還原只允許新目的地，檢查 SQLite 主檔及 sidecar。
- 不自動刪除／合併條目、筆記、標籤或來源追溯資料。
- 移除圖片關聯不刪除原始圖片；圖片說明只更新 DB metadata。
- 相關條目推薦維持唯讀，不持久化關係。

## 驗證限制
Python／SQLite fixture 與靜態契約測試不等於 .NET 編譯或 Windows 桌面實測。若執行環境沒有 .NET 10 SDK／Windows，必須將 restore/build、CLI acceptance、WinForms UI／DPI／多螢幕列為未測，不得宣稱通過。
