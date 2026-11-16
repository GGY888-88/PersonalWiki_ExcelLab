# PersonalWiki ExcelLab 3.6.0 / 3.7.0 測試素材

所有素材均為合成測試資料，不含使用者正式資料。

## 測試資料庫
- `PersonalWiki_3.6.0_Test.db`：SQLite Schema v1，9 筆測試條目、17 筆標籤關聯、筆記、自訂欄位、來源追溯及 2 筆圖片關聯。
- `PersonalWiki_3.6.0_Test_Image.png`：有效圖片樣本。
- 其中一筆圖片關聯指向不存在的檔案，用於驗證缺圖狀態；移除關聯不應刪除原始圖片。

## Excel 匯入設定
- `PersonalWiki_Import_Sample.xlsx` 搭配同名 `.mapping.json`、`.rules.json`：單一工作表測試。
- `PersonalWiki_MultiLayout_Test.xlsx` 搭配同名 `.mapping.json`、`.rules.json`：多工作表測試。
- 所有規則均為 `Review`，只產生人工檢查項目，不會自動修改、合併或刪除來源資料。

## 測試指令
在專案根目錄執行：

```powershell
python scripts/run-3.7.0-python-tests.py
```

Python 測試驗證 SQLite 查詢語意、測試素材、靜態程式碼契約及發行文件；它們不等於 .NET 編譯、Microsoft.Data.Sqlite 執行期或 Windows 桌面驗收。
