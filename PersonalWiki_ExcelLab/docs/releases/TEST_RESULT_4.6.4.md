# PersonalWiki ExcelLab 4.6.4 測試結果

執行命令：`python scripts/run-4.6.4-python-tests.py`  
執行結果：完整 runner 回傳碼 0。

| 測試項目 | 結果 |
|---|---:|
| 4.6.4 輸入框／BAT 專項 | 33/33 |
| 4.6.4 DPI 回歸 | 23/23 |
| 搜尋工具列／日期篩選 | 15/15 |
| 響應式表格 | 15/15 |
| 工作區／視窗版面 | 12/12 |
| 4.6.2 建置修正靜態檢查 | 10/10 |
| 4.6.4 主畫面設定回歸 | 15/15 |
| 4.6.1 UI 回歸 | 通過 |
| 4.6.0 acceptance contract | 35/35 |

4.6.0 acceptance contract 會刻意模擬 CLI 失敗，驗證錯誤資訊、部分報告及重跑命令可被保留。因此日誌中的 `Acceptance: 0/1 passed, 1 failed` 是預期的錯誤路徑模擬，不代表該自動測試失敗；該腳本最後以 35/35 contract checks 通過並回傳成功。

**未執行項目：** 此工作環境沒有 .NET SDK／C# compiler／Windows WinForms，因此沒有執行 `dotnet build`、Windows 啟動或 100%／150%／200%／300% 實機畫面驗收。這份結果只代表 Python 靜態與回歸檢查通過。

完整逐項輸出見同目錄 `TEST_RUN_4.6.4.log`。
