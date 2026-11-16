using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;

namespace PersonalWiki.ExcelLab.Services;

public static class WorkbookValidator
{
    public static object Validate(string filePath, int maxErrors = 100)
    {
        WorkbookReader.EnsureSupported(filePath);
        if (maxErrors < 1 || maxErrors > 1000)
            throw new ArgumentOutOfRangeException(nameof(maxErrors), "maxErrors 必須介於 1 至 1000。");

        using var document = SpreadsheetDocument.Open(filePath, false);
        var validator = new OpenXmlValidator { MaxNumberOfErrors = maxErrors };
        var errors = validator.Validate(document)
            .Select(error => new
            {
                error.Id,
                error.Description,
                Part = error.Part?.Uri.ToString()
            })
            .ToList();

        return new
        {
            File = Path.GetFullPath(filePath),
            ValidationTarget = validator.FileFormat.ToString(),
            ErrorCountReturned = errors.Count,
            MaxErrors = maxErrors,
            HasErrors = errors.Count > 0,
            Errors = errors,
            Note = "OpenXmlValidator 執行 Open XML 結構/規格驗證；通過不代表 Excel 商業邏輯、公式計算或所有 Office 擴充功能都已驗證。"
        };
    }
}
