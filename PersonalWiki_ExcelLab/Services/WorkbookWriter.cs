using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PersonalWiki.ExcelLab.Services;

public static class WorkbookWriter
{
    /// <summary>
    /// 安全原型：只複製來源檔到新路徑，再修改副本內既有儲存格；不支援原地覆寫。
    /// </summary>
    public static string WriteTextToCopy(
        string sourcePath,
        string outputPath,
        string sheetName,
        string cellReference,
        string value)
    {
        WorkbookReader.EnsureSupported(sourcePath);
        if (File.Exists(outputPath))
            throw new IOException($"輸出檔已存在，為避免覆蓋而停止：{outputPath}");

        var fullOutput = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullOutput)!);
        File.Copy(sourcePath, fullOutput, overwrite: false);

        try
        {
            using var document = SpreadsheetDocument.Open(fullOutput, true);
            var workbookPart = document.WorkbookPart ?? throw new InvalidDataException("找不到 WorkbookPart。");
            var workbook = workbookPart.Workbook
                ?? throw new InvalidDataException("活頁簿內容不存在。");
            var sheet = workbook.Sheets?.Elements<Sheet>()
                .FirstOrDefault(s => string.Equals(s.Name?.Value, sheetName, StringComparison.Ordinal));
            if (sheet is null)
                throw new InvalidOperationException($"找不到工作表：{sheetName}");

            var worksheetPart = WorkbookReader.GetWorksheetPart(workbookPart, sheet);
            var worksheet = worksheetPart.Worksheet
                ?? throw new InvalidDataException($"工作表「{sheetName}」內容不存在。");
            var cell = worksheet.Descendants<Cell>()
                .FirstOrDefault(c => string.Equals(c.CellReference?.Value, cellReference, StringComparison.OrdinalIgnoreCase));
            if (cell is null)
                throw new InvalidOperationException($"找不到既有儲存格：{sheetName}!{cellReference}。原型不會自動新增儲存格。");

            // 以 InlineString 寫入純文字，避免將使用者輸入誤判為公式。
            cell.RemoveAllChildren();
            cell.DataType = CellValues.InlineString;
            cell.AppendChild(new InlineString(new Text(value)
            {
                Space = SpaceProcessingModeValues.Preserve
            }));
            worksheet.Save();
            workbookPart.Workbook.Save();
        }
        catch
        {
            // 保留失敗副本供檢查，不碰來源檔。
            throw;
        }

        return fullOutput;
    }
}
