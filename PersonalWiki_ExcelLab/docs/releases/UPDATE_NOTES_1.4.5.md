# PersonalWiki ExcelLab 1.4.5 更新說明

## SQLite 備份／還原安全強化
- 備份與還原改採目的資料夾內的唯一暫存檔完成 SQLite Backup API 作業。
- 暫存資料庫完成 `integrity_check` 與 `foreign_key_check` 後，才以不覆蓋方式移至最終目的路徑。
- 備份或還原失敗時，盡力清理由本次流程建立的暫存資料庫及 SQLite sidecar 檔案，不影響來源資料庫。
- 明確拒絕備份／還原來源與目的路徑相同。
- 還原流程增加外鍵驗證；復原驗證的 Schema、健康狀態或逐表筆數不符時，清理由本次建立的還原副本並回報失敗。

## 安全界線
- 不覆蓋任何既有目的檔。
- 不修改原始資料庫、條目、筆記、標籤、圖片或 Excel。
- 不變更 SQLite Schema 或資料庫位置。
