using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ImportCleaningSheetReport(
    string SheetName,
    long RowsScanned,
    long MissingTitleRows,
    long DuplicateTitleRows,
    long TitleWhitespaceRows,
    long CellsWithEdgeWhitespace,
    long CellsWithControlCharacters,
    long CellsWithErrorValues,
    IReadOnlyList<uint> MissingTitleRowSamples,
    IReadOnlyList<uint> DuplicateTitleRowSamples,
    IReadOnlyList<uint> WhitespaceRowSamples,
    IReadOnlyList<uint> ErrorValueRowSamples,
    IReadOnlyList<string> Notes);

public sealed record ImportCleaningReport(
    string SourceFile,
    string MappingFile,
    DateTimeOffset CreatedAtUtc,
    bool MappingValid,
    IReadOnlyList<ImportMappingIssue> MappingIssues,
    IReadOnlyList<ImportCleaningSheetReport> Sheets,
    IReadOnlyList<string> Notes);

public static class ImportCleaningService
{
    private static readonly HashSet<string> ExcelErrors = new(StringComparer.OrdinalIgnoreCase)
    {
        "#REF!", "#DIV/0!", "#VALUE!", "#NAME?", "#N/A", "#NUM!", "#NULL!"
    };

    public static ImportCleaningReport Analyze(string workbookPath, string mappingPath)
    {
        var mappingCheck = ImportMappingService.Validate(workbookPath, mappingPath);
        if (!mappingCheck.IsValid)
            throw new InvalidDataException("欄位對應設定含有錯誤；請先執行 mapping-check 並修正 Error 項目。");

        var profile = JsonSerializer.Deserialize<ImportMappingProfile>(File.ReadAllText(mappingPath))
            ?? throw new InvalidDataException("欄位對應 JSON 無法解析。");

        WorkbookReader.EnsureSupported(workbookPath);
        using var document = SpreadsheetDocument.Open(workbookPath, false);
        var workbookPart = document.WorkbookPart
            ?? throw new InvalidDataException("找不到 WorkbookPart。");
        var sharedStrings = WorkbookReader.ReadSharedStrings(workbookPart);
        var sheetsByName = WorkbookReader.GetSheets(workbookPart)
            .ToDictionary(sheet => sheet.Name?.Value ?? "(未命名)", StringComparer.Ordinal)
            ?? new Dictionary<string, Sheet>(StringComparer.Ordinal);

        var results = new List<ImportCleaningSheetReport>();
        foreach (var mappingSheet in profile.Sheets)
        {
            if (!sheetsByName.TryGetValue(mappingSheet.SheetName, out var sheet))
                continue;

            var worksheetPart = WorkbookReader.GetWorksheetPart(workbookPart, sheet);
            results.Add(AnalyzeSheet(worksheetPart, mappingSheet, sharedStrings));
        }

        return new ImportCleaningReport(
            Path.GetFullPath(workbookPath),
            Path.GetFullPath(mappingPath),
            DateTimeOffset.UtcNow,
            mappingCheck.IsValid,
            mappingCheck.Issues,
            results,
            new[]
            {
                "本報告只檢查資料，不會自動修正或刪除任何內容。",
                "空白標題與重複標題只針對欄位對應中 Mode=Title 的欄位。",
                "空白字元檢查包含前後空白；控制字元檢查不包含 Tab、CR、LF。",
                "錯誤值依儲存格文字判斷；不會重新計算 Excel 公式。",
                "每類問題最多列出 20 個 Excel 列號樣本。"
            });
    }

