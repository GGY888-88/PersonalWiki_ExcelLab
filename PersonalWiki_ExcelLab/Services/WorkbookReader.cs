using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PersonalWiki.ExcelLab.Services;

public static class WorkbookReader
{
    public static object Inspect(string filePath)
    {
        EnsureSupported(filePath);
        using var document = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = document.WorkbookPart ?? throw new InvalidDataException("找不到 WorkbookPart。");
        var sheets = new List<object>();

        foreach (var sheet in GetSheets(workbookPart))
        {
            var worksheetPart = GetWorksheetPart(workbookPart, sheet);
            var worksheet = GetWorksheet(worksheetPart);
            var rows = worksheet.Descendants<Row>().ToList();
            var cells = worksheet.Descendants<Cell>().ToList();
            var formulaCount = cells.Count(c => c.CellFormula is not null);
            var hyperlinks = worksheet.GetFirstChild<Hyperlinks>()?.Elements<Hyperlink>().Count() ?? 0;
            var merges = worksheet.GetFirstChild<MergeCells>()?.Elements<MergeCell>().Count() ?? 0;
            var validations = worksheet.GetFirstChild<DataValidations>()?.Elements<DataValidation>().Count() ?? 0;
            var tables = worksheetPart.TableDefinitionParts.Count();

            sheets.Add(new
            {
                Name = sheet.Name?.Value ?? "(未命名)",
                State = sheet.State?.Value.ToString() ?? "Visible",
                RowCount = rows.Count,
                CellCount = cells.Count,
                FormulaCount = formulaCount,
                MergedRangeCount = merges,
                DataValidationCount = validations,
                TableCount = tables,
                HyperlinkCount = hyperlinks,
                HiddenRowCount = rows.Count(r => r.Hidden?.Value == true),
                HiddenCellColumnCount = cells.Count(c => IsHiddenColumn(worksheet, c.CellReference?.Value))
            });
        }

        return new
        {
            File = Path.GetFullPath(filePath),
            Extension = Path.GetExtension(filePath).ToLowerInvariant(),
            FileSize = new FileInfo(filePath).Length,
            SheetCount = sheets.Count,
            Sheets = sheets,
            Notes = new[]
            {
                "公式欄位同時顯示公式文字及快取結果；本工具不負責計算公式。",
                "此為測試原型，尚未驗證 VBA、Power Query、外部連線及所有特殊 Excel 結構。"
            }
        };
    }

    public static IEnumerable<IndexedRow> ReadRows(string filePath)
    {
        EnsureSupported(filePath);
        using var document = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = document.WorkbookPart ?? throw new InvalidDataException("找不到 WorkbookPart。");
        var sharedStrings = ReadSharedStrings(workbookPart);

        foreach (var sheet in GetSheets(workbookPart))
        {
            var sheetName = sheet.Name?.Value ?? "(未命名)";
            var worksheetPart = GetWorksheetPart(workbookPart, sheet);
            using var reader = OpenXmlReader.Create(worksheetPart);
            while (reader.Read())
            {
                if (reader.ElementType != typeof(Row))
                    continue;

                var row = reader.LoadCurrentElement() as Row
                    ?? throw new InvalidDataException("Open XML reader 回傳的資料列型別不正確。");
                var rowNumber = row.RowIndex?.Value ?? 0U;
                // 規格第一列為欄位標題，不當成百科資料列索引。
                if (rowNumber <= 1U)
                    continue;
                var parts = new List<string>();
                string? name = null;
                foreach (var cell in row.Elements<Cell>())
                {
                    var value = GetCellText(cell, sharedStrings);
                    if (string.IsNullOrWhiteSpace(value) && cell.CellFormula is null)
                        continue;

                    var reference = cell.CellReference?.Value ?? string.Empty;
                    if (name is null && !string.IsNullOrWhiteSpace(value))
                        name = value;

                    parts.Add($"{reference} {value}".Trim());
                    if (cell.CellFormula is not null)
                        parts.Add($"公式 {cell.CellFormula.Text}");
                }

                if (parts.Count == 0)
                    continue;

                yield return new IndexedRow(
                    Path.GetFullPath(filePath),
                    sheetName,
                    rowNumber,
                    name ?? string.Empty,
                    string.Join(" | ", parts));
            }
        }
    }

    public static IEnumerable<Sheet> GetSheets(WorkbookPart workbookPart)
    {
        var workbook = workbookPart.Workbook
            ?? throw new InvalidDataException("活頁簿內容不存在。");
        return workbook.Sheets?.Elements<Sheet>() ?? Enumerable.Empty<Sheet>();
    }

    public static Worksheet GetWorksheet(WorksheetPart worksheetPart) => worksheetPart.Worksheet
        ?? throw new InvalidDataException("工作表內容不存在。");

    public static WorksheetPart GetWorksheetPart(WorkbookPart workbookPart, Sheet sheet)
    {
        var relationshipId = sheet.Id?.Value;
        if (string.IsNullOrWhiteSpace(relationshipId))
            throw new InvalidDataException($"工作表「{sheet.Name?.Value ?? "(未命名)"}」缺少有效的關聯 ID。");

        return workbookPart.GetPartById(relationshipId) as WorksheetPart
            ?? throw new InvalidDataException($"工作表「{sheet.Name?.Value ?? "(未命名)"}」的關聯不是有效的 WorksheetPart。");
    }

    public static string GetCellText(Cell cell, IReadOnlyList<string> sharedStrings)
    {
        if (cell.DataType?.Value == CellValues.InlineString)
            return cell.InlineString?.InnerText ?? string.Empty;

        var raw = cell.CellValue?.Text ?? string.Empty;
        if (cell.DataType?.Value == CellValues.SharedString && int.TryParse(raw, out var index))
            return index >= 0 && index < sharedStrings.Count ? sharedStrings[index] : string.Empty;
        if (cell.DataType?.Value == CellValues.Boolean)
            return raw == "1" ? "TRUE" : "FALSE";
        if (cell.DataType?.Value == CellValues.Error)
            return $"#ERROR:{raw}";
        return raw;
    }

    public static IReadOnlyList<string> ReadSharedStrings(WorkbookPart workbookPart)
    {
        var table = workbookPart.SharedStringTablePart?.SharedStringTable;
        if (table is null)
            return Array.Empty<string>();
        return table.Elements<SharedStringItem>().Select(item => item.InnerText).ToList();
    }

    private static bool IsHiddenColumn(Worksheet worksheet, string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
            return false;
        var columnIndex = GetColumnIndex(cellReference);
        return worksheet.GetFirstChild<Columns>()?.Elements<Column>().Any(c =>
            c.Hidden?.Value == true &&
            (c.Min?.Value ?? 0U) <= columnIndex &&
            (c.Max?.Value ?? 0U) >= columnIndex) == true;
    }

    private static uint GetColumnIndex(string cellReference)
    {
        uint result = 0;
        foreach (var ch in cellReference)
        {
            if (!char.IsLetter(ch))
                break;
            result = result * 26 + (uint)(char.ToUpperInvariant(ch) - 'A' + 1);
        }
        return result;
    }

    public static void EnsureSupported(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("找不到 Excel 檔案。", filePath);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension is not ".xlsx" and not ".xlsm")
            throw new NotSupportedException($"目前原型只處理 XLSX/XLSM；不支援：{extension}");
    }
}
