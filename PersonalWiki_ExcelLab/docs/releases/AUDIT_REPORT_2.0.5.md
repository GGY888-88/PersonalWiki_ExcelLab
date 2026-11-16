# PersonalWiki ExcelLab 2.0.5 靜態稽核報告

## 修改範圍
- `PersonalWiki.Desktop/DesktopSettingsService.cs`：儲存前讀取既有設定檔時，JSON 格式錯誤或內容為 null 不再回退空白設定；改為丟出 `InvalidDataException` 並保留原始檔，避免靜默覆蓋復原來源。
- `Program.cs`：self-test 新增 SafeFileOutput 防覆寫、原子替換成功、替換失敗暫存清理測試；快照測試擴大至 8 MiB 串流產生。
- CLI／桌面 csproj、CLI help、自我測試報告及 acceptance test pack 更新為 2.0.5。
- 新增 2.0.5 更新說明、測試指南與交接文件。

## 安全檢查
- SQLite Schema `user_version=1` 不變，未新增 NuGet 套件。
- 不變更資料庫及設定檔位置。
- 匯入流程仍預檢、備份、再次預檢、唯讀快照、快照預檢、交易匯入及批次驗證。
- 設定檔格式損毀時停止儲存，不會自動刪除或覆蓋原始設定檔。

## 靜態驗證
- 待封裝時檢查兩個 csproj XML、版本字串、self-test 新斷言、交接文件及 ZIP CRC。
- 本環境沒有 .NET SDK、Windows、PowerShell；不宣稱 build 或執行期測試通過。

## 尚未驗證
- Windows 檔案鎖定、磁碟滿載、強制終止及大型檔案效能。
- 桌面設定檔損毀時實際 UI 錯誤呈現及復原流程。
- WinForms 多工作表匯入與 300% DPI 實機驗收。
