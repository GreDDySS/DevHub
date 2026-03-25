using CommunityToolkit.Mvvm.ComponentModel;

namespace DevHub.Presentation.Base;

public abstract class BaseUserControlViewModel : ViewModelBase
{
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private bool _isVisible = true;

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private double _opacity = 1.0;

    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }
}
