using System.Text.Json;
using DevHub.Infrastructure.Configuration;

namespace DevHub.Infrastructure.Storage;

public abstract class JsonFileStore<T> where T : class
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);

    protected JsonFileStore(string filePath)
    {
        _filePath = filePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    protected async Task<List<T>> LoadAllAsync(CancellationToken ct = default)
    {
        AppPaths.EnsureDirectoriesExist();

        if (!File.Exists(_filePath))
            return [];

        await _lock.WaitAsync(ct);
        try
        {
            ct.ThrowIfCancellationRequested();

            var json = await File.ReadAllTextAsync(_filePath, ct);
            if (string.IsNullOrWhiteSpace(json))
                return [];

            var wrapper = JsonSerializer.Deserialize<JsonDataWrapper<T>>(json, _jsonOptions);
            return wrapper?.Items ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
        finally
        {
            _lock.Release();
        }
    }

    protected async Task SaveAllAsync(List<T> items, CancellationToken ct = default)
    {
        AppPaths.EnsureDirectoriesExist();

        await _lock.WaitAsync(ct);
        try
        {
            ct.ThrowIfCancellationRequested();

            var wrapper = new JsonDataWrapper<T>
            {
                Version = 1,
                UpdatedAt = DateTime.UtcNow,
                Items = items
            };

            var json = JsonSerializer.Serialize(wrapper, _jsonOptions);

            // Atomic write: write to temp file first, then replace
            var tempPath = _filePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, json, ct);
            File.Replace(tempPath, _filePath, null);
        }
        finally
        {
            _lock.Release();
        }
    }
}

internal class JsonDataWrapper<T> where T : class
{
    public int Version { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<T> Items { get; set; } = [];
}
