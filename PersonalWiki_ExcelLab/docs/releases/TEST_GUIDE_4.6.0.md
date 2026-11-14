# 4.6.0 測試指南

## A. 跨平台 Python／靜態與歷史回歸

在專案根目錄執行：

```powershell
python -m py_compile scripts/entry-query-acceptance.py scripts/test-4.6.0-query-acceptance.py scripts/run-4.6.0-python-tests.py scripts/verify-release-4.6.0.py
python scripts/entry-query-acceptance.py --self-test
python scripts/test-4.6.0-query-acceptance.py
python scripts/run-4.6.0-python-tests.py
```

## B. Windows + .NET 10 真實 CLI 驗收

```powershell
python scripts/entry-query-acceptance.py --database samples/PersonalWiki_3.6.0_Test.db
```

指定全新輸出資料夾（不得已存在）：

```powershell
python scripts/entry-query-acceptance.py --database samples/PersonalWiki_3.6.0_Test.db --output Acceptance_4.6.0_Run1
```

只重跑第 7 組案例：

```powershell
python scripts/entry-query-acceptance.py --database samples/PersonalWiki_3.6.0_Test.db --case-index 7 --output Acceptance_4.6.0_Case7_Rerun
```

若已完成 Release build，可加 `--no-build`。報告中的 `rerunCommand` 會提供對應失敗案例的可複製命令；執行前請確認命令內輸出資料夾尚不存在。

## 報告內容
- `acceptance-comparison.json`：案例、比對欄位、CLI 命令、錯誤、Benchmark 數值及重跑命令。
- `acceptance-comparison.md`：摘要、失敗診斷與逐案例重跑命令。
- 每組成功執行的原始搜尋 JSON 及 Benchmark JSON。
- 每完成一組案例即更新報告；環境初始化失敗時也會保留設定錯誤與未執行數量。

## 判定與安全
- 任一 parity 案例失敗時結束碼為 2；整體設定／建置失敗為 1；全部通過為 0。
- 每個案例分開記錄錯誤，不因單一案例失敗跳過其餘案例。
- 原始資料庫只讀取並複製到暫存資料夾，查詢只對複本執行。
- 輸出目錄已存在時拒絕執行，不覆寫舊報告。
- Python 模擬測試不代表真實 .NET CLI 或 Windows UI 已驗收。

## 尚待實機驗收
- .NET 10 restore/build 與 15 組真實 CLI parity。
- Windows WinForms、DPI、剪貼簿與長時間操作。
