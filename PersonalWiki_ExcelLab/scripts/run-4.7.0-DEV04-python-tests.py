#!/usr/bin/env python3
"""DEV04 static contract checks; intentionally not executed during development."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(f"FAIL: {message}")


def main() -> None:
    dialog = (ROOT / "PersonalWiki.Desktop/ExcelImportDryRunDialog.cs").read_text(encoding="utf-8")
    main_form = (ROOT / "PersonalWiki.Desktop/MainForm.cs").read_text(encoding="utf-8-sig")
    dry_run = (ROOT / "Services/ImportDryRunService.cs").read_text(encoding="utf-8")
    workflow = (ROOT / "Services/ExcelImportWorkflowService.cs").read_text(encoding="utf-8")

    require("ImportDryRunReport" in dialog, "問題檢視視窗必須接收既有 Dry-run 報告")
    require("ListView" in dialog and "SelectedIndexChanged" in dialog, "問題清單必須可選取導覽")
    require("Clipboard.SetText" in dialog, "必須提供複製 Excel 列號功能")
    require("重新調整檢查項目" in dialog and "取消本次匯入" in dialog, "必須保留返回設定及取消匯入操作")
    require("new ExcelImportDryRunDialog(dryRun)" in main_form, "匯入精靈必須使用問題明細視窗")
    require("ImportDryRunService.Analyze" in main_form, "匯入精靈必須保留既有 Dry-run 分析")
    require("ImportDryRunReport" in dry_run and "RowSamples" in dry_run, "既有 Dry-run 報告契約必須保留")
    require("Backup" in workflow or "backup" in workflow.lower(), "既有匯入工作流程需保留備份處理")

    for name in ("01_建置程式.bat", "02_啟動程式.bat", "03_建置並啟動.bat"):
        require((ROOT / name).is_file(), f"缺少標準 BAT：{name}")

    print("DEV04 static contract checks passed.")
    print("Note: these checks were prepared for the later centralized test phase.")


if __name__ == "__main__":
    main()
