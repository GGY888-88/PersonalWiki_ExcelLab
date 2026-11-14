using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public static class PackageIntegrity
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static PackageManifest CreateManifest(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("找不到檔案。", filePath);

        var parts = new List<PackagePartInfo>();
        using var archive = ZipFile.OpenRead(filePath);
        foreach (var entry in archive.Entries.OrderBy(e => e.FullName, StringComparer.Ordinal))
        {
            using var stream = entry.Open();
            var hash = Convert.ToHexString(SHA256.HashData(stream));
            parts.Add(new PackagePartInfo(entry.FullName, entry.Length, hash));
        }

        return new PackageManifest(Path.GetFullPath(filePath), DateTimeOffset.Now, parts);
    }

    public static void SaveManifest(string filePath, string outputJson)
    {
        var manifest = CreateManifest(filePath);
        var fullOutputPath = Path.GetFullPath(outputJson);
        Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath)!);
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        // Reports are user data: never replace an existing destination.
        using var stream = new FileStream(fullOutputPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(json);
    }

    public static IReadOnlyList<PartDifference> Compare(string beforePath, string afterPath)
    {
        var before = CreateManifest(beforePath).Parts.ToDictionary(p => p.Name, StringComparer.Ordinal);
        var after = CreateManifest(afterPath).Parts.ToDictionary(p => p.Name, StringComparer.Ordinal);
        var names = before.Keys.Union(after.Keys, StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal);
        var differences = new List<PartDifference>();

        foreach (var name in names)
        {
            before.TryGetValue(name, out var oldPart);
            after.TryGetValue(name, out var newPart);
            if (oldPart is null)
                differences.Add(new PartDifference(name, "新增", null, newPart!.Sha256));
            else if (newPart is null)
                differences.Add(new PartDifference(name, "刪除", oldPart.Sha256, null));
            else if (!string.Equals(oldPart.Sha256, newPart.Sha256, StringComparison.Ordinal))
                differences.Add(new PartDifference(name, "修改", oldPart.Sha256, newPart.Sha256));
        }

        return differences;
    }
}
