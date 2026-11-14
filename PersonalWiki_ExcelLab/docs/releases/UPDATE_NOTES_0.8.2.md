# 0.8.2 開發紀錄

- 新增唯讀 `db-note-audit`，檢查空白筆記、無效建立/更新時間及完全相同筆記候選。
- 重複內容只供人工複核；JSON 不包含筆記正文。
- 報告採 CreateNew，不覆蓋；不修改 SQLite、不刪除筆記。
- SQLite Schema Version 1 不變；無新增第三方套件。
- 依開發階段安排，本版未執行 build/功能測試，尚未驗收。
