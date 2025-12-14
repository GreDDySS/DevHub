using DevHub.Core.Interfaces;
using DevHub.Core.Models;
using DevHub.Core.Services;
using DevHub.UI.Base;
using System.Windows.Input;

namespace DevHub.UI.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private ProjectSettings _settings;

        public ProjectsViewModel ProjectsVM { get; }

        private ProjectSettingsViewModel? _settingsVM;

        public ProjectSettingsViewModel? SettingsVM
        {
            get => _settingsVM;
            private set
            {
                _settingsVM = value;
                OnPropertyChanged();
            }
        }

        private bool _isSettingsOpen;
        public bool IsSettingsOpen
        {
            get => _isSettingsOpen;
            set
            {
                _isSettingsOpen = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenSettingsCommand { get; }

        public MainWindowViewModel()
        {
            _settingsService = new SettingsService();
            _settings = _settingsService.LoadSettings();

            var scanner = new ProjectsScanner();
            var service = new ProjectsService(scanner, _settings);

            ProjectsVM = new ProjectsViewModel(service, _settings);

            OpenSettingsCommand = new RelayCommand(OpenSettings);
        }

        private void OpenSettings()
        {
            SettingsVM = new ProjectSettingsViewModel(_settingsService, _settings);
            SettingsVM.SettingsSaved += OnSettingsSaved;
            SettingsVM.SettingsCancelled += OnSettingsCancelled;
            IsSettingsOpen = true;
        }

        private void OnSettingsSaved(ProjectSettings newSettings)
        {
            _settings = newSettings;
            var scanner = new ProjectsScanner();
            var service = new ProjectsService(scanner, _settings);
            ProjectsVM.UpdateSettings(service, _settings);

            if (SettingsVM != null)
            {
                SettingsVM.SettingsSaved -= OnSettingsSaved;
                SettingsVM.SettingsCancelled -= OnSettingsCancelled;
            }

            IsSettingsOpen = false;
            SettingsVM = null;
        }

        private void OnSettingsCancelled()
        {
            if (SettingsVM != null)
            {
                SettingsVM.SettingsSaved -= OnSettingsSaved;
                SettingsVM.SettingsCancelled -= OnSettingsCancelled;
            }

            IsSettingsOpen = false;
            SettingsVM = null;
        }
    }
}