# 1.2.8 更新紀錄

- 修正 300% DPI 下新增／編輯條目對話框「確定儲存／取消」按鈕文字上下裁切。
- 按鈕改用內容自動量測（AutoSize），明確指定中文字型與相容文字繪製，並依 DPI 提供更大的最小尺寸及內距。
- 增加底部按鈕列高度，避免按鈕被容器裁切。
- 保留 1.2.7 已修正的編輯名稱初始顯示位置。
- 診斷檔更新為 `%LOCALAPPDATA%\PersonalWiki\diagnostics\ui-1.2.8.txt`。
- SQLite Schema 維持 v1；沒有新增 NuGet 相依套件。
- 尚未在 Windows 300% DPI 實機執行介面驗收。
