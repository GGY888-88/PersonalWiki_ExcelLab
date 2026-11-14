# PersonalWiki ExcelLab 2.5.1 測試指南

## 編譯錯誤回歸
在解壓後的 `PersonalWiki_ExcelLab` 目錄執行：

```powershell
dotnet restore .\PersonalWiki.ExcelLab.csproj
dotnet build .\PersonalWiki.ExcelLab.csproj -c Release
dotnet build .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
```

預期：不再出現 `Program.cs(769,31)` 或 `(770,33)` 的 CS1061；兩個 self-test 結果應正常編譯。

## 自我測試

```powershell
dotnet run -c Release -- self-test .\test-output\self-test-2.5.1
```

預期：報告內 `SnapshotSafety.Passed`、`SafeFileOutputSafety.Passed` 可序列化並保留各自詳細欄位；測試只使用輸入活頁簿的工作副本及隔離測試目錄。測試輸出資料夾請使用全新路徑；程式會在該資料夾建立 `SelfTestReport.json`。

## 整合驗收

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\run-acceptance-tests.ps1
```

## 本版驗證限制
本打包環境缺少 `dotnet`、Windows 與 PowerShell，以上編譯及執行測試尚未由本環境實際執行。使用者回報的 CS1061 已針對型別根因修正，但仍需 Windows/.NET 10 實際建置確認。
