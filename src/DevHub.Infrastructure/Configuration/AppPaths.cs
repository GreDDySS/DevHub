namespace DevHub.Infrastructure.Configuration;

public static class AppPaths
{
    public static string BaseDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DevHub");

    public static string ProjectsFile { get; } =
        Path.Combine(BaseDirectory, "projects.json");

    public static string LinksFile { get; } =
        Path.Combine(BaseDirectory, "links.json");

    public static string SettingsFile { get; } =
        Path.Combine(BaseDirectory, "settings.json");

    public static string LogsDirectory { get; } =
        Path.Combine(BaseDirectory, "logs");

    public static string BackupDirectory { get; } =
        Path.Combine(BaseDirectory, "backup");

    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(BaseDirectory);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(BackupDirectory);
    }
}
