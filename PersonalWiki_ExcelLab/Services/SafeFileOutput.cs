using System.Text;

namespace PersonalWiki.ExcelLab.Services;

/// <summary>Creates user-visible output files without ever replacing an existing destination.</summary>
public static class SafeFileOutput
{
    public static void WriteAllText(string path, string contents, Encoding? encoding = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(contents);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

        FileStream? stream = null;
        try
        {
            stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            using (stream)
            using (var writer = new StreamWriter(stream, encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
                writer.Write(contents);
        }
        catch
        {
            // Only remove a file this call successfully created; never touch a pre-existing destination.
            if (stream is not null)
            {
                try { File.Delete(fullPath); } catch { /* Preserve the original write exception. */ }
            }
            throw;
        }
    }
    /// <summary>Atomically updates an application setting or user-edited profile after fully writing a sibling temp file.</summary>
    public static void WriteAllTextAtomicallyReplacing(string path, string contents, Encoding? encoding = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(contents);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        var temporary = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            WriteAllText(temporary, contents, encoding);
            File.Move(temporary, fullPath, overwrite: true);
        }
        catch
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); } catch { /* Preserve the original exception. */ }
            throw;
        }
    }

}
