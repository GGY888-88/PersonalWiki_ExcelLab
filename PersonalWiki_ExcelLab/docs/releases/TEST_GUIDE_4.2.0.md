# 4.2.0 測試指南

## 自動化測試

```bash
python scripts/run-4.2.0-python-tests.py
```

包含：
- 4.2.0 查詢計畫服務靜態契約與 25,000 筆 SQLite 合成資料一致性／效能觀察。
- 4.1.0 桌面摘要複製、4.0.0 JSON 匯出、3.9.0 發行安全、3.8.0 圖片／關聯、3.7.0 篩選、3.6.0 搜尋排序及資料庫安全回歸。

## CLI 查詢計畫

```bash
dotnet run --project PersonalWiki.ExcelLab.csproj -- entry-query-plan "<database.db>" --type "人物" --tag "研究" --archived exclude --sort updated-desc
```

命令只讀取資料庫並輸出 JSON。SQLite 計畫中的 `SCAN`、`SEARCH`、`USE TEMP B-TREE` 是規劃器資訊，不等於實際耗時；需在隔離副本上以代表性資料另外量測。

## 尚待人工驗收
- Windows/.NET 10：`dotnet restore`、CLI build、`entry-query-plan` 真實資料庫執行。
- WinForms 搜尋結果與 CLI 使用同一組篩選時，逐筆核對 ID、排序、總筆數與分頁。
- 不得在正式資料庫執行任何寫入測試；使用資料庫副本。
