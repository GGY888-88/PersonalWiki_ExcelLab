# PersonalWiki ExcelLab 2.5.3 更新說明

## 桌面主畫面按鈕裁字修正
- 主畫面工具列按鈕（包含「新增條目」、「編輯條目」）依 `PreferredSize` 動態計算工具列高度。
- 使用 `UseCompatibleTextRendering`，並增加按鈕內距及外距，降低高 DPI／字型縮放下文字被裁切的情況。
- 工具列可水平捲動；按鈕列高度不再依賴固定 42 px。
- 「輸入視窗按鈕大小」設定仍只套用於新增／編輯條目對話框中的「確定儲存／取消」按鈕，不影響主畫面功能按鈕。

## 文件管理
- 將歷代交接、更新說明、測試指南、稽核報告及 UI 測試矩陣集中至 `docs/releases/`。
- 新增 `docs/releases/INDEX.md` 作為文件索引。

## 未變更
- SQLite Schema `user_version=1`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- 資料庫及桌面設定檔位置、匯入安全流程及資料安全限制。
