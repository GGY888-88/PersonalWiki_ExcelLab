# 4.6.1 測試指南

## 環境
Windows 10／11、.NET 10 SDK、Python 3。套件沿用 4.6.0，不需新增安裝。

## 自動測試
在專案根目錄執行：

```powershell
python scripts/test-4.6.1-ui.py
python scripts/run-4.6.1-python-tests.py
```

## Windows 桌面實機驗收

```powershell
dotnet build PersonalWiki.Desktop/PersonalWiki.Desktop.csproj -c Release
dotnet run --project PersonalWiki.Desktop/PersonalWiki.Desktop.csproj -c Release
```

檢查：
1. 主畫面不再為「目前資料庫位置」保留獨立一整列。資料庫路徑應出現在搜尋列尾端，確認仍可 Ctrl+A／Ctrl+C，長路徑可左右移動檢視。若搜尋列超出視窗寬度，可使用原有水平捲動。
2. 開啟「Excel 匯入精靈」並進入第 2 步，確認「Excel 標題列」、「第一筆資料列」及括號提示完整顯示，沒有上下裁切。
3. 選擇附帶的 `samples/PersonalWiki_Import_Sample.xlsx`，只進行預覽及欄位設定測試；若要實際匯入，必須使用測試資料庫副本及新的備份檔案路徑。
4. 確認原有搜尋、匯入檢查及其他功能沒有回歸。

## 安全
不要用正式資料庫做匯入測試。測試資料庫請先複製到全新測試目錄；不要覆寫既有備份或報告。
