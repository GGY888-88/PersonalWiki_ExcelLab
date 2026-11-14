using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text.Json;

namespace PersonalWiki.ExcelLab.Services;

public sealed record DatabaseImageFileIntegrityAuditReport(
    string Database, DateTimeOffset CreatedAtUtc, int TotalReferences, int ExistingFiles,
    int MissingFiles, int InvalidPaths, int HashRecorded, int HashMatches,
    int HashMismatches, int HashUnavailable, long TotalExistingFileBytes, string ReportPath);

/// <summary>Read-only image-file verification. Reports aggregate counts only; never emits paths or image content.</summary>
public sealed class DatabaseImageFileIntegrityAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public DatabaseImageFileIntegrityAuditReport Create(string databasePath, string reportPath)
    {
        var database = Path.GetFullPath(databasePath);
        var output = Path.GetFullPath(reportPath);
        if (!File.Exists(database)) throw new FileNotFoundException("找不到知識庫。", database);
        if (string.Equals(database, output, StringComparison.OrdinalIgnoreCase)) throw new IOException("報告路徑不可與資料庫路徑相同。");
        if (File.Exists(output)) throw new IOException("圖片完整性報告已存在；為避免覆蓋已停止輸出。");

        var total = 0; var existing = 0; var missing = 0; var invalid = 0;
        var recorded = 0; var matches = 0; var mismatches = 0; var unavailable = 0; long bytes = 0;
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT file_path, sha256 FROM entry_images ORDER BY id;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                total++;
                var path = reader.GetString(0);
                var expected = reader.IsDBNull(1) ? null : reader.GetString(1);
                if (!string.IsNullOrWhiteSpace(expected)) recorded++;
                try
                {
                    var fullPath = Path.GetFullPath(path);
                    var info = new FileInfo(fullPath);
                    if (!info.Exists) { missing++; continue; }
                    existing++; bytes += info.Length;
                    if (string.IsNullOrWhiteSpace(expected)) continue;
                    try
                    {
                        using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                        var actual = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
                        if (string.Equals(actual, expected.Trim(), StringComparison.OrdinalIgnoreCase)) matches++; else mismatches++;
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException) { unavailable++; }
                }
                catch (Exception ex) when (ex is ArgumentException or NotSupportedException or IOException or UnauthorizedAccessException) { invalid++; }
            }
        }
        var result = new DatabaseImageFileIntegrityAuditReport(database, DateTimeOffset.UtcNow, total, existing, missing,
            invalid, recorded, matches, mismatches, unavailable, bytes, output);
        var directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            JsonSerializer.Serialize(stream, result, JsonOptions);
        return result;
    }
}
