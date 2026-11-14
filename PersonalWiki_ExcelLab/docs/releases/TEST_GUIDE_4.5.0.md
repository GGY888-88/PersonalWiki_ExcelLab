# 4.5.0 測試指南

## A. 不需 .NET 的本機檢查

```powershell
python -m py_compile scripts/entry-query-acceptance.py scripts/test-4.5.0-query-acceptance.py scripts/run-4.5.0-python-tests.py
python scripts/entry-query-acceptance.py --self-test
python scripts/test-4.5.0-query-acceptance.py
```

## B. 真實 .NET CLI 驗收（Windows／已安裝 .NET 10 SDK）

在專案根目錄執行，資料庫使用隨附隔離測試資料：

```powershell
python scripts/entry-query-acceptance.py --database samples/PersonalWiki_3.6.0_Test.db
```

若要指定報告資料夾，該資料夾必須尚不存在：

```powershell
python scripts/entry-query-acceptance.py --database samples/PersonalWiki_3.6.0_Test.db --output Acceptance_4.5.0_Run1
```

流程會執行 .NET restore/build，再對資料庫位元組複本執行 15 組搜尋與 Benchmark 比對。報告包括：
- `acceptance-comparison.json`
- `acceptance-comparison.md`
- 每組搜尋 JSON 與 Benchmark JSON

`--no-build` 僅在已存在 `bin/Release/net10.0/PersonalWiki.ExcelLab.dll` 時使用。

## 驗收判定
- 每組 `entry-query` 與 `entry-query-benchmark` 的總筆數、本頁筆數、ID 與順序、頁碼、頁大小、排序、封存及篩選條件必須完全一致。
- Benchmark 執行 3 次，耗時只作觀察，不設跨設備固定門檻。
- 任一比對失敗時腳本以非零結束碼結束，仍保留已產生的診斷報告。
- 驗收不會對使用者原始資料庫寫入；測試資料庫副本位於暫存目錄。

## 尚未執行
目前開發容器沒有 .NET SDK／Windows，因此 B 項真實 CLI、WinForms、DPI、剪貼簿及 Windows 實機操作仍需在 Windows + .NET 10 環境驗收。
