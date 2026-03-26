using System.Text.Json;
using DevHub.Infrastructure.Configuration;

namespace DevHub.Infrastructure.Storage;

public class JsonSettingsStore
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppSettings Load()
    {
        AppPaths.EnsureDirectoriesExist();

        if (!File.Exists(AppPaths.SettingsFile))
            return AppSettings.DetectDefaults();

        try
        {
            var json = File.ReadAllText(AppPaths.SettingsFile);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
            return settings ?? AppSettings.DetectDefaults();
        }
        catch
        {
            return AppSettings.DetectDefaults();
        }
    }

    public void Save(AppSettings settings)
    {
        AppPaths.EnsureDirectoriesExist();
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        File.WriteAllText(AppPaths.SettingsFile, json);
    }
}
