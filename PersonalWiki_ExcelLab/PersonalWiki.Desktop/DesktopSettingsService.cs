using PersonalWiki.ExcelLab.Services;
using System.Text.Json;

namespace PersonalWiki.Desktop;

internal sealed class DesktopSettingsService
{
    private sealed class SettingsData
    {
        public string? LastDatabasePath { get; set; }
        public int EntryButtonWidth { get; set; } = 160;
        public int EntryButtonHeight { get; set; } = 64;
        public int MainActionButtonWidth { get; set; } = 100;
        public int MainActionButtonHeight { get; set; } = 68;
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PersonalWiki", "desktop-settings.json");

    public string? Load()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return null;
            var data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_settingsPath));
            return data?.LastDatabasePath;
        }
        catch
        {
            // A damaged settings file must not prevent the application from starting.
            return null;
        }
    }

    public (int Width, int Height) LoadEntryButtonSize()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return (160, 64);
            var data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_settingsPath));
            return (Math.Clamp(data?.EntryButtonWidth ?? 160, 100, 360), Math.Clamp(data?.EntryButtonHeight ?? 64, 36, 160));
        }
        catch { return (160, 64); }
    }

    public void SaveEntryButtonSize(int width, int height)
    {
        var data = LoadData();
        data.EntryButtonWidth = Math.Clamp(width, 100, 360);
        data.EntryButtonHeight = Math.Clamp(height, 36, 160);
        Save(data);
    }

    public (int Width, int Height) LoadMainActionButtonSize()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return (100, 68);
            var data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_settingsPath));
            var width = data?.MainActionButtonWidth ?? 100;
            var height = data?.MainActionButtonHeight ?? 68;
            // 2.5.4 的舊預設值不是使用者明確設定，升級後轉為新的預設 100x68。
            if (width == 220 && height == 64) return (100, 68);
            return (Math.Clamp(width, 40, 360), Math.Clamp(height, 20, 160));
        }
        catch { return (100, 68); }
    }

    public void SaveMainActionButtonSize(int width, int height)
    {
        var data = LoadData();
        data.MainActionButtonWidth = Math.Clamp(width, 40, 360);
        data.MainActionButtonHeight = Math.Clamp(height, 20, 160);
        Save(data);
    }

    public void SaveLastDatabase(string path)
    {
        var data = LoadData();
        data.LastDatabasePath = path;
        Save(data);
    }

    private SettingsData LoadData()
    {
        if (!File.Exists(_settingsPath)) return new SettingsData();

        try
        {
            return JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_settingsPath))
                ?? throw new InvalidDataException("桌面設定檔內容為空值。");
        }
        catch (JsonException ex)
        {
            // Never silently replace a damaged settings file with defaults during Save.
            // Keep the original file intact so the user can recover it manually.
            throw new InvalidDataException(
                $"桌面設定檔格式無效，為避免覆蓋可供復原的原始設定，已停止儲存：{_settingsPath}", ex);
        }
    }

    private void Save(SettingsData data)
    {
        var directory = Path.GetDirectoryName(_settingsPath)!;
        Directory.CreateDirectory(directory);
        SafeFileOutput.WriteAllTextAtomicallyReplacing(_settingsPath, JsonSerializer.Serialize(data, JsonOptions));
    }
}
