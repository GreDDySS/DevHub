using System.Diagnostics;
using DevHub.Domain.Interfaces;

namespace DevHub.Infrastructure.Services;

public class ProcessLauncher : IProcessLauncher
{
    private static readonly HashSet<string> ExcludedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", "node_modules", ".venv", "venv", "bin", "obj", "dist", "build", "__pycache__"
    };

    public void OpenInExplorer(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

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
        ArgumentException.ThrowIfNullOrWhiteSpace(idePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        if (!File.Exists(idePath))
            throw new FileNotFoundException($"IDE not found: {idePath}");

        if (!Directory.Exists(projectPath))
            throw new DirectoryNotFoundException($"Project directory not found: {projectPath}");

        var psi = new ProcessStartInfo
        {
            FileName = idePath,
            UseShellExecute = true
        };
        psi.ArgumentList.Add(projectPath);

        Process.Start(psi);
    }

    public void OpenConsole(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var targetPath = Directory.Exists(path) ? path : Path.GetDirectoryName(path);

        if (string.IsNullOrWhiteSpace(targetPath) || !Directory.Exists(targetPath))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "wt",
                UseShellExecute = true
            };
            psi.ArgumentList.Add("-d");
            psi.ArgumentList.Add(targetPath);

            Process.Start(psi);
        }
        catch
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = true
            };
            psi.ArgumentList.Add("/K");
            psi.ArgumentList.Add($"cd /d \"{targetPath}\"");

            Process.Start(psi);
        }
    }

    public DateTime? GetLastWriteTime(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return null;

            var latestWrite = Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly)
                .Where(f => !IsExcluded(f))
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
                    .Where(d => !ExcludedFolders.Contains(Path.GetFileName(d)))
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

    private static bool IsExcluded(string filePath)
        => ExcludedFolders.Any(e => filePath.Contains($"\\{e}\\") || filePath.Contains($"/{e}/"));
}
