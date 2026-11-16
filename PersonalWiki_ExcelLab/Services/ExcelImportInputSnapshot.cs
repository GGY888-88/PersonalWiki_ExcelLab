using System.Security.Cryptography;

namespace PersonalWiki.ExcelLab.Services;

/// <summary>
/// Creates a private, immutable-for-the-import copy of the three Excel import inputs.
/// The original files are opened read-only with sharing that denies concurrent writers.
/// </summary>
internal sealed class ExcelImportInputSnapshot : IDisposable
{
    private readonly string _directory;

    private ExcelImportInputSnapshot(string directory, string workbookPath, string mappingPath, string rulesPath)
    {
        _directory = directory;
        WorkbookPath = workbookPath;
        MappingPath = mappingPath;
        RulesPath = rulesPath;
    }

    public string WorkbookPath { get; }
    public string MappingPath { get; }
    public string RulesPath { get; }
    internal string SnapshotDirectory => _directory;

    public static ExcelImportInputSnapshot Create(
        string workbookPath,
        string mappingPath,
        string rulesPath,
        string expectedWorkbookHash,
        string expectedMappingHash,
        string expectedRulesHash)
    {
        var directory = Path.Combine(Path.GetTempPath(), "PersonalWiki-Import-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var workbookExtension = Path.GetExtension(workbookPath);
            if (string.IsNullOrWhiteSpace(workbookExtension)) workbookExtension = ".xlsx";
            var workbookCopy = Path.Combine(directory, "source" + workbookExtension);
            var mappingCopy = Path.Combine(directory, "mapping.json");
            var rulesCopy = Path.Combine(directory, "rules.json");

            CopyAndVerify(workbookPath, workbookCopy, expectedWorkbookHash, "來源 Excel");
            CopyAndVerify(mappingPath, mappingCopy, expectedMappingHash, "欄位對應設定");
            CopyAndVerify(rulesPath, rulesCopy, expectedRulesHash, "清理規則設定");

            // Re-hash the originals after all copies. A change before, during, or between
            // copies invalidates the snapshot; the import never falls back to live files.
            VerifyOriginal(workbookPath, expectedWorkbookHash, "來源 Excel");
            VerifyOriginal(mappingPath, expectedMappingHash, "欄位對應設定");
            VerifyOriginal(rulesPath, expectedRulesHash, "清理規則設定");

            // Mark completed snapshot files read-only before exposing them to the importer.
            // This is an additional guard against accidental writes by downstream code.
            File.SetAttributes(workbookCopy, File.GetAttributes(workbookCopy) | FileAttributes.ReadOnly);
            File.SetAttributes(mappingCopy, File.GetAttributes(mappingCopy) | FileAttributes.ReadOnly);
            File.SetAttributes(rulesCopy, File.GetAttributes(rulesCopy) | FileAttributes.ReadOnly);

            return new ExcelImportInputSnapshot(directory, workbookCopy, mappingCopy, rulesCopy);
        }
        catch
        {
            TryDeleteDirectory(directory);
            throw;
        }
    }

    private static void CopyAndVerify(string sourcePath, string targetPath, string expectedHash, string label)
    {
        using var source = new FileStream(Path.GetFullPath(sourcePath), FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 128, FileOptions.SequentialScan);
        using var target = new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 128, FileOptions.SequentialScan);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[1024 * 128];
        int read;
        while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            hash.AppendData(buffer, 0, read);
            target.Write(buffer, 0, read);
        }
        target.Flush(flushToDisk: true);
        var actualHash = Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
        if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
            throw new IOException($"{label}在建立匯入快照時與預檢版本不同；已停止匯入，請重新預檢。");
    }

    private static void VerifyOriginal(string path, string expectedHash, string label)
    {
        var actualHash = ExcelImportPreflightService.HashFile(path);
        if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
            throw new IOException($"{label}在建立匯入快照期間發生變動；已停止匯入，請重新預檢。");
    }

    public void Dispose() => TryDeleteDirectory(_directory);

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (!Directory.Exists(directory)) return;
            // Windows refuses to delete read-only files; clear only attributes inside
            // this uniquely-created private snapshot directory before removing it.
            foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                try { File.SetAttributes(file, FileAttributes.Normal); }
                catch { /* Continue best-effort cleanup without masking the import result. */ }
            }
            Directory.Delete(directory, recursive: true);
        }
        catch { /* A leftover temp snapshot is safer than masking the import result. */ }
    }
}
