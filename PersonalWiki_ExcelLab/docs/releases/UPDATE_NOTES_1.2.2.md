# PersonalWiki ExcelLab 1.2.2（桌面啟動修正版）

## 修正
- 根據 Windows .NET Runtime 記錄，修正 `MainForm.cs` 的 `SplitContainer` 初始化例外。
- `SplitterDistance`、`Panel1MinSize`、`Panel2MinSize` 延後至 Form `Shown` 事件設定，確保使用實際控制項寬度；分隔位置以可用寬度 Clamp。
- 不修改既有資料庫、Excel 原始檔、筆記或圖片；SQLite Schema Version 1 不變。
- 無新增相依套件。

## 更新方式
1. 解壓縮本 ZIP 到新的資料夾。
2. 在新資料夾執行：
   ```powershell
   dotnet run --project .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
   ```
3. 原有資料庫檔案仍放在原位置，使用程式「開啟資料庫」或「開啟上次資料庫」載入；不要覆蓋或刪除資料庫。

## 驗證狀態
- 已依使用者提供的 Windows 例外堆疊定位到 `MainForm.cs` 的 SplitContainer 初始化。
- 已完成程式碼靜態修正、版本同步及 ZIP 完整性檢查。
- 本工具環境沒有 .NET CLI，未執行 Windows Forms Build 或實際啟動；需由使用者在 Windows 驗證。
