# PersonalWiki ExcelLab 1.0.7

## 桌面端功能整合
- 條目：建立、編輯名稱/類型/摘要、封存/還原。
- 搜尋：全文關鍵字、類型文字篩選、封存狀態篩選，單次最多 200 筆。
- 筆記：新增、編輯、刪除。
- 圖片：選擇圖片建立關聯、預覽、移除關聯；不複製或刪除原圖。
- 資料庫：建立/開啟 Schema Version 1 資料庫。
- Excel：桌面端選擇來源、mapping、rules、備份路徑，呼叫既有安全匯入工作流程。
- 新增 `scripts/publish-desktop-portable.ps1`，可產生 Windows x64 self-contained single-file 發佈資料夾；目的資料夾已存在時停止，不覆蓋。

## 安全限制
- 不會在程式啟動或開啟資料庫時自動匯入 Excel。
- Excel 匯入來源唯讀；匯入前先預檢、健康檢查與建立驗證備份。
- 匯入前檢查來源活頁簿與 mapping/rules 雜湊是否變動。
- SQLite 寫入沿用既有服務交易及批次驗證。
- 移除圖片只刪資料庫關聯，不刪硬碟原圖。
- Schema Version 1 不變；DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14；無新增套件。

## 尚未完成/限制
- mapping/rules 需由既有範本預先產生，尚無視覺化 mapping 編輯器。
- 搜尋目前單頁最多 200 筆，尚無頁碼導覽。
- 圖片預覽以本機可解碼圖片為主，SVG 等格式可能不顯示。
- 本版尚未在 Windows/.NET 10 SDK 環境 Build 或執行；不得視為已驗收。
