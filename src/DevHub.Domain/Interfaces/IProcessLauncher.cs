namespace DevHub.Domain.Interfaces;

public interface IProcessLauncher
{
    void OpenInExplorer(string path);
    void OpenInIde(string idePath, string projectPath);
    void OpenConsole(string path);
    DateTime? GetLastWriteTime(string path);
}
