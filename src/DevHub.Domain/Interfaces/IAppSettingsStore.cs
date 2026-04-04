using DevHub.Domain.Models;

namespace DevHub.Domain.Interfaces;

public interface IAppSettingsStore
{
    Task<AppSettings> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(AppSettings settings, CancellationToken ct = default);
}
