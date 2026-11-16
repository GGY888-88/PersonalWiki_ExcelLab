# 1.3.9 測試指南（可延後執行）

目前不需要立即測試。待有空時再依下列步驟集中驗收。

1. 解壓縮 ZIP 至新資料夾，不要覆蓋正式資料夾或資料庫。
2. 使用 Windows + .NET 10 SDK 執行 `scripts\build-desktop.ps1`，確認桌面專案可編譯。
3. 執行 `scripts\run-acceptance-tests.ps1`，確認報告輸出於新建的 `AcceptanceTestOutput` 資料夾，且報告版本為 1.3.9。
4. 確認驗收報告包含「批次來源核對」及「圖片檔案完整性稽核」兩項結果，且不再因 `$temp` 未定義而失敗。
5. 在空白新輸出路徑執行 `scripts\build-portable.ps1`，確認可建立發布包。
6. 再次指定已存在的輸出資料夾，確認腳本停止且不刪除該資料夾內容。
7. 如要驗收 1.3.8 Excel 工作表欄位版面，請參考 `TEST_GUIDE_1.3.8.md`，並只使用測試資料庫及測試 Excel。

注意：本版未在目前 Linux 開發環境完成 .NET 編譯或 Windows Forms 實機測試。不得以正式資料庫作破壞性測試；原始 Excel 與圖片檔應保持不變。
