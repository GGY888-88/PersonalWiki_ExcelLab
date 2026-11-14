# 3.7.0 稽核報告

## 修改範圍
- `Services/EntryQueryService.cs`：日期正規化、日期範圍驗證、四個日期條件及排序白名單。
- `Program.cs`：CLI 日期及排序參數。
- `PersonalWiki.Desktop/MainForm.cs`：標籤、四個可選日期欄位、排序與清除條件。
- `scripts/test-3.7.0-composite-filters.py`：SQLite fixture 與靜態契約測試。

## 安全與相容性
- 查詢連線保持唯讀；SQL 條件值均使用參數。
- 排序只允許固定 SQL 白名單，禁止使用者輸入直接拼入 ORDER BY。
- 日期上界採次日零時排他比較，包含迄日整天；`9999-12-31` 有專用邊界處理。
- COUNT 與資料查詢共用相同 WHERE 條件；頁碼與每頁 1–200 上限保留。
- 無 Schema／NuGet／資料位置變更。

## 未測試項目
目前沒有 .NET SDK／Windows；尚未執行 C# 編譯、Microsoft.Data.Sqlite 執行期、CLI acceptance 及 WinForms 實機測試。需確認不同時區來源資料的字串排序一致性及日期選擇器在不同 DPI 下的版面。
