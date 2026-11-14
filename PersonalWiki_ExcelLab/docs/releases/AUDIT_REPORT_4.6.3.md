# 4.6.3 稽核報告

## 變更檔案
- `PersonalWiki.Desktop/VerticallyCenteredTextBox.cs`：切回 Windows 原生單行 EDIT（`Multiline=false`），保留 `AutoSize=false` 以接受版面配置高度，避免多行 EDIT 與 `PlaceholderText` Cue Banner 不相容；移除自訂 P/Invoke 文字矩形定位。
- `PersonalWiki.ExcelLab.csproj`、`PersonalWiki.Desktop/PersonalWiki.Desktop.csproj`：版本同步更新至 4.6.3。
- `01_建置程式.bat`、`02_啟動程式.bat`、`03_建置並啟動.bat`：更新版本標示；維持 ASCII／CRLF；build 輸出改為只寫入 `build_release.log`，錯誤時以記事本開啟，不再把建置日誌原樣輸出至 CMD。
- `scripts/test-4.6.3-input-alignment.py`、`scripts/run-4.6.3-python-tests.py`：新增垂直置中與 BAT 編碼回歸檢查及整合執行器。
- 更新 4.6.2 歷史回歸測試對目前 4.6.3 版本的相容檢查，並更新 README、發行索引及本版交接文件。

## 保護範圍
- 未修改 `Services/`、`Models.cs`、CLI `Program.cs`、`PersonalWiki.Desktop/DesktopSettingsService.cs`、`DpiLayout.cs`、`MainForm.cs`、`ImportDialogs.cs`、`DateFilterControl.cs`。
- SQLite `PRAGMA user_version=1`，沒有 Schema migration。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`，版本不變。
- 桌面設定路徑 `%LOCALAPPDATA%\PersonalWiki\desktop-settings.json` 不變；本版沒有重建或覆寫使用者設定，所以現有按鈕 80×22 設定保留。
- Excel 原始檔唯讀、匯入前預檢／備份／快照／交易及匯入安全流程均未觸及。

## 自動測試
整合執行入口：`python scripts/run-4.6.3-python-tests.py`，目前結果全部通過：
- 4.6.3 輸入框／BAT 專項：26/26。
- 4.6.2 DPI、篩選列、表格、視窗空間、建置修正及主畫面回歸：23/23、15/15、15/15、12/12、10/10、15/15。
- 4.6.1 UI 回歸：8/8。
- 4.6.0 查詢 acceptance 契約：35/35（包含預期的失敗案例模擬，用來驗證失敗報告產生機制）。
- 合計 159 項靜態／Python 回歸檢查通過。
- 上述測試不能取代 .NET 編譯或 Windows WinForms 實機顯示驗收。

## 未完成的實機驗收
- 本次工作容器未安裝 .NET 10 SDK，亦非 Windows，無法執行 `dotnet build` 或 WinForms 實機顯示驗收。
- 需在繁體中文 Windows 下完成 100%／150%／200%／300% DPI 輸入框及三個 BAT 的手動測試；尤其要確認 PlaceholderText（Cue Banner）與實際輸入文字都居中。
- 不得將本報告的靜態通過等同於已確認 Windows 300% 實際畫面正常。
