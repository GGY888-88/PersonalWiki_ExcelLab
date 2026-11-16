using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ImportColumnPreview(int Index, string Name);
public sealed record ImportRowPreview(uint ExcelRow, IReadOnlyList<string> Values);
public sealed record ImportTablePreview(string Name, string Range);
public sealed record ImportSheetPreview(
    string Name,
    string State,
    uint HeaderRow,
    long DataRowCount,
    long BlankRowCount,
    int ColumnCount,
    IReadOnlyList<ImportColumnPreview> Columns,
    IReadOnlyList<ImportTablePreview> Tables,
    IReadOnlyList<ImportRowPreview> PreviewRows,
    string HeaderDetection);
public sealed record ImportWorkbookPreview(
    string File,
    string Extension,
    long FileSize,
    int PreviewRowLimit,
    IReadOnlyList<ImportSheetPreview> Sheets,
    IReadOnlyList<string> Notes);

public static class ImportPreviewService
{
    public static ImportWorkbookPreview Analyze(string filePath, int previewRows = 10)
    {
        WorkbookReader.EnsureSupported(filePath);
        if (previewRows is < 1 or > 200)
            throw new ArgumentOutOfRangeException(nameof(previewRows), "預覽列數必須介於 1 至 200。");

        using var document = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = document.WorkbookPart
            ?? throw new InvalidDataException("找不到 WorkbookPart。");
        var sharedStrings = WorkbookReader.ReadSharedStrings(workbookPart);
        var results = new List<ImportSheetPreview>();

        foreach (var sheet in WorkbookReader.GetSheets(workbookPart))
        {
            var worksheetPart = WorkbookReader.GetWorksheetPart(workbookPart, sheet);
            results.Add(AnalyzeSheet(
                worksheetPart,
                sheet.Name?.Value ?? "(未命名)",
                sheet.State?.Value.ToString() ?? "Visible",
                sharedStrings,
                previewRows));
        }

        return new ImportWorkbookPreview(
            Path.GetFullPath(filePath),
            Path.GetExtension(filePath).ToLowerInvariant(),
            new FileInfo(filePath).Length,
            previewRows,
            results,
            new[]
            {
                "此功能只讀取來源活頁簿，不會修改 Excel，也不會寫入資料庫。",
                "目前以工作表第一個非空白列作為欄位標題候選；正式匯入前仍可在後續欄位對應階段調整。",
                "預覽中的公式欄位顯示檔案內既有快取值；本工具不計算 Excel 公式。"
            });
    }

    private static ImportSheetPreview AnalyzeSheet(
        WorksheetPart worksheetPart,
        string sheetName,
        string state,
        IReadOnlyList<string> sharedStrings,
        int previewRows)
    {
        uint headerRow = 0;
        uint previousRowNumber = 0;
        long dataRowCount = 0;
        long blankRowCount = 0;
        var maxColumn = 0;
        var preview = new List<ImportRowPreview>();
        List<string>? headers = null;

        using var reader = OpenXmlReader.Create(worksheetPart);
        while (reader.Read())
        {
            if (reader.ElementType != typeof(Row))
                continue;

            var row = reader.LoadCurrentElement() as Row
                ?? throw new InvalidDataException("Open XML reader 回傳的資料列型別不正確。");
            var rowNumber = row.RowIndex?.Value ?? 0U;
            var valuesByColumn = new SortedDictionary<int, string>();

            foreach (var cell in row.Elements<Cell>())
            {
                var reference = cell.CellReference?.Value;
                var column = GetColumnIndex(reference);
                if (column <= 0)
                    continue;

                var value = WorkbookReader.GetCellText(cell, sharedStrings);
                if (!string.IsNullOrWhiteSpace(value) || cell.CellFormula is not null)
                    valuesByColumn[column] = value;
            }

            if (headerRow > 0 && rowNumber > previousRowNumber + 1 && previousRowNumber >= headerRow)
                blankRowCount += rowNumber - previousRowNumber - 1;

            if (valuesByColumn.Count == 0)
            {
                if (headerRow > 0 && rowNumber > headerRow)
                    blankRowCount++;
                previousRowNumber = Math.Max(previousRowNumber, rowNumber);
                continue;
            }

            maxColumn = Math.Max(maxColumn, valuesByColumn.Keys.Max());
            previousRowNumber = Math.Max(previousRowNumber, rowNumber);

            if (headerRow == 0)
            {
                headerRow = rowNumber;
                headers = BuildHeaders(valuesByColumn, maxColumn);
                continue;
            }

            dataRowCount++;
            if (preview.Count < previewRows)
                preview.Add(new ImportRowPreview(rowNumber, ExpandValues(valuesByColumn, maxColumn)));
        }

        // Count explicit blank Row elements and missing row-number gaps between used rows.
        headers ??= new List<string>();
        var columns = Enumerable.Range(1, maxColumn)
            .Select(i => new ImportColumnPreview(i, i <= headers.Count ? headers[i - 1] : $"欄位{i}"))
            .ToList();
        var normalizedPreview = preview
            .Select(row => new ImportRowPreview(
                row.ExcelRow,
                row.Values.Concat(Enumerable.Repeat(string.Empty, Math.Max(0, maxColumn - row.Values.Count)))
                    .Take(maxColumn).ToList()))
            .ToList();

        var tables = worksheetPart.TableDefinitionParts
            .Select(part => new ImportTablePreview(
                part.Table?.Name?.Value ?? part.Table?.DisplayName?.Value ?? "(未命名表格)",
                part.Table?.Reference?.Value ?? string.Empty))
            .ToList();

        return new ImportSheetPreview(
            sheetName,
            state,
            headerRow,
            dataRowCount,
            blankRowCount,
            maxColumn,
            columns,
            tables,
            normalizedPreview,
            headerRow == 0 ? "找不到非空白列，無法判斷標題" : $"使用第 {headerRow} 列作為標題候選");
    }

    private static List<string> BuildHeaders(SortedDictionary<int, string> values, int maxColumn)
    {
        var result = new List<string>(maxColumn);
        for (var i = 1; i <= maxColumn; i++)
        {
            var value = values.TryGetValue(i, out var found) ? found.Trim() : string.Empty;
            result.Add(string.IsNullOrWhiteSpace(value) ? $"欄位{i}" : value);
        }
        return result;
    }

    private static List<string> ExpandValues(SortedDictionary<int, string> values, int maxColumn)
    {
        var result = new List<string>(maxColumn);
        for (var i = 1; i <= maxColumn; i++)
            result.Add(values.TryGetValue(i, out var value) ? value : string.Empty);
        return result;
    }

    private static int GetColumnIndex(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
            return 0;

        var result = 0;
        foreach (var ch in cellReference)
        {
            if (!char.IsLetter(ch))
                break;
            result = result * 26 + char.ToUpperInvariant(ch) - 'A' + 1;
        }
        return result;
    }
}
