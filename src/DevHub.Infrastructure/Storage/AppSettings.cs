using CommunityToolkit.Mvvm.ComponentModel;
using DevHub.Domain.Enums;

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

    private CloseAction _closeAction = CloseAction.Ask;

    public CloseAction CloseAction
    {
        get => _closeAction;
        set => SetProperty(ref _closeAction, value);
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
