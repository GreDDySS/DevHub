using System.Collections.ObjectModel;
using System.Text.Json;
using DevHub.Infrastructure.Configuration;

namespace DevHub.Infrastructure.Storage;

public abstract class JsonFileStore<T> where T : class
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private List<T>? _cache;
    private DateTime _lastCacheTime = DateTime.MinValue;

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

        // Return cached data if file hasn't changed
        if (_cache is not null && File.Exists(_filePath))
        {
            var lastWrite = File.GetLastWriteTimeUtc(_filePath);
            if (lastWrite <= _lastCacheTime)
                return [.. _cache];
        }

        if (!File.Exists(_filePath))
        {
            _cache = [];
            return _cache;
        }

        await _lock.WaitAsync(ct);
        try
        {
            ct.ThrowIfCancellationRequested();

            // Double-check after acquiring lock
            if (_cache is not null)
            {
                var lastWrite = File.GetLastWriteTimeUtc(_filePath);
                if (lastWrite <= _lastCacheTime)
                    return [.. _cache];
            }

            var json = await File.ReadAllTextAsync(_filePath, ct);
            if (string.IsNullOrWhiteSpace(json))
            {
                _cache = [];
                return _cache;
            }

            var wrapper = JsonSerializer.Deserialize<JsonDataWrapper<T>>(json, _jsonOptions);
            _cache = wrapper?.Items ?? [];
            _lastCacheTime = DateTime.UtcNow;
            return [.. _cache];
        }
        catch (JsonException)
        {
            _cache = [];
            return _cache;
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
            await Task.Run(() => File.Replace(tempPath, _filePath, null), ct);

            // Update cache — replace reference, don't copy
            _cache = items;
            _lastCacheTime = DateTime.UtcNow;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void InvalidateCache()
    {
        _cache = null;
        _lastCacheTime = DateTime.MinValue;
    }
}

internal class JsonDataWrapper<T> where T : class
{
    public int Version { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<T> Items { get; set; } = [];
}
