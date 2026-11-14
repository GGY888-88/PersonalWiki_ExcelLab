# 4.6.0 發行稽核報告

## 變更範圍
僅強化 4.5.0 CLI acceptance runner 的錯誤隔離、部分報告持續保存、案例命令留存與指定案例重跑。既有查詢服務、Benchmark 服務、桌面 UI、資料庫 Schema 及資料安全流程不變。

## 稽核檢查
- [x] CLI／Desktop 版本同步為 4.6.0。
- [x] `--case-index` 僅允許 1–15。
- [x] 案例失敗會記錄錯誤並繼續下一案例。
- [x] 每個案例完成後即寫入 JSON／Markdown 報告。
- [x] 記錄搜尋與 Benchmark 實際命令及單案例重跑命令。
- [x] 報告區分案例失敗、設定錯誤與未執行案例。
- [x] 已存在輸出目錄仍拒絕覆寫。
- [x] 資料庫仍先複製至暫存位置，查詢唯讀。
- [x] Schema `user_version=1`、NuGet 版本與 DB／設定路徑未變。

## 測試限制
Python 靜態與模擬失敗測試可在跨平台環境執行。真實 .NET CLI、Windows WinForms、DPI 與剪貼簿仍待 Windows + .NET 10 SDK 實機驗收。不得將模擬失敗測試解讀為真實 CLI 成功。
