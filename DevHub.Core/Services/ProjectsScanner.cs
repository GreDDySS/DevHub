using DevHub.Core.Models;
using DevHub.Core.Interfaces;
using System.Diagnostics;

namespace DevHub.Core.Services
{
    public class ProjectsScanner : IProjectsScanner
    {
        public List<ProjectInfo> Scan(ProjectSettings settings)
        {
            var projects = new List<ProjectInfo>();

            foreach (var root in settings.ScanPath)
            {
                if (Directory.Exists(root))
                    ScanDerectory(root, settings, projects);

                
            }

            return projects;
        }

        private void ScanDerectory(string directory, ProjectSettings settings, List<ProjectInfo> projects)
        {
            try
            {
                var dirName = Path.GetFileName(directory);

                if (settings.IgnoredDirectories.Any(d => d.Equals(dirName, StringComparison.OrdinalIgnoreCase))) return;

                foreach (var ext in settings.FileExtension)
                {
                    var files = Directory.GetFiles(directory, $"*{ext}", SearchOption.TopDirectoryOnly);

                    if (files.Length > 0)
                    {
                        var file = files[0];
                        var info = new FileInfo(file);

                        projects.Add(new ProjectInfo
                        {
                            Name = new DirectoryInfo(directory).Name,
                            Path = directory,
                            Language = DetectLanguage(ext),
                            LastModifided = info.LastWriteTime
                        });

                        return;
                    }
                }

                foreach (var subDir in Directory.GetDirectories(directory))
                {
                    ScanDerectory(subDir, settings, projects);
                }
            }
            catch (UnauthorizedAccessException) { }
        }

        public string DetectLanguage(string ext) =>
            ext switch
            {
                ".sln" or ".csproj" => "C#",
                ".py" => "Python",
                ".js" => "JS",
                _ => "Unknown"
            };
    }
}
