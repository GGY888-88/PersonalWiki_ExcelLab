# PersonalWiki ExcelLab 3.0.0 測試指南

## A. 跨平台靜態檢查（已於打包前執行）
需要 Python 3.10+，不需安裝第三方套件：

```bash
python scripts/verify-release.py
```

成功時應顯示所有 `[PASS]` 並以 `0 failed` 結尾。此工具不等同 C# 編譯或執行期測試。

## B. Windows / .NET 10 完整驗收（待執行）
在 Windows 10/11 安裝 .NET 10 SDK，於專案根目錄執行：

```powershell
dotnet --info
dotnet restore .\PersonalWiki.ExcelLab.csproj
dotnet build .\PersonalWiki.ExcelLab.csproj -c Release --no-restore
dotnet build .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
```

執行 CLI 自我測試，輸出路徑必須是全新路徑：

```powershell
pwsh -NoProfile -File .\scripts\run-self-test.ps1 -Output .\TestOutput-3.0.0-selftest
```

執行完整 acceptance（使用全新輸出目錄；腳本會拒絕覆蓋既有資料）：

```powershell
pwsh -NoProfile -File .\scripts\run-acceptance-tests.ps1 -OutputRoot .\TestOutput-3.0.0-acceptance
```

若腳本參數名稱因目前腳本版本不同，先執行 `Get-Help .\scripts\run-acceptance-tests.ps1 -Full`，不要刪除既有測試輸出目錄。

## C. 3.0.0 JSON 串流解析回歸
- 用相同的匯出 JSON 分別執行 `db-export-validate`、`db-import-preview`、`db-import-verify`；輸出應與 2.6.1 相同。
- 使用格式錯誤 JSON、缺少 tables、缺少必要資料表、超過 MaxDepth 64 的 JSON，確認仍安全拒絕。
- 在 Windows 對正在解析的 JSON 嘗試一般寫入／刪除；目前採 `FileShare.Read`，應拒絕一般寫入／刪除共享。
- 使用大型 JSON（例如 1 GiB，若測試設備容量允許）觀察工作集；此改動只消除整檔 byte[] 複製，`JsonDocument` 本身仍會建立 DOM，並非真正逐筆串流處理。

## D. 匯入與資料安全回歸
- 原始 Excel、mapping、rules 不得被修改；匯入只使用已驗證唯讀快照。
- 驗證預檢、健康檢查、備份驗證、來源雜湊重驗、快照預檢、單一交易匯入、提交前 `quick_check(1)`／`foreign_key_check`、匯入後驗證順序。
- 注入交易中途錯誤、取消、外鍵違規及提交前檢查失敗，確認 rollback；原資料及已建立備份保留。
- 備份／還原／匯出／報告／範本不得覆蓋既有檔案；還原只允許新路徑。
- 驗證 WAL／鎖定／程序中止的備份還原情境。

## E. WinForms 實機驗收（待執行）
- Windows 10/11，100%、125%、150%、200%、300% DPI。
- 單螢幕及多螢幕，主副螢幕不同 DPI，移動視窗後版面應正確重排。
- 主畫面功能按鈕預設 100×68（96 DPI 基準）、文字自然寬度；主畫面按鈕尺寸與輸入視窗按鈕尺寸設定互相獨立。
- 資料庫路徑欄填滿可用寬度，可 Ctrl+A／Ctrl+C 複製完整路徑。
- 開啟／建立資料庫、匯入、搜尋分頁、標籤、圖片關聯、備份、還原、診斷及匯出。

## F. 驗收資料安全
所有測試使用 `samples/` 內樣本與新建隔離資料庫／輸出路徑。禁止指向使用者正式資料庫執行破壞性測試。Schema `user_version=1` 不得變更。
