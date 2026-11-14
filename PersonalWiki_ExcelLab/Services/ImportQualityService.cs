using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ImportColumnQuality(
    int Index,
    string Name,
    long NonEmptyCount,
    long BlankCount,
    long FormulaCount,
    long ErrorCount,
    IReadOnlyList<string> Samples,
    IReadOnlyList<string> Warnings);

public sealed record ImportSheetQuality(
    string Name,
    string State,
    uint HeaderRow,
    long DataRowCount,
    long BlankRowCount,
    IReadOnlyList<ImportColumnQuality> Columns,
    IReadOnlyList<string> Warnings);

public sealed record ImportQualityReport(
    string File,
    long FileSize,
    IReadOnlyList<ImportSheetQuality> Sheets,
    IReadOnlyList<string> Notes);

public static class ImportQualityService
{
    private sealed class ColumnStats
    {
        public long NonEmpty;
        public long Formula;
        public long Errors;
        public HashSet<string> Samples { get; } = new(StringComparer.Ordinal);
    }

    public static ImportQualityReport Analyze(string filePath)
    {
        WorkbookReader.EnsureSupported(filePath);
        using var document = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = document.WorkbookPart
            ?? throw new InvalidDataException("找不到 WorkbookPart。");
        var sharedStrings = WorkbookReader.ReadSharedStrings(workbookPart);
        var sheets = new List<ImportSheetQuality>();

        foreach (var sheet in WorkbookReader.GetSheets(workbookPart))
        {
            var worksheetPart = WorkbookReader.GetWorksheetPart(workbookPart, sheet);
            sheets.Add(AnalyzeSheet(
                worksheetPart,
                sheet.Name?.Value ?? "(未命名)",
                sheet.State?.Value.ToString() ?? "Visible",
                sharedStrings));
        }

        return new ImportQualityReport(
            Path.GetFullPath(filePath),
            new FileInfo(filePath).Length,
            sheets,
            new[]
            {
                "本報告只做唯讀統計，不會清理或修改來源 Excel。",
                "空白統計以第一個非空白列作為標題候選列，其後非空白資料列為基準。",
                "樣本最多列出每欄 3 個不同值；不會保存整欄內容。",
                "公式錯誤只依檔案內可讀取的快取文字判斷，本工具不會重新計算公式。"
            });
    }

    private static ImportSheetQuality AnalyzeSheet(
        WorksheetPart worksheetPart,
        string sheetName,
        string state,
        IReadOnlyList<string> sharedStrings)
    {
        uint headerRow = 0;
        uint previousRow = 0;
        long dataRows = 0;
        long blankRows = 0;
        var maxColumn = 0;
        var headers = new Dictionary<int, string>();
        var stats = new Dictionary<int, ColumnStats>();

        using var reader = OpenXmlReader.Create(worksheetPart);
        while (reader.Read())
        {
            if (reader.ElementType != typeof(Row))
                continue;

            var row = reader.LoadCurrentElement() as Row
                ?? throw new InvalidDataException("Open XML reader 回傳的資料列型別不正確。");
            var rowNumber = row.RowIndex?.Value ?? 0U;
            var values = new Dictionary<int, (string Value, bool Formula)>();

            foreach (var cell in row.Elements<Cell>())
            {
                var column = GetColumnIndex(cell.CellReference?.Value);
                if (column <= 0)
                    continue;
                var value = WorkbookReader.GetCellText(cell, sharedStrings);
                var hasFormula = cell.CellFormula is not null;
                if (!string.IsNullOrWhiteSpace(value) || hasFormula)
                    values[column] = (value, hasFormula);
            }

            if (headerRow > 0 && previousRow > 0 && rowNumber > previousRow + 1)
                blankRows += rowNumber - previousRow - 1;

            if (values.Count == 0)
            {
                if (headerRow > 0 && rowNumber > headerRow)
                    blankRows++;
                previousRow = Math.Max(previousRow, rowNumber);
                continue;
            }

            maxColumn = Math.Max(maxColumn, values.Keys.Max());
            previousRow = Math.Max(previousRow, rowNumber);

            if (headerRow == 0)
            {
                headerRow = rowNumber;
                for (var i = 1; i <= maxColumn; i++)
                {
                    var name = values.TryGetValue(i, out var item) ? item.Value.Trim() : string.Empty;
                    headers[i] = string.IsNullOrWhiteSpace(name) ? $"欄位{i}" : name;
                }
                continue;
            }

            dataRows++;
            foreach (var pair in values)
            {
                if (!stats.TryGetValue(pair.Key, out var columnStats))
                    stats[pair.Key] = columnStats = new ColumnStats();

                columnStats.NonEmpty++;
                if (pair.Value.Formula)
                    columnStats.Formula++;
                if (pair.Value.Value.StartsWith("#", StringComparison.Ordinal))
                    columnStats.Errors++;
                if (!string.IsNullOrWhiteSpace(pair.Value.Value) && columnStats.Samples.Count < 3)
                    columnStats.Samples.Add(pair.Value.Value.Length <= 80
                        ? pair.Value.Value
                        : pair.Value.Value[..80]);
            }
        }

        var duplicateNames = headers
            .GroupBy(pair => pair.Value, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var columns = Enumerable.Range(1, maxColumn).Select(index =>
        {
            stats.TryGetValue(index, out var columnStats);
            var name = headers.TryGetValue(index, out var header) ? header : $"欄位{index}";
            var warnings = new List<string>();
            if (name == $"欄位{index}")
                warnings.Add("標題空白或缺少，使用暫定欄位名稱");
            if (duplicateNames.Contains(name))
                warnings.Add("欄位名稱重複");
            if ((columnStats?.Errors ?? 0) > 0)
                warnings.Add("含有疑似 Excel 錯誤值");
            return new ImportColumnQuality(
                index,
                name,
                columnStats?.NonEmpty ?? 0,
                Math.Max(0, dataRows - (columnStats?.NonEmpty ?? 0)),
                columnStats?.Formula ?? 0,
                columnStats?.Errors ?? 0,
                columnStats?.Samples.ToList() ?? new List<string>(),
                warnings);
        }).ToList();

        var sheetWarnings = new List<string>();
        if (headerRow == 0)
            sheetWarnings.Add("找不到非空白列，無法判斷欄位標題");
        if (duplicateNames.Count > 0)
            sheetWarnings.Add("存在重複欄位名稱");
        if (columns.Any(column => column.Warnings.Contains("含有疑似 Excel 錯誤值")))
            sheetWarnings.Add("至少一個欄位含有疑似 Excel 錯誤值");

        return new ImportSheetQuality(
            sheetName, state, headerRow, dataRows, blankRows, columns, sheetWarnings);
    }

    private static int GetColumnIndex(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return 0;
        var result = 0;
        foreach (var ch in reference)
        {
            if (!char.IsLetter(ch))
                break;
            result = result * 26 + char.ToUpperInvariant(ch) - 'A' + 1;
        }
        return result;
    }
}
