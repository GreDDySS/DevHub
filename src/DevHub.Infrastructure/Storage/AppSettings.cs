using CommunityToolkit.Mvvm.ComponentModel;

namespace DevHub.Infrastructure.Storage;

public class AppSettings : ObservableObject
{
    private string _vsCodePath = string.Empty;

    public string VsCodePath
    {
        get => _vsCodePath;
        set => SetProperty(ref _vsCodePath, value);
    }

    private string _visualStudioPath = string.Empty;

    public string VisualStudioPath
    {
        get => _visualStudioPath;
        set => SetProperty(ref _visualStudioPath, value);
    }

    private string _riderPath = string.Empty;

    public string RiderPath
    {
        get => _riderPath;
        set => SetProperty(ref _riderPath, value);
    }

    private bool _autostartEnabled;

    public bool AutostartEnabled
    {
        get => _autostartEnabled;
        set => SetProperty(ref _autostartEnabled, value);
    }

    private bool _minimizeToTray = true;

    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set => SetProperty(ref _minimizeToTray, value);
    }

    public static AppSettings DetectDefaults()
    {
        var settings = new AppSettings();

        var vsCodePaths = new[]
        {
            @"C:\Program Files\Microsoft VS Code\Code.exe",
            @"C:\Program Files (x86)\Microsoft VS Code\Code.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Programs\Microsoft VS Code\Code.exe")
        };
        settings.VsCodePath = vsCodePaths.FirstOrDefault(File.Exists) ?? string.Empty;

        var riderPaths = new[]
        {
            @"C:\Program Files\JetBrains\Rider\bin\rider64.exe",
            @"C:\Program Files\JetBrains\Rider 2024.3\bin\rider64.exe",
            @"C:\Program Files\JetBrains\Rider 2025.1\bin\rider64.exe"
        };
        settings.RiderPath = riderPaths.FirstOrDefault(File.Exists) ?? string.Empty;

        return settings;
    }
}
