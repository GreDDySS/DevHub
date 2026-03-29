using System.Diagnostics;
using System.IO;
using DevHub.Domain.Interfaces;

namespace DevHub.Infrastructure.Services;

public class ProcessLauncher : IProcessLauncher
{
    public void OpenInExplorer(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));

        var targetPath = Directory.Exists(path) ? path : Path.GetDirectoryName(path);

        if (string.IsNullOrWhiteSpace(targetPath) || !Directory.Exists(targetPath))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        Process.Start(new ProcessStartInfo
        {
            FileName = targetPath,
            UseShellExecute = true
        });
    }

    public void OpenInIde(string idePath, string projectPath)
    {
        if (string.IsNullOrWhiteSpace(idePath))
            throw new ArgumentException("IDE path cannot be empty", nameof(idePath));

        if (string.IsNullOrWhiteSpace(projectPath))
            throw new ArgumentException("Project path cannot be empty", nameof(projectPath));

        if (!File.Exists(idePath))
            throw new FileNotFoundException($"IDE not found: {idePath}");

        if (!Directory.Exists(projectPath))
            throw new DirectoryNotFoundException($"Project directory not found: {projectPath}");

        Process.Start(new ProcessStartInfo
        {
            FileName = idePath,
            Arguments = $"\"{projectPath}\"",
            UseShellExecute = true
        });
    }

    public void OpenConsole(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));

        var targetPath = Directory.Exists(path) ? path : Path.GetDirectoryName(path);

        if (string.IsNullOrWhiteSpace(targetPath) || !Directory.Exists(targetPath))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "wt",
                Arguments = $"-d \"{targetPath}\"",
                UseShellExecute = true
            });
        }
        catch
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/K cd /d \"{targetPath}\"",
                UseShellExecute = true
            });
        }
    }

    public DateTime? GetLastWriteTime(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return null;

            var excluded = new HashSet<string> { ".git", "node_modules", ".venv", "venv", "bin", "obj", "dist", "build", "__pycache__" };

            var latestWrite = Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly)
                .Where(f => !IsExcluded(f, excluded))
                .Select(f =>
                {
                    try { return File.GetLastWriteTimeUtc(f); }
                    catch { return DateTime.MinValue; }
                })
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();

            if (latestWrite == DateTime.MinValue)
            {
                latestWrite = Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly)
                    .Where(d => !excluded.Contains(Path.GetFileName(d)))
                    .Select(d =>
                    {
                        try { return Directory.GetLastWriteTimeUtc(d); }
                        catch { return DateTime.MinValue; }
                    })
                    .DefaultIfEmpty(DateTime.MinValue)
                    .Max();
            }

            return latestWrite == DateTime.MinValue ? null : latestWrite;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsExcluded(string filePath, HashSet<string> excluded)
    {
        return excluded.Any(e => filePath.Contains($"\\{e}\\") || filePath.Contains($"/{e}/"));
    }
}
