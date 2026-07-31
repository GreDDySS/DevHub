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
            return await DetectDefaultsAsync(ct);

        try
        {
            var json = await File.ReadAllTextAsync(AppPaths.SettingsFile, ct);
            if (string.IsNullOrWhiteSpace(json))
                return await DetectDefaultsAsync(ct);

            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
            return settings ?? await DetectDefaultsAsync(ct);
        }
        catch
        {
            return await DetectDefaultsAsync(ct);
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken ct = default)
    {
        AppPaths.EnsureDirectoriesExist();
        var json = JsonSerializer.Serialize(settings, _jsonOptions);

        // Atomic write
        var tempPath = AppPaths.SettingsFile + ".tmp";
        await File.WriteAllTextAsync(tempPath, json, ct);
        await Task.Run(() => File.Replace(tempPath, AppPaths.SettingsFile, null), ct);
    }

    private async Task<AppSettings> DetectDefaultsAsync(CancellationToken ct = default)
    {
        var ides = await ideScanner.ScanAsync(ct);

        return new AppSettings
        {
            Ides = ides,
            DefaultIdeIndex = ides.Count > 0 ? 0 : 0
        };
    }
}
