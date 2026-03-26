using CommunityToolkit.Mvvm.ComponentModel;

namespace DevHub.Infrastructure.Storage;

public class AppSettings : ObservableObject
{
    private List<IdeEntry> _ides = [];

    public List<IdeEntry> Ides
    {
        get => _ides;
        set => SetProperty(ref _ides, value);
    }

    private int _defaultIdeIndex;

    public int DefaultIdeIndex
    {
        get => _defaultIdeIndex;
        set => SetProperty(ref _defaultIdeIndex, value);
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
        var scanner = new IdeScanner();
        settings.Ides = scanner.Scan();

        if (settings.Ides.Count > 0)
            settings.DefaultIdeIndex = 0;

        return settings;
    }
}
