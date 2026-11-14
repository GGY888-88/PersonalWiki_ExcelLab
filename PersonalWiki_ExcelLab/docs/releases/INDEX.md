# 發行文件索引

## 最新整合開發版本
- **目前版本：4.7.0 DEV03（Excel 匯入批次差異比較；未執行測試／編譯）**
- **下一版本：4.7.0 DEV04**
- 最新交接：`HANDOFF_4.7.0_DEV03.md`

## 4.7.0 DEV03
- 新增 Excel 匯入批次唯讀差異比較，顯示條目及自訂欄位新增／移除／修改摘要，支援 UTF-8 JSON 匯出。
- 保留既有 SQLite Schema、資料路徑及 Excel 匯入安全流程。
- 文件：`UPDATE_NOTES_4.7.0_DEV03.md`、`HANDOFF_4.7.0_DEV03.md`、`PACKAGING_REPAIR_4.7.0_DEV03.md`。

## 4.7.0 DEV02
- 新增 Excel 匯入批次歷史唯讀檢視。
- 文件：`UPDATE_NOTES_4.7.0_DEV02.md`、`HANDOFF_4.7.0_DEV02.md`。

## 4.7.0 DEV01
- 新增標籤使用統計 JSON 報告。
- 文件：`UPDATE_NOTES_4.7.0_DEV01.md`、`HANDOFF_4.7.0_DEV.md`。

## 4.6.6
- 僅將資料庫路徑、全文搜尋、標籤三個輸入框高度統一微調為 26 logical px，保留置中與既有原生單行輸入行為；不改其他欄位。
- 本輪依使用者指示未執行測試。文件：`UPDATE_NOTES_4.6.6.md`、`TEST_GUIDE_4.6.6.md`、`AUDIT_REPORT_4.6.6.md`、`HANDOFF_4.6.6.md`。

## 4.6.5
- 修正自訂輸入控制項三個代理屬性的 WinForms Designer WFO1000 序列化中繼資料。

## 4.6.4
- 將資料庫路徑、全文搜尋、標籤欄位改為外層固定高度容器＋內層原生單行 TextBox，使用實際建議高度置中；其他輸入框不替換。
- 文件：`UPDATE_NOTES_4.6.4.md`、`TEST_GUIDE_4.6.4.md`、`AUDIT_REPORT_4.6.4.md`、`HANDOFF_4.6.4.md`、`TEST_RESULT_4.6.4.md`、`TEST_RUN_4.6.4.log`、`PLAN_4.7.0.md`。

## 4.6.3
- 改回原生單行 EDIT 以支援提示文字與正常垂直排版，並將 BAT 建置記錄改由記事本開啟。
- 文件：`UPDATE_NOTES_4.6.3.md`、`TEST_GUIDE_4.6.3.md`、`AUDIT_REPORT_4.6.3.md`、`HANDOFF_4.6.3.md`。

## 4.4.0 文件
- `HANDOFF_4.4.0.md`
- `UPDATE_NOTES_4.4.0.md`
- `TEST_GUIDE_4.4.0.md`
- `AUDIT_REPORT_4.4.0.md`

## 4.3.0 文件
- `HANDOFF_4.3.0.md`
- `UPDATE_NOTES_4.3.0.md`
- `TEST_GUIDE_4.3.0.md`
- `AUDIT_REPORT_4.3.0.md`

## 4.2.0 文件
- `HANDOFF_4.2.0.md`
- `UPDATE_NOTES_4.2.0.md`
- `TEST_GUIDE_4.2.0.md`
- `AUDIT_REPORT_4.2.0.md`

## 4.1.0 文件
- `HANDOFF_4.1.0.md`
- `UPDATE_NOTES_4.1.0.md`
- `TEST_GUIDE_4.1.0.md`
- `AUDIT_REPORT_4.1.0.md`

## 4.0.0 文件
- `HANDOFF_4.0.0.md`
- `UPDATE_NOTES_4.0.0.md`
- `TEST_GUIDE_4.0.0.md`
- `AUDIT_REPORT_4.0.0.md`
- 3.6.0／3.7.0／3.8.0 功能及回歸文件均保留。

## 3.9.0 文件
- `HANDOFF_3.9.0.md`
- `UPDATE_NOTES_3.9.0.md`
- `TEST_GUIDE_3.9.0.md`
- `AUDIT_REPORT_3.9.0.md`

## 3.8.0 文件
- `HANDOFF_3.8.0.md`
- `UPDATE_NOTES_3.8.0.md`
- `TEST_GUIDE_3.8.0.md`
- `AUDIT_REPORT_3.8.0.md`

## 3.7.0 文件
- `HANDOFF_3.8.0.md`
- `UPDATE_NOTES_3.7.0.md`
- `TEST_GUIDE_3.7.0.md`
- `AUDIT_REPORT_3.7.0.md`

## 3.6.0 重建文件
- `HANDOFF_3.6.0.md`
- `UPDATE_NOTES_3.6.0.md`
- `TEST_GUIDE_3.6.0.md`
- `AUDIT_REPORT_3.6.0.md`

## 歷史文件
歷代 0.x–3.0.0 的更新說明、測試指南、稽核報告、交接文件及 UI 測試矩陣均保留於本目錄，不刪除、不覆寫。

## 4.6.0
- Acceptance 失敗診斷、部分報告持續保存、單案例重跑及可複製命令。
- 文件：`UPDATE_NOTES_4.6.0.md`、`TEST_GUIDE_4.6.0.md`、`AUDIT_REPORT_4.6.0.md`、`HANDOFF_4.6.0.md`。

## 4.5.0
- 新增 Benchmark `resultIds`，以及真實 .NET CLI 搜尋／Benchmark 自動比對 acceptance runner。
- 文件：`UPDATE_NOTES_4.5.0.md`、`TEST_GUIDE_4.5.0.md`、`AUDIT_REPORT_4.5.0.md`、`HANDOFF_4.5.0.md`。

## 4.6.1
- 主畫面資料庫位置整合至搜尋列；Excel 匯入欄位設定列修正文字裁切。
- 文件：`UPDATE_NOTES_4.6.1.md`、`TEST_GUIDE_4.6.1.md`、`AUDIT_REPORT_4.6.1.md`、`HANDOFF_4.6.1.md`。

## 4.6.2
- WinForms 啟用 PerMonitorV2；主畫面、Excel 匯入精靈及對話框加入 DPI 自適應與工作區限制。
- 文件：`UPDATE_NOTES_4.6.2.md`、`TEST_GUIDE_4.6.2.md`、`AUDIT_REPORT_4.6.2.md`、`HANDOFF_4.6.2.md`。

- 4.6.2 MainUiRefine：`HANDOFF_4.6.2_MainUiRefine.md`、`PATCH_NOTES_4.6.2_MainUiRefine.md`

- 4.6.4 發行素材包含 `scripts/test-4.6.4-input-alignment.py`、`scripts/run-4.6.4-python-tests.py`、DPI／介面回歸副本及三個 ASCII／CRLF BAT。歷史 4.6.3 測試素材保留。
