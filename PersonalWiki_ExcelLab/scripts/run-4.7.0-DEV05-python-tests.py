#!/usr/bin/env python3
"""DEV05 static contract checks; intentionally not executed during development."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(f"FAIL: {message}")


def main() -> None:
    main_form = (ROOT / "PersonalWiki.Desktop/MainForm.cs").read_text(encoding="utf-8-sig")
    dialog = (ROOT / "PersonalWiki.Desktop/RelatedEntryPreviewDialog.cs").read_text(encoding="utf-8")

    require("檢視相關條目摘要" in main_form, "相關條目頁必須提供摘要檢視入口")
    require("_related.DoubleClick" in main_form, "雙擊相關條目必須可開啟摘要預覽")
    require("new RelatedEntryPreviewDialog(item)" in main_form, "摘要預覽入口必須使用獨立對話框")
    require("row.Tag = item" in main_form, "相關條目列必須保留唯讀預覽資料")
    require("ReadOnly = true" in dialog, "摘要與關聯依據必須唯讀")
    require("Clipboard.SetText(item.Title)" in dialog, "必須可複製條目名稱")
    require("RelatedEntryService" not in dialog, "預覽視窗不得自行執行資料查詢")
    require("EntryService" not in dialog, "預覽視窗不得建立或修改條目")

    for name in ("01_建置程式.bat", "02_啟動程式.bat", "03_建置並啟動.bat"):
        require((ROOT / name).is_file(), f"缺少標準 BAT：{name}")

    for rel in ("PersonalWiki.ExcelLab.csproj", "PersonalWiki.Desktop/PersonalWiki.Desktop.csproj"):
        content = (ROOT / rel).read_text(encoding="utf-8-sig")
        require("4.7.0-DEV05" in content, f"{rel} 版本未同步 DEV05")

    print("DEV05 static contract checks passed.")
    print("Note: this script was prepared for centralized testing and has not been executed.")


if __name__ == "__main__":
    main()
