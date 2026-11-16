# 4.7.0 DEV04 集中測試指南（尚未執行）

## 測試前準備
- Windows 10／11、.NET 10 SDK。
- 使用 DEV04 ZIP 解壓後的 `PersonalWiki_ExcelLab` 資料夾。
- 使用既有素材：
  - `samples/PersonalWiki_Import_Sample.xlsx`
  - `samples/PersonalWiki_Import_Sample.mapping.json`
  - `samples/PersonalWiki_Import_Sample.rules.json`
  - `samples/PersonalWiki_MultiLayout_Test.xlsx`
  - `samples/PersonalWiki_MultiLayout_Test.mapping.json`
  - `samples/PersonalWiki_MultiLayout_Test.rules.json`
- 請先複製測試素材到測試資料夾；不要使用正式知識庫或正式 Excel。

## 驗收項目
1. 啟動程式，開啟測試知識庫，進入「Excel 匯入精靈」。
2. 使用 `PersonalWiki_Import_Sample` 素材完成前段預覽、欄位用途及規則設定。
3. 確認 Dry-run 有問題時，出現「Excel 匯入檢查問題明細（唯讀）」視窗。
4. 確認清單依工作表及檢查項目分列，符合列數及 Excel 列號樣本正確顯示。
5. 選取不同問題項目，確認下方明細同步切換。
6. 點擊「複製 Excel 列號」，貼到記事本確認每個列號各自一行。
7. 確認超過 20 筆的規則顯示未列入樣本的剩餘數量，不誤稱已列出全部列號。
8. 點擊「重新調整檢查項目」，確認返回既有規則設定視窗；修改後再次 Dry-run，明細隨結果更新。
9. 點擊「取消本次匯入」，確認流程停止，不建立匯入前備份、不寫入資料庫。
10. 使用多工作表素材，確認工作表切換及多筆問題導覽正常。
11. 在 100%、150%、200%、300% DPI 檢查視窗、欄位、按鈕及工作區邊界。
12. 確認既有匯入歷史、批次差異比較、搜尋、標籤、圖片、備份／還原功能無回歸。
13. 若確認無問題，再依既有流程測試正式匯入：備份先建立並驗證、來源 Excel 不被修改、資料庫使用既有單一交易流程。

## 安全驗收
- 視窗不得寫入 SQLite 或修改 Excel。
- 視窗不得顯示或保存儲存格原文。
- 不得自動忽略問題列或自動修正來源資料。
- 取消本次匯入不得進入備份與正式匯入步驟。
- SQLite `PRAGMA user_version` 必須維持 1。

## 執行狀態
所有項目目前均為「待執行」。此文件是測試操作指南，不代表測試已通過。
