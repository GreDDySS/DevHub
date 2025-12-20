using DevHub.Core.Interfaces;
using DevHub.Core.Models;

namespace DevHub.Modules.Projects.Services
{
    public class ProjectsService : IProjectsService
    {
        private readonly IProjectsScanner _scanner;
        private readonly ISettingsService _settingsService;

        public List<ProjectInfo> _projectsInternal = new();
        public IReadOnlyList<ProjectInfo> Projects => _projectsInternal;
        public IReadOnlyList<IDEInfo> IDEs => _settingsService.LoadSettings().IDEs;

        public ProjectsService(IProjectsScanner scanner, ISettingsService settingsService)
        {
            _scanner = scanner;
            _settingsService = settingsService;
        }

        public async Task RefreshAsync()
        {
            var settings = _settingsService.LoadSettings();
            _projectsInternal.Clear();

            var projects = await _scanner.ScanAsync(settings);

            _projectsInternal.AddRange(projects);
        }
    }
}
