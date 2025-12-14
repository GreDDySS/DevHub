using DevHub.Core.Models;

namespace DevHub.Core.Interfaces
{
    public interface IProjectsService
    {
        IReadOnlyList<ProjectInfo> Projects { get; }
        IReadOnlyList<IDEInfo> IDEs { get; }
        void Refresh();
    }
}
