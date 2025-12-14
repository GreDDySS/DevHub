using DevHub.Core.Models;

namespace DevHub.Core.Interfaces
{
    public interface IIDEOpener
    {
        void OpenProject(string projectPath, IDEInfo ide);
    }
}
