using DevHub.Core.Interfaces;
using DevHub.Core.Models;
using DevHub.Core.ViewModels;
using System.Collections.ObjectModel;
using DevHub.Modules.Projects.ViewModels;

namespace DevHub.Modules.Projects.ViewModels
{
    public class ProjectsViewModel : BaseViewModel
    {
        public bool IsLoading { get; private set; }

        private readonly IProjectsService _service;
        private readonly IIDEOpener _ideOpener;
        private readonly ISettingsService _settingsService;
        public ObservableCollection<ProjectCardViewModel> Projects { get; }

        public ProjectsViewModel(IProjectsService service, ISettingsService settingsService, IIDEOpener ideOpener)
        {
            _service = service;
            _settingsService = settingsService;
            _ideOpener = ideOpener;

            var currentSettings = _settingsService.LoadSettings();

            Projects = new ObservableCollection<ProjectCardViewModel>(
                _service.Projects.Select(p => 
                new ProjectCardViewModel(p, _service.IDEs, _ideOpener, currentSettings)));

            LoadProjects();
        }

        private async void LoadProjects()
        {
            IsLoading = true;

            await _service.RefreshAsync();
            UpdateList();

            IsLoading = false;
        }

        public void UpdateSettings()
        {
            _service.RefreshAsync();
            UpdateList();
        }
        public async void UpdateList()
        {
            var currentSettings = _settingsService.LoadSettings();
            Projects.Clear();
            foreach (var p in _service.Projects)
            {
                Projects.Add(new ProjectCardViewModel(p, _service.IDEs, _ideOpener, currentSettings));
            }
        }
    }
}