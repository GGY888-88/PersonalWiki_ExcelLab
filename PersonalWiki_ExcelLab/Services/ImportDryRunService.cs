using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PersonalWiki.ExcelLab.Services;

public sealed record ImportDryRunRuleResult(string RuleId, long MatchedRows, IReadOnlyList<uint> RowSamples);
public sealed record ImportDryRunSheetResult(string SheetName, long CandidateRows, long RowsWithReview, IReadOnlyList<ImportDryRunRuleResult> Rules);
public sealed record ImportDryRunReport(string SourceFile, string MappingFile, string RulesFile, DateTimeOffset CreatedAtUtc, bool ConfigurationValid, long CandidateRows, long RowsRequiringReview, IReadOnlyList<ImportDryRunSheetResult> Sheets, IReadOnlyList<string> SafetyNotes);

public static class ImportDryRunService
{
    private static readonly HashSet<string> ExcelErrors = new(StringComparer.OrdinalIgnoreCase)
        { "#REF!", "#DIV/0!", "#VALUE!", "#NAME?", "#N/A", "#NUM!", "#NULL!" };

    public static ImportDryRunReport Analyze(string workbookPath, string mappingPath, string rulesPath)
    {
        var validation = ImportCleaningRulesService.Validate(workbookPath, mappingPath, rulesPath);
        if (!validation.IsValid)
            throw new InvalidDataException("mapping 或清理規則未通過驗證；Dry-run 已停止，請先修正設定。");

        var mapping = JsonSerializer.Deserialize<ImportMappingProfile>(File.ReadAllText(mappingPath))
            ?? throw new InvalidDataException("mapping JSON 無法解析。");
        var rules = JsonSerializer.Deserialize<ImportCleaningRulesProfile>(File.ReadAllText(rulesPath))
            ?? throw new InvalidDataException("清理規則 JSON 無法解析。");
        var rulesBySheet = rules.Sheets.ToDictionary(x => x.SheetName, StringComparer.Ordinal);
        WorkbookReader.EnsureSupported(workbookPath);
        using var doc = SpreadsheetDocument.Open(workbookPath, false);
        var wb = doc.WorkbookPart ?? throw new InvalidDataException("找不到 WorkbookPart。");
        var shared = WorkbookReader.ReadSharedStrings(wb);
        var sheetsByName = WorkbookReader.GetSheets(wb)
            .ToDictionary(x => x.Name?.Value ?? throw new InvalidDataException("來源活頁簿含有未命名工作表。"), StringComparer.Ordinal);
        var results = new List<ImportDryRunSheetResult>();
        long totalCandidates = 0, totalReview = 0;

        foreach (var mapSheet in mapping.Sheets)
        {
            if (mapSheet.Columns.Count > 0 && mapSheet.Columns.All(column => column.Mode.Equals("Ignore", StringComparison.OrdinalIgnoreCase)))
                continue;
            if (!rulesBySheet.TryGetValue(mapSheet.SheetName, out var ruleSheet))
                throw new InvalidDataException($"工作表「{mapSheet.SheetName}」缺少清理規則設定；Dry-run 停止。請重新產生規則範本或補齊設定。");
            if (!sheetsByName.TryGetValue(mapSheet.SheetName, out var sheet))
                throw new InvalidDataException($"來源活頁簿找不到工作表「{mapSheet.SheetName}」。");
            var activeRules = ruleSheet.Rules.Where(x => x.Enabled).ToList();
            var titleCol = mapSheet.Columns.Single(x => x.Mode.Equals("Title", StringComparison.OrdinalIgnoreCase)).SourceIndex;
            var mapped = mapSheet.Columns.Where(x => x.Mode is not null && !x.Mode.Equals("Unmapped", StringComparison.OrdinalIgnoreCase) && !x.Mode.Equals("Ignore", StringComparison.OrdinalIgnoreCase)).ToList();
            var perRule = activeRules.ToDictionary(x => x.RuleId, _ => new List<uint>(), StringComparer.OrdinalIgnoreCase);
            var counts = activeRules.ToDictionary(x => x.RuleId, _ => 0L, StringComparer.OrdinalIgnoreCase);
            var seenTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var reviewRows = new HashSet<uint>();
            long candidates = 0;
            var part = WorkbookReader.GetWorksheetPart(wb, sheet);
            using var reader = OpenXmlReader.Create(part);
            while (reader.Read())
            {
                if (reader.ElementType != typeof(Row)) continue;
                var row = reader.LoadCurrentElement() as Row
                    ?? throw new InvalidDataException("Open XML reader 回傳的資料列型別不正確。");
                var rowNo = row.RowIndex?.Value ?? 0;
                if (rowNo < mapSheet.FirstDataRow) continue;
                var values = new Dictionary<int, string>();
                foreach (var cell in row.Elements<Cell>())
                {
                    var idx = ColumnIndex(cell.CellReference?.Value);
                    if (idx > 0) values[idx] = WorkbookReader.GetCellText(cell, shared);
                }
                if (values.Count == 0 || values.Values.All(string.IsNullOrWhiteSpace)) continue;
                candidates++;
                var title = values.GetValueOrDefault(titleCol, string.Empty);
                var normalizedTitle = title.Trim();
                if (activeRules.Any(x => x.RuleId.Equals("missing-title", StringComparison.OrdinalIgnoreCase)) && string.IsNullOrWhiteSpace(title)) Match("missing-title");
                if (activeRules.Any(x => x.RuleId.Equals("duplicate-title", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(title) && !seenTitles.Add(normalizedTitle)) Match("duplicate-title");
                else if (!string.IsNullOrWhiteSpace(title)) seenTitles.Add(normalizedTitle);

                foreach (var rule in activeRules)
                {
                    if (rule.RuleId.Equals("missing-title", StringComparison.OrdinalIgnoreCase) || rule.RuleId.Equals("duplicate-title", StringComparison.OrdinalIgnoreCase)) continue;
                    var targetColumns = mapped.Where(c => rule.TargetFields.Count == 0 || rule.TargetFields.Contains(c.TargetField!, StringComparer.OrdinalIgnoreCase)).Select(c => c.SourceIndex);
                    var matched = targetColumns.Any(idx => values.TryGetValue(idx, out var value) && (rule.RuleId.ToLowerInvariant() switch
                    {
                        "edge-whitespace" => value.Length > 0 && !string.Equals(value, value.Trim(), StringComparison.Ordinal),
                        "control-characters" => value.Any(ch => char.IsControl(ch) && ch is not '\t' and not '\r' and not '\n'),
                        "excel-error" => ExcelErrors.Contains(value) || value.StartsWith("#ERROR:", StringComparison.OrdinalIgnoreCase),
                        _ => false
                    }));
                    if (matched) Match(rule.RuleId);
                }
                void Match(string id)
                {
                    counts[id]++;
                    if (perRule[id].Count < 20) perRule[id].Add(rowNo);
                    reviewRows.Add(rowNo);
                }
            }
            var ruleResults = activeRules.Select(x => new ImportDryRunRuleResult(x.RuleId, counts[x.RuleId], perRule[x.RuleId])).ToList();
            results.Add(new ImportDryRunSheetResult(mapSheet.SheetName, candidates, reviewRows.Count, ruleResults));
            totalCandidates += candidates;
            totalReview += reviewRows.Count;
        }

        return new ImportDryRunReport(Path.GetFullPath(workbookPath), Path.GetFullPath(mappingPath), Path.GetFullPath(rulesPath), DateTimeOffset.UtcNow, true, totalCandidates, totalReview, results,
            new[] { "Dry-run 僅讀取 Excel、mapping 與 rules；不寫入 SQLite、不修改或另存來源 Excel。", "MatchedRows 是符合該規則的列數；同一列符合多項規則時，各規則分別計數。", "RowsRequiringReview 是至少符合一項啟用規則的唯一資料列數。", "每項規則最多列出 20 個 Excel 列號樣本；不輸出儲存格內容。", "目前 Action 僅支援 Review，報告不代表已匯入或已修正。" });
    }

    private static int ColumnIndex(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference)) return 0;
        var n = 0;
        foreach (var ch in reference) { if (!char.IsLetter(ch)) break; n = n * 26 + char.ToUpperInvariant(ch) - 'A' + 1; }
        return n;
    }
}
