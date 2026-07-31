using DevHub.Application.Interfaces;
using DevHub.Domain.Enums;
using DevHub.Domain.Models;

namespace DevHub.Application.UseCases.Projects;

public class DetectProjectsUseCase : IDetectProjectsUseCase
{
    private const int MaxScanDepth = 5;

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

    public async Task<List<Project>> ExecuteAsync(string rootPath, CancellationToken ct = default)
    {
        if (!Directory.Exists(rootPath))
            return [];

        var dirs = Directory.EnumerateDirectories(rootPath)
            .Where(d => !ExcludedFolders.Contains(Path.GetFileName(d)))
            .ToList();

        var results = new List<(string Dir, ProgrammingLanguage? Lang)>();
        using var throttle = new SemaphoreSlim(Environment.ProcessorCount);

        var tasks = dirs.Select(async dir =>
        {
            await throttle.WaitAsync(ct);
            try
            {
                var lang = await Task.Run(() => ScanDirectory(dir, 0), ct);
                lock (results) { results.Add((dir, lang)); }
            }
            finally
            {
                throttle.Release();
            }
        });

        await Task.WhenAll(tasks);

        return results
            .Where(r => r.Lang.HasValue)
            .Select(r => Project.Create(Path.GetFileName(r.Dir), r.Dir, r.Lang!.Value))
            .ToList();
    }

    private ProgrammingLanguage? DetectLanguage(string directory)
        => ScanDirectory(directory, 0);

    private ProgrammingLanguage? ScanDirectory(string directory, int depth)
    {
        if (depth > MaxScanDepth)
            return null;

        try
        {
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                var ext = Path.GetExtension(file);
                if (string.IsNullOrEmpty(ext) || IgnoredExtensions.Contains(ext))
                    continue;

                if (ExtensionMap.TryGetValue(ext, out var lang))
                    return lang;
            }

            foreach (var subDir in Directory.EnumerateDirectories(directory))
            {
                var folderName = Path.GetFileName(subDir);
                if (ExcludedFolders.Contains(folderName))
                    continue;

                var result = ScanDirectory(subDir, depth + 1);
                if (result.HasValue)
                    return result;
            }
        }
        catch
        {
            // Ignore inaccessible directories
        }

        return null;
    }
}
