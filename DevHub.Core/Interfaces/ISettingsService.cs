using DevHub.Core.Models;

namespace DevHub.Core.Interfaces
{
    public interface ISettingsService
    {
        ProjectSettings LoadSettings();
        void SaveSettings(ProjectSettings settings);
    }
}
