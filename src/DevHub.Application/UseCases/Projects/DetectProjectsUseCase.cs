using DevHub.Domain.Enums;
using DevHub.Domain.Models;

namespace DevHub.Application.UseCases.Projects;

public class DetectProjectsUseCase
{
    private static readonly HashSet<string> ExcludedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "node_modules", ".venv", "venv", "__pycache__", ".git", "bin", "obj",
        "target", ".next", "dist", "build", ".vs", ".idea", "packages"
    };

    private static readonly Dictionary<string, ProgrammingLanguage> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".cs"] = ProgrammingLanguage.CSharp,
        [".csproj"] = ProgrammingLanguage.CSharp,
        [".sln"] = ProgrammingLanguage.CSharp,
        [".py"] = ProgrammingLanguage.Python,
        [".js"] = ProgrammingLanguage.JavaScript,
        [".mjs"] = ProgrammingLanguage.JavaScript,
        [".cjs"] = ProgrammingLanguage.JavaScript,
        [".ts"] = ProgrammingLanguage.TypeScript,
        [".tsx"] = ProgrammingLanguage.TypeScript,
        [".rs"] = ProgrammingLanguage.Rust,
        [".go"] = ProgrammingLanguage.Go,
        [".java"] = ProgrammingLanguage.Java,
        [".kt"] = ProgrammingLanguage.Java,
        [".cpp"] = ProgrammingLanguage.Cpp,
        [".c"] = ProgrammingLanguage.Cpp,
        [".h"] = ProgrammingLanguage.Cpp,
    };

    private static readonly HashSet<string> IgnoredExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".txt", ".gitignore", ".gitattributes", ".yml", ".yaml",
        ".json", ".xml", ".lock", ".cfg", ".ini", ".env", ".editorconfig",
        ".dockerignore", ".eslintrc", ".prettierrc", ".npmrc"
    };

    public List<Project> Execute(string rootPath)
    {
        if (!Directory.Exists(rootPath))
            return [];

        var projects = new List<Project>();

        foreach (var dir in Directory.GetDirectories(rootPath))
        {
            if (ExcludedFolders.Contains(Path.GetFileName(dir)))
                continue;

            var language = DetectLanguage(dir);
            if (language.HasValue)
            {
                projects.Add(new Project
                {
                    Name = Path.GetFileName(dir),
                    Path = dir,
                    Language = language.Value,
                    Status = ProjectStatus.Active,
                    Tags = [],
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        return projects;
    }

    private ProgrammingLanguage? DetectLanguage(string directory)
    {
        return ScanDirectory(directory, 0);
    }

    private ProgrammingLanguage? ScanDirectory(string directory, int depth)
    {
        if (depth > 5)
            return null;

        try
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                var ext = Path.GetExtension(file);
                if (string.IsNullOrEmpty(ext) || IgnoredExtensions.Contains(ext))
                    continue;

                if (ExtensionMap.TryGetValue(ext, out var lang))
                    return lang;
            }

            foreach (var subDir in Directory.GetDirectories(directory))
            {
                var folderName = Path.GetFileName(subDir);
                if (ExcludedFolders.Contains(folderName))
                    continue;

                var result = ScanDirectory(subDir, depth + 1);
                if (result.HasValue)
                    return result;
            }
        }
        catch { }

        return null;
    }
}
