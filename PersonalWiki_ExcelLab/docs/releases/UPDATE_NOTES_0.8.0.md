# 0.8.0 開發更新

- 新增 `db-image-duplicate-report <database.db> <report.json>`。
- 以圖片記錄中的 SHA-256 分組，列出同一內容雜湊被多筆圖片關聯引用的候選。
- 報告包含圖片 ID、條目 ID/標題、路徑及說明文字，供人工判斷。
- 唯讀開啟 SQLite；不計算或改寫既有雜湊、不刪除檔案或資料庫關聯。
- JSON 報告採 CreateNew，不覆蓋既有檔案。
- SQLite Schema Version 1 不變；無新增第三方套件。
- 依使用者要求，本版僅開發與封裝，未執行 build 或功能測試。
