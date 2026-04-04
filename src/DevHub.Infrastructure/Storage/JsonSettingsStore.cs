using System.Text.Json;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Infrastructure.Configuration;

namespace DevHub.Infrastructure.Storage;

public class JsonSettingsStore(IIdeScanner ideScanner) : IAppSettingsStore
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<AppSettings> LoadAsync(CancellationToken ct = default)
    {
        AppPaths.EnsureDirectoriesExist();

        if (!File.Exists(AppPaths.SettingsFile))
            return DetectDefaults();

        try
        {
            var json = await File.ReadAllTextAsync(AppPaths.SettingsFile, ct);
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

    public async Task SaveAsync(AppSettings settings, CancellationToken ct = default)
    {
        AppPaths.EnsureDirectoriesExist();
        var json = JsonSerializer.Serialize(settings, _jsonOptions);

        // Atomic write
        var tempPath = AppPaths.SettingsFile + ".tmp";
        await File.WriteAllTextAsync(tempPath, json, ct);
        File.Replace(tempPath, AppPaths.SettingsFile, null);
    }

    private AppSettings DetectDefaults()
    {
        var ides = ideScanner.Scan();

        return new AppSettings
        {
            Ides = ides,
            DefaultIdeIndex = ides.Count > 0 ? 0 : 0
        };
    }
}
