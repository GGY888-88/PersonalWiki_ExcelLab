# PersonalWiki ExcelLab 1.2.1（Build 修正版）

## 修正
- 修正 `Services/DatabaseImportBatchAuditService.cs` 的 Create 方法回傳型別名稱錯誤：`DatabaseImportBatchAuditReport` 改為已宣告的 `ImportBatchAuditReport`。
- 同步修正巢狀 `package/PersonalWiki_ExcelLab` 快照。
- CLI 與桌面專案版本更新為 1.2.1。

## Windows PowerShell 建置
若執行政策阻擋 `.ps1`，使用單次繞過方式：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\build-desktop.ps1
```

此命令只對該次 PowerShell 程序生效，不會永久修改系統執行政策。也可直接執行：

```powershell
dotnet run --project .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
```

## 驗證狀態
本修正版完成靜態型別名稱核對及 ZIP 完整性檢查；尚未在 Windows/.NET 10 環境實際 Build，需由使用者重新執行並回報後續錯誤。
