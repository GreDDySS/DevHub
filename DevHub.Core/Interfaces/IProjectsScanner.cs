using DevHub.Core.Models;

namespace DevHub.Core.Interfaces
{
    public interface IProjectsScanner
    {
        List<ProjectInfo> Scan(ProjectSettings settings);
    }
}
