using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PersonalWiki.ExcelLab.Services;

public static class WorkbookFixtureBuilder
{
    public static string CreateLargeFixture(string outputPath, int dataRowCount)
    {
        const int maxDataRows = 1_048_575;
        if (dataRowCount is < 1 or > maxDataRows)
            throw new ArgumentOutOfRangeException(nameof(dataRowCount), $"資料列數必須介於 1 至 {maxDataRows}。" );

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        if (File.Exists(outputPath))
            throw new IOException($"檔案已存在，為避免覆蓋而停止：{outputPath}");

        using var document = SpreadsheetDocument.Create(outputPath, SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();
        var sheets = workbookPart.Workbook.AppendChild(new Sheets());
        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        sheets.Append(new Sheet
        {
            Id = workbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1U,
            Name = "大量資料"
        });

        // 使用 OpenXmlWriter 逐列串流輸出，避免一次將百萬筆資料放入記憶體。
        using (var writer = OpenXmlWriter.Create(worksheetPart))
        {
            writer.WriteStartElement(new Worksheet());
            writer.WriteStartElement(new SheetData());
            writer.WriteElement(CreateRow(1U,
                TextCell("A1", "名稱"), TextCell("B1", "介紹"), TextCell("C1", "序號")));

            for (var i = 1; i <= dataRowCount; i++)
            {
                var rowNumber = (uint)i + 1U;
                var suffix = i.ToString("D7", System.Globalization.CultureInfo.InvariantCulture);
                writer.WriteElement(CreateRow(rowNumber,
                    TextCell($"A{rowNumber}", $"測試人物{suffix}"),
                    TextCell($"B{rowNumber}", $"繁體中文全文搜尋樣本，分類第 {i % 100} 組，關鍵字百科測試"),
                    NumberCell($"C{rowNumber}", i)));
            }

            writer.WriteEndElement(); // SheetData
            writer.WriteEndElement(); // Worksheet
        }

        workbookPart.Workbook.Save();
        return outputPath;
    }

    public static string CreateBasicFixture(string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        if (File.Exists(outputPath))
            throw new IOException($"檔案已存在，為避免覆蓋而停止：{outputPath}");

        using var document = SpreadsheetDocument.Create(outputPath, SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();
        var sheets = workbookPart.Workbook.AppendChild(new Sheets());

        var visibleSheet = AddWorksheet(workbookPart, sheets, 1U, "基本資料", SheetStateValues.Visible);
        BuildMainSheet(visibleSheet);

        var hiddenSheet = AddWorksheet(workbookPart, sheets, 2U, "隱藏資料", SheetStateValues.Hidden);
        BuildSimpleSheet(hiddenSheet, "隱藏工作表內容", "隱藏資料仍應被索引");

        var veryHiddenSheet = AddWorksheet(workbookPart, sheets, 3U, "非常隱藏", SheetStateValues.VeryHidden);
        BuildSimpleSheet(veryHiddenSheet, "非常隱藏內容", "VeryHidden 工作表測試");

        workbookPart.Workbook.CalculationProperties = new CalculationProperties
        {
            CalculationMode = CalculateModeValues.Auto,
            ForceFullCalculation = true,
            FullCalculationOnLoad = true
        };

        workbookPart.Workbook.Save();
        return outputPath;
    }

    private static WorksheetPart AddWorksheet(
        WorkbookPart workbookPart,
        Sheets sheets,
        uint sheetId,
        string name,
        SheetStateValues state)
    {
        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());
        var sheet = new Sheet
        {
            Id = workbookPart.GetIdOfPart(worksheetPart),
            SheetId = sheetId,
            Name = name,
            State = state
        };
        sheets.Append(sheet);
        return worksheetPart;
    }

    private static void BuildMainSheet(WorksheetPart worksheetPart)
    {
        var columns = new Columns(
            new Column { Min = 1U, Max = 1U, Width = 22D, CustomWidth = true },
            new Column { Min = 2U, Max = 2U, Width = 34D, CustomWidth = true },
            new Column { Min = 3U, Max = 7U, Width = 18D, CustomWidth = true },
            new Column { Min = 8U, Max = 8U, Hidden = true, Width = 18D, CustomWidth = true });

        var worksheet = WorkbookReader.GetWorksheet(worksheetPart);
        var sheetData = worksheet.GetFirstChild<SheetData>()
            ?? throw new InvalidDataException("測試活頁簿缺少 SheetData 節點。");
        sheetData.Append(CreateRow(1U,
            TextCell("A1", "名稱"), TextCell("B1", "介紹"), TextCell("C1", "數字"),
            TextCell("D1", "計算結果"), TextCell("E1", "網址"), TextCell("F1", "備註"),
            TextCell("G1", "狀態"), TextCell("H1", "寫入測試")));
        sheetData.Append(CreateRow(2U,
            TextCell("A2", "測試人物A"), TextCell("B2", "繁體中文百科測試資料"), NumberCell("C2", 100),
            FormulaCell("D2", "C2*2", "200"), TextCell("E2", "https://example.com/person-a"),
            TextCell("F2", "保留公式及其他內容"), TextCell("G2", "一般"), TextCell("H2", "等待修改")));
        sheetData.Append(CreateRow(3U,
            TextCell("A3", "Test Person B"), TextCell("B3", "English test content"), NumberCell("C3", 200),
            FormulaCell("D3", "C3*2", "400"), TextCell("E3", "https://example.com/person-b"),
            TextCell("F3", "English note"), TextCell("G3", "重要"), TextCell("H3", "等待修改")));
        var hiddenRow = CreateRow(4U,
            TextCell("A4", "測試人物A"), TextCell("B4", "同名但不同資料列"), NumberCell("C4", 300),
            FormulaCell("D4", "C4*2", "600"), TextCell("E4", "https://example.com/person-a-2"),
            TextCell("F4", "此列設為隱藏列，仍應被讀取"), TextCell("G4", "一般"), TextCell("H4", "等待修改"));
        hiddenRow.Hidden = true;
        sheetData.Append(hiddenRow);
        sheetData.Append(CreateRow(8U, TextCell("A8", "合併儲存格測試")));

        worksheet.InsertBefore(columns, sheetData);

        var mergeCells = new MergeCells(new MergeCell { Reference = "A8:B8" });
        worksheet.Append(mergeCells);

        var validations = new DataValidations(
            new DataValidation
            {
                Type = DataValidationValues.List,
                AllowBlank = true,
                SequenceOfReferences = new ListValue<StringValue> { InnerText = "G2:G4" },
                Formula1 = new Formula1("\"一般,重要,待處理\"")
            }) { Count = 1U };
        worksheet.Append(validations);

        var hyperlinkElements = new Hyperlinks();
        foreach (var item in new[]
        {
            (Cell: "E2", Url: "https://example.com/person-a"),
            (Cell: "E3", Url: "https://example.com/person-b"),
            (Cell: "E4", Url: "https://example.com/person-a-2")
        })
        {
            var relationshipId = worksheetPart.AddHyperlinkRelationship(new Uri(item.Url), true).Id;
            hyperlinkElements.Append(new Hyperlink { Reference = item.Cell, Id = relationshipId });
        }
        worksheet.Append(hyperlinkElements);

        AddTable(worksheetPart, "A1:H4");
        worksheet.Save();
    }

    private static void AddTable(WorksheetPart worksheetPart, string reference)
    {
        var tablePart = worksheetPart.AddNewPart<TableDefinitionPart>();
        var table = new Table
        {
            Id = 1U,
            Name = "PeopleTable",
            DisplayName = "PeopleTable",
            Reference = reference,
            HeaderRowCount = 1U,
            TotalsRowShown = false
        };
        table.Append(new AutoFilter { Reference = reference });
        var columns = new TableColumns { Count = 8U };
        var names = new[] { "名稱", "介紹", "數字", "計算結果", "網址", "備註", "狀態", "寫入測試" };
        for (var i = 0; i < names.Length; i++)
            columns.Append(new TableColumn { Id = (uint)i + 1U, Name = names[i] });
        table.Append(columns);
        table.Append(new TableStyleInfo
        {
            Name = "TableStyleMedium2",
            ShowFirstColumn = false,
            ShowLastColumn = false,
            ShowRowStripes = true,
            ShowColumnStripes = false
        });
        tablePart.Table = table;
        tablePart.Table.Save();

        var tableParts = new TableParts { Count = 1U };
        tableParts.Append(new TablePart { Id = worksheetPart.GetIdOfPart(tablePart) });
        (worksheetPart.Worksheet ?? throw new InvalidDataException("測試活頁簿工作表內容不存在。"))
            .Append(tableParts);
    }

    private static void BuildSimpleSheet(WorksheetPart worksheetPart, string first, string second)
    {
        var worksheet = WorkbookReader.GetWorksheet(worksheetPart);
        var data = worksheet.GetFirstChild<SheetData>()
            ?? throw new InvalidDataException("測試活頁簿缺少 SheetData 節點。");
        data.Append(CreateRow(1U, TextCell("A1", "欄位"), TextCell("B1", "內容")));
        data.Append(CreateRow(2U, TextCell("A2", first), TextCell("B2", second)));
        worksheet.Save();
    }

    private static Row CreateRow(uint rowIndex, params Cell[] cells)
    {
        var row = new Row { RowIndex = rowIndex };
        foreach (var cell in cells)
            row.Append(cell);
        return row;
    }

    private static Cell TextCell(string reference, string value) => new()
    {
        CellReference = reference,
        DataType = CellValues.InlineString,
        InlineString = new InlineString(new Text(value) { Space = SpaceProcessingModeValues.Preserve })
    };

    private static Cell NumberCell(string reference, int value) => new()
    {
        CellReference = reference,
        DataType = CellValues.Number,
        CellValue = new CellValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture))
    };

    private static Cell FormulaCell(string reference, string formula, string cachedValue) => new()
    {
        CellReference = reference,
        CellFormula = new CellFormula(formula),
        CellValue = new CellValue(cachedValue)
    };
}
