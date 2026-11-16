# PersonalWiki ExcelLab 4.7.0 DEV04 更新說明

## 新增：Excel 匯入問題明細檢視與導覽
- 新增 `PersonalWiki.Desktop/ExcelImportDryRunDialog.cs`，以獨立唯讀視窗呈現 Dry-run 發現的問題。
- 依工作表及檢查規則分列顯示，提供符合列數、最多 20 個 Excel 列號樣本及明細說明。
- 可選取不同問題項目切換明細，並複製目前項目的 Excel 列號，方便回到來源活頁簿定位。
- 原本匯入流程的「預檢暫停」訊息改為明細視窗；「重新調整檢查項目」仍返回既有規則設定步驟，「取消本次匯入」仍立即停止。
- 不顯示或保存儲存格原文，不自動修正、略過或匯入問題資料。

## 保護範圍
- 重用既有 `ImportDryRunService` 與 `ImportDryRunReport`；未修改資料分析規則及報告格式。
- 不修改 SQLite Schema／`user_version`、資料庫路徑、Models、匯入預檢、來源唯讀、匯入前備份及單一交易流程。
- 不修改既有匯入歷史、批次差異比較、搜尋、標籤、圖片及備份／還原功能。
- NuGet 套件版本維持 DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。

## 版本標示
- Desktop／CLI 專案版本：`4.7.0-DEV04`
- 三個標準 BAT 顯示版本同步為 4.7.0 DEV04。

## 測試狀態
- 依目前工作安排，尚未執行 Python 測試、.NET restore/build、CLI acceptance 或 Windows 實機測試。
- 待集中測試時，依 `TEST_GUIDE_4.7.0-DEV04.md` 驗收。
