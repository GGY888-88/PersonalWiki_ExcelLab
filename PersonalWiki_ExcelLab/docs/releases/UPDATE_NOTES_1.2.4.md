# PersonalWiki ExcelLab 1.2.4 更新紀錄

- 修正新增／編輯條目視窗在 Windows 字型/DPI 自動縮放下尺寸異常縮小的問題。
- 改為明確的 ClientSize 與最小視窗尺寸，並加寬標籤欄，避免中文標籤逐字換行。
- 保留 1.2.3 的其他功能與資料庫 Schema v1，不變更資料庫、不新增相依套件。

## 更新方式
1. 解壓縮至新資料夾（資料夾名稱仍為 `PersonalWiki_ExcelLab`）。
2. 使用原本的 SQLite 資料庫開啟，不要刪除或覆蓋資料庫。
3. 執行：

```powershell
dotnet run --project .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
```

## 驗證狀態
- 已完成程式碼差異檢查及 ZIP 完整性檢查。
- 開發環境沒有 .NET SDK／Windows Forms，未能執行 Windows Build 或實際介面驗收；需在使用者 Windows 環境確認新增及編輯視窗尺寸。
