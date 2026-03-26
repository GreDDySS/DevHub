namespace DevHub.Infrastructure.Storage;

public class IdeScanner
{
    private static readonly (string Name, string[] Paths)[] KnownIdes =
    [
        ("VS Code", new[]
        {
            @"C:\Program Files\Microsoft VS Code\Code.exe",
            @"C:\Program Files (x86)\Microsoft VS Code\Code.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Programs\Microsoft VS Code\Code.exe")
        }),
        ("Visual Studio", new[]
        {
            @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
            @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe",
            @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe"
        }),
        ("Rider", new[]
        {
            @"C:\Program Files\JetBrains\Rider\bin\rider64.exe",
            @"C:\Program Files\JetBrains\Rider 2024.3\bin\rider64.exe",
            @"C:\Program Files\JetBrains\Rider 2025.1\bin\rider64.exe"
        }),
        ("IntelliJ IDEA", new[]
        {
            @"C:\Program Files\JetBrains\IntelliJ IDEA\bin\idea64.exe",
            @"C:\Program Files\JetBrains\IntelliJ IDEA 2024.3\bin\idea64.exe"
        }),
        ("WebStorm", new[]
        {
            @"C:\Program Files\JetBrains\WebStorm\bin\webstorm64.exe",
            @"C:\Program Files\JetBrains\WebStorm 2024.3\bin\webstorm64.exe"
        }),
        ("PyCharm", new[]
        {
            @"C:\Program Files\JetBrains\PyCharm\bin\pycharm64.exe",
            @"C:\Program Files\JetBrains\PyCharm 2024.3\bin\pycharm64.exe"
        }),
        ("CLion", new[]
        {
            @"C:\Program Files\JetBrains\CLion\bin\clion64.exe"
        }),
        ("GoLand", new[]
        {
            @"C:\Program Files\JetBrains\GoLand\bin\goland64.exe"
        }),
        ("Notepad++", new[]
        {
            @"C:\Program Files\Notepad++\notepad++.exe",
            @"C:\Program Files (x86)\Notepad++\notepad++.exe"
        }),
        ("Sublime Text", new[]
        {
            @"C:\Program Files\Sublime Text\subl.exe",
            @"C:\Program Files\Sublime Text 3\sublime_text.exe"
        })
    ];

    public List<IdeEntry> Scan()
    {
        var results = new List<IdeEntry>();

        foreach (var (name, paths) in KnownIdes)
        {
            var found = paths.FirstOrDefault(File.Exists);
            if (found is not null)
                results.Add(new IdeEntry(name, found));
        }

        return results;
    }

    public static IdeEntry? ScanDirectory(string directory)
    {
        if (!Directory.Exists(directory)) return null;

        var exes = Directory.GetFiles(directory, "*.exe", SearchOption.TopDirectoryOnly);

        if (exes.Length == 0) return null;
        if (exes.Length == 1)
            return new IdeEntry(Path.GetFileNameWithoutExtension(exes[0]), exes[0]);

        var priority = new[] { "code.exe", "devenv.exe", "rider64.exe", "idea64.exe" };
        var best = exes.FirstOrDefault(e =>
            priority.Contains(Path.GetFileName(e).ToLowerInvariant()));

        return new IdeEntry(
            Path.GetFileNameWithoutExtension(best ?? exes[0]),
            best ?? exes[0]);
    }
}
