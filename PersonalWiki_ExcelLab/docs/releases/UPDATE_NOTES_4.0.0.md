# 4.0.0 更新說明（第一批：搜尋結果管理）

## 新增功能
- CLI `entry-query` 新增 `--output <report.json>`，可將當前搜尋條件、分頁資訊、符合總筆數及本頁條目完整輸出為 JSON。
- 未指定 `--output` 時維持原本 JSON stdout 行為，既有腳本相容。
- 匯出透過 `SafeFileOutput.WriteAllText` 使用 `FileMode.CreateNew`；目的檔已存在即拒絕覆寫。
- 匯出完成後顯示完整目的路徑、符合總筆數及本頁筆數。

## 使用方式
```bash
dotnet run --project PersonalWiki.ExcelLab.csproj -- entry-query "<database.db>" "關鍵字" --tag "收藏" --page 1 --page-size 50 --output "reports/search-results.json"
```
輸出為 UTF-8 JSON。請使用新的輸出檔名；若檔案已存在，程式會停止並保留原檔。

## 安全與相容性
- 查詢仍透過唯讀 SQLite 連線執行。
- 不修改 Schema（仍為 `user_version=1`）、資料庫路徑、桌面設定路徑或 NuGet 版本。
- 不修改既有搜尋排序、篩選、分頁及匯入流程。

## 驗證限制
- 新增 Python／SQLite fixture 與靜態契約測試；未能取代 C# 編譯及 CLI 實際執行。
- 本工作環境沒有 .NET SDK／Windows，restore/build、CLI acceptance、WinForms 實機仍待測。
