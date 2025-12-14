using DevHub.Core.Interfaces;
using DevHub.Core.Models;
using DevHub.UI.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DevHub.UI.ViewModels
{
    public class ProjectSettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly ProjectSettings _originalSettings;

        public ObservableCollection<string> ScanPaths { get; }
        public ObservableCollection<string> FileExtensions { get; }
        public ObservableCollection<IDEInfo> IDEs { get; }
        public int RefreshIntervalMinutes { get; set; }

        public bool ShowName { get; set; }
        public bool ShowDescription { get; set; }
        public bool ShowLanguage { get; set; }
        public bool ShowLastModified { get; set; }
        public bool ShowIDEButtons { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddScanPathCommand { get; }
        public ICommand RemoveScanPathCommand { get; }
        public ICommand AddFileExtensionCommand { get; }
        public ICommand RemoveFileExtensionCommand { get; }
        public ICommand AddIDECommand { get; }
        public ICommand RemoveIDECommand { get; }

        public event Action<ProjectSettings>? SettingsSaved;
        public event Action? SettingsCancelled;

        public ProjectSettingsViewModel(ISettingsService settingsService, ProjectSettings currentSettings)
        {
            _settingsService = settingsService;
            _originalSettings = currentSettings;

            ScanPaths = new ObservableCollection<string>(currentSettings.ScanPath);
            FileExtensions = new ObservableCollection<string>(currentSettings.FileExtension);
            IDEs = new ObservableCollection<IDEInfo>(currentSettings.IDEs);
            RefreshIntervalMinutes = currentSettings.RefreshIntervalMinutes;

            ShowName = currentSettings.ShowName;
            ShowDescription = currentSettings.ShowDescription;
            ShowLanguage = currentSettings.ShowLanguage;
            ShowLastModified = currentSettings.ShowLastModified;
            ShowIDEButtons = currentSettings.ShowIDEButtons;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(() => SettingsCancelled?.Invoke());
            AddScanPathCommand = new RelayCommand(AddScanPath);
            AddFileExtensionCommand = new RelayCommand(AddFileExtension);
            AddIDECommand = new RelayCommand(AddIDE);

            RemoveScanPathCommand = new RelayCommand((param) =>
            {
                if (param is string path)
                    RemoveScanPath(path);
            });
            RemoveFileExtensionCommand = new RelayCommand((param) =>
            {
                if (param is string ext)
                    RemoveFileExtension(ext);
            });
            RemoveIDECommand = new RelayCommand((param) =>
            {
                if (param is IDEInfo ide)
                    RemoveIDE(ide);
            });
        }

        private void Save()
        {
            var settings = new ProjectSettings
            {
                ScanPath = ScanPaths.ToList(),
                FileExtension = FileExtensions.ToList(),
                IgnoredDirectories = _originalSettings.IgnoredDirectories,
                RefreshIntervalMinutes = RefreshIntervalMinutes,
                ShowName = ShowName,
                ShowDescription = ShowDescription,
                ShowLanguage = ShowLanguage,
                ShowLastModified = ShowLastModified,
                ShowIDEButtons = ShowIDEButtons,
                IDEs = IDEs.ToList()
            };

            _settingsService.SaveSettings(settings);
            SettingsSaved?.Invoke(settings);
        }

        private void AddScanPath() => ScanPaths.Add(string.Empty);
        private void RemoveScanPath(string? path) => ScanPaths.Remove(path ?? string.Empty);
        private void AddFileExtension() => FileExtensions.Add(string.Empty);
        private void RemoveFileExtension(string? ext) => FileExtensions.Remove(ext ?? string.Empty);
        private void AddIDE() => IDEs.Add(new IDEInfo());
        private void RemoveIDE(IDEInfo? ide) => IDEs.Remove(ide!);
    }
}
