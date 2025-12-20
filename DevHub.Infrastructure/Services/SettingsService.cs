using DevHub.Core.Interfaces;
using DevHub.Core.Models;
using System.IO;
using System.Text.Json;

namespace DevHub.Infrastructure.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingPath;

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "DevHub");
            Directory.CreateDirectory(appFolder);
            _settingPath = Path.Combine(appFolder, "settings.json");
        }

        public ProjectSettings LoadSettings()
        {
            if (!File.Exists(_settingPath)) return new ProjectSettings();

            try
            {
                var json = File.ReadAllText(_settingPath);
                return JsonSerializer.Deserialize<ProjectSettings>(json) ?? new ProjectSettings();
            }
            catch
            {
                return new ProjectSettings(); 
            }
        }

        public void SaveSettings(ProjectSettings settings)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingPath, json);
        }
    }
}
