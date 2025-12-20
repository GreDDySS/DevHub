using DevHub.Core.Models;
using DevHub.Core.Interfaces;
using System.Diagnostics;
using System.IO;

namespace DevHub.Infrastructure.Services
{
    public class ProjectsScanner : IProjectsScanner
    {
        public async Task<List<ProjectInfo>> ScanAsync(ProjectSettings settings)
        {
            return await Task.Run(() => ScanInterval(settings));
        }

        private List<ProjectInfo> ScanInterval(ProjectSettings settings)
        {
            var projects = new List<ProjectInfo>();

            var otions = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
                MaxRecursionDepth = 6,
                ReturnSpecialDirectories = false
            };

            foreach (var rootPath in settings.ScanPath)
            {
                if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
                    continue;

                foreach (var ext in settings.FileExtension)
                {
                    try
                    {
                        var files = Directory.EnumerateFiles(rootPath, $"*{ext}", otions);

                        foreach (var file in files)
                        {
                            var dir = Path.GetDirectoryName(file);
                            var dirName = Path.GetFileName(dir);

                            if (settings.IgnoredDirectories.Contains(dirName, StringComparer.OrdinalIgnoreCase))
                                continue;

                            projects.Add(new ProjectInfo
                            {
                                Name = dirName ?? "Unknown",
                                Path = dir ?? rootPath,
                                Description = "Найдено сканером",
                                Language = DetectLanguage(ext),
                                LastModified = File.GetLastWriteTime(file)
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error scanning {rootPath}: {ex.Message}");
                    }
                }
            }

            return projects.GroupBy(p => p.Path).Select(g => g.First()).ToList();
        }

        public string DetectLanguage(string ext) => ext switch
        {
            ".sln" or ".csproj" => "C#",
            ".py" => "Python",
            ".js" or ".ts" => "JavaScript",
            ".cpp" or ".h" => "C++",
            _ => "Unknown"
        };
    }
}
