# 0.7.9 開發更新

- 新增 `db-vacuum-copy`：以 SQLite Backup API 複製來源至暫存檔，僅對副本執行 VACUUM。
- 最佳化後檢查 integrity_check、foreign_key_check、user_version；通過後以不覆蓋方式移至目的路徑。
- 來源資料庫唯讀，不會直接修改來源；目的檔存在時拒絕執行。
- 更新 CLI help、README、版本號及驗收腳本，新增來源 SHA-256 不變與副本存在檢查。
- SQLite Schema Version 1 不變；DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14；無新增套件。
- 依使用者要求暫不 build/test；本版尚未驗收。