    private static ImportCleaningSheetReport AnalyzeSheet(
        WorksheetPart worksheetPart,
        ImportMappingSheet mapping,
        IReadOnlyList<string> sharedStrings)
    {
        var titleColumns = mapping.Columns
            .Where(column => column.Mode.Equals("Title", StringComparison.OrdinalIgnoreCase))
            .Select(column => column.SourceIndex)
            .ToArray();
        if (titleColumns.Length != 1)
            throw new InvalidDataException($"工作表「{mapping.SheetName}」必須且只能指定一個 Title 欄位。");
        var titleColumn = titleColumns[0];
        var mappedColumns = mapping.Columns
            .Where(column => !column.Mode.Equals("Unmapped", StringComparison.OrdinalIgnoreCase)
                          && !column.Mode.Equals("Ignore", StringComparison.OrdinalIgnoreCase))
            .Select(column => column.SourceIndex)
            .ToHashSet();

        long rowsScanned = 0, missingTitles = 0, duplicateTitles = 0, titleWhitespace = 0;
        long edgeWhitespace = 0, controlCharacters = 0, errorValues = 0;
        var seenTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var missingSamples = new List<uint>();
        var duplicateSamples = new List<uint>();
        var whitespaceSamples = new List<uint>();
        var errorSamples = new List<uint>();

        using var reader = OpenXmlReader.Create(worksheetPart);
        while (reader.Read())
        {
            if (reader.ElementType != typeof(Row))
                continue;

            var row = reader.LoadCurrentElement() as Row
                ?? throw new InvalidDataException("Open XML reader 回傳的資料列型別不正確。");
            var rowNumber = row.RowIndex?.Value ?? 0U;
            if (rowNumber < mapping.FirstDataRow)
                continue;

            var values = new Dictionary<int, string>();
            foreach (var cell in row.Elements<Cell>())
            {
                var columnIndex = GetColumnIndex(cell.CellReference?.Value);
                if (columnIndex <= 0)
                    continue;
                values[columnIndex] = WorkbookReader.GetCellText(cell, sharedStrings);
            }

            if (values.Count == 0 || values.Values.All(string.IsNullOrWhiteSpace))
                continue;

            rowsScanned++;
            var title = values.TryGetValue(titleColumn, out var titleValue) ? titleValue : string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                missingTitles++;
                AddSample(missingSamples, rowNumber);
            }
            else
            {
                var normalized = title.Trim();
                if (!seenTitles.Add(normalized))
                {
                    duplicateTitles++;
                    AddSample(duplicateSamples, rowNumber);
                }
                if (!string.Equals(title, normalized, StringComparison.Ordinal))
                {
                    titleWhitespace++;
                    AddSample(whitespaceSamples, rowNumber);
                }
            }

            var rowHasWhitespace = false;
            var rowHasError = false;
            foreach (var pair in values)
            {
                if (!mappedColumns.Contains(pair.Key))
                    continue;

                var value = pair.Value;
                if (!string.IsNullOrEmpty(value) && !string.Equals(value, value.Trim(), StringComparison.Ordinal))
                {
                    edgeWhitespace++;
                    rowHasWhitespace = true;
                }

                if (value.Any(ch => char.IsControl(ch) && ch is not '\t' and not '\r' and not '\n'))
                    controlCharacters++;

                if (ExcelErrors.Contains(value) || value.StartsWith("#ERROR:", StringComparison.OrdinalIgnoreCase))
                {
                    errorValues++;
                    rowHasError = true;
                }
            }

            if (rowHasWhitespace)
                AddSample(whitespaceSamples, rowNumber);
            if (rowHasError)
                AddSample(errorSamples, rowNumber);
        }

        return new ImportCleaningSheetReport(
            mapping.SheetName,
            rowsScanned,
            missingTitles,
            duplicateTitles,
            titleWhitespace,
            edgeWhitespace,
            controlCharacters,
            errorValues,
            missingSamples,
            duplicateSamples,
            whitespaceSamples,
            errorSamples,
            new[]
            {
                "重複標題以 Trim 後、不區分大小寫比較；不會合併資料。",
                "空白資料列不計入 RowsScanned。",
                "同一列可能同時符合多種問題。"
            });
    }

    private static void AddSample(List<uint> samples, uint rowNumber)
    {
        if (samples.Count < 20)
            samples.Add(rowNumber);
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
