using System.Text.Json;
using DevHub.Domain.Models;
using DevHub.Infrastructure.Configuration;

namespace DevHub.Infrastructure.Storage;

public class JsonSettingsStore : DevHub.Domain.Interfaces.IAppSettingsStore
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<AppSettings> LoadAsync()
    {
        AppPaths.EnsureDirectoriesExist();

        if (!File.Exists(AppPaths.SettingsFile))
            return DetectDefaults();

        try
        {
            var json = await File.ReadAllTextAsync(AppPaths.SettingsFile);
            if (string.IsNullOrWhiteSpace(json))
                return DetectDefaults();

            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
            return settings ?? DetectDefaults();
        }
        catch
        {
            return DetectDefaults();
        }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        AppPaths.EnsureDirectoriesExist();
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        await File.WriteAllTextAsync(AppPaths.SettingsFile, json);
    }

    private AppSettings DetectDefaults()
    {
        var settings = new AppSettings();
        settings.Ides = _ideScanner.Scan();

        if (settings.Ides.Count > 0)
            settings.DefaultIdeIndex = 0;

        return settings;
    }

    private readonly IdeScanner _ideScanner;

    public JsonSettingsStore(IdeScanner ideScanner)
    {
        _ideScanner = ideScanner;
    }
}
