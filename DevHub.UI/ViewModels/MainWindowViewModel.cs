using DevHub.Core.Interfaces;
using DevHub.Core.Models;
using DevHub.Core.Services;
using DevHub.UI.Base;
using System.Windows.Input;

namespace DevHub.UI.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IProjectsService _projectsService;
        private readonly ISettingsService _settingsService;
        private readonly IIDEOpener _ideOpener;

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

        public ProjectsViewModel ProjectsVM { get; private set; }

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
        public ICommand OpenSettingsCommand { get; }


        public MainWindowViewModel(IProjectsService projectsService, ISettingsService settingsService, IIDEOpener ideOpener)
        {
            _projectsService = projectsService;
            _settingsService = settingsService;
            _ideOpener = ideOpener;

            ProjectsVM = new ProjectsViewModel(_projectsService, _settingsService, _ideOpener);

            OpenSettingsCommand = new RelayCommand(OpenSettings);
        }

        private void OpenSettings()
        {
            var currentSettings = _settingsService.LoadSettings();

            SettingsVM = new ProjectSettingsViewModel(_settingsService ,currentSettings);

            SettingsVM.SettingsSaved += OnSettingsSaved;
            SettingsVM.SettingsCancelled += OnSettingsCancelled;

            IsSettingsOpen = true;
        }

        private void OnSettingsSaved(ProjectSettings newSettings)
        {
            ProjectsVM.UpdateSettings();
            CloseSettings();
        }

        private void OnSettingsCancelled()
        {
            CloseSettings();
        }

        private void CloseSettings()
        {
            IsSettingsOpen = false;

            if (SettingsVM != null)
            {
                SettingsVM.SettingsSaved -= OnSettingsSaved;
                SettingsVM.SettingsCancelled -= OnSettingsCancelled;
            }
            SettingsVM = null;
        }
    }
}