# PersonalWiki ExcelLab 2.5.2 更新說明

## 修正
- 修正 `PersonalWiki.Desktop/DesktopSettingsService.cs` 未匯入 `PersonalWiki.ExcelLab.Services` 命名空間，導致 `SafeFileOutput` 無法辨識的 CS0103 編譯錯誤。
- 僅新增必要的 using，不改動設定檔儲存行為、原子替換邏輯或既有功能。
- CLI／桌面版本與 acceptance 測試包版號更新為 2.5.2。

## 相容性與安全
- SQLite Schema 維持 `user_version=1`。
- NuGet 相依套件不變。
- 設定檔損毀保護及 SafeFileOutput 原子替換行為保留。

## 驗證限制
已進行原始碼及專案版本靜態檢查、ZIP CRC 檢查；目前執行環境沒有 .NET SDK／Windows／PowerShell，無法代替使用者執行 Windows 編譯與 acceptance。
