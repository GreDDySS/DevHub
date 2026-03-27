using DevHub.Domain.Models;

namespace DevHub.Domain.Interfaces;

public interface IAppSettingsStore
{
    Task<AppSettings> LoadAsync();
    Task SaveAsync(AppSettings settings);
}
