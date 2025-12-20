using DevHub.Core.Models;

namespace DevHub.Core.Interfaces
{
    public interface IProjectsScanner
    {
        Task<List<ProjectInfo>> ScanAsync(ProjectSettings settings);
    }
}
