using System.ComponentModel;
using System.Windows;

namespace DevHub.Presentation.Base;

public abstract class BaseWindowViewModel : ViewModelBase
{
    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private double _width = 1024;

    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    private double _height = 768;

    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    private WindowState _windowState = WindowState.Normal;

    public WindowState WindowState
    {
        get => _windowState;
        set => SetProperty(ref _windowState, value);
    }

    public virtual void OnWindowLoaded()
    {
    }

    public virtual void OnWindowClosing(CancelEventArgs e)
    {
    }

    public virtual void OnWindowClosed()
    {
    }
}
