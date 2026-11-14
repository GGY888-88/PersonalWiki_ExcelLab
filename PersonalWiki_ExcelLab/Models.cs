namespace PersonalWiki.ExcelLab;

public sealed record CellInfo(string Reference, string Value, string? Formula);

public sealed record IndexedRow(
    string FilePath,
    string SheetName,
    uint RowNumber,
    string Name,
    string Content);

public sealed record PackagePartInfo(string Name, long Length, string Sha256);

public sealed record PackageManifest(
    string SourceFile,
    DateTimeOffset CreatedAt,
    IReadOnlyList<PackagePartInfo> Parts);

public sealed record PartDifference(
    string Name,
    string ChangeType,
    string? BeforeSha256,
    string? AfterSha256);
