using DevHub.Core.Interfaces;
using DevHub.Core.Models;

namespace DevHub.Core.Services
{
    public class ProjectsService : IProjectsService
    {
        private readonly IProjectsScanner _scanner;
        private readonly ProjectSettings _settings;

        public List<ProjectInfo> ProjectsInternal { get; } = new();
        public IReadOnlyList<ProjectInfo> Projects => ProjectsInternal;
        public IReadOnlyList<IDEInfo> IDEs => _settings.IDEs;

        public ProjectsService(IProjectsScanner scanner, ProjectSettings settings)
        {
            _scanner = scanner;
            _settings = settings;
        }

        public void Refresh()
        {
            ProjectsInternal.Clear();
            ProjectsInternal.AddRange(_scanner.Scan(_settings));
        }
    }
}
