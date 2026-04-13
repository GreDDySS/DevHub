using System.Windows;
using DevHub.Domain.Interfaces;
using WpfApplication = System.Windows.Application;

namespace DevHub.Presentation.Services;

public interface IThemeService
{
    bool IsDarkTheme { get; }
    event EventHandler? ThemeChanged;
    void ToggleTheme();
    void SetTheme(bool isDark);
    Task InitializeAsync();
}

public class ThemeService : IThemeService
{
    private readonly IAppSettingsStore _settingsStore;
    private bool _isDarkTheme = true;
    private ResourceDictionary? _darkThemeDict;
    private ResourceDictionary? _lightThemeDict;

    public bool IsDarkTheme => _isDarkTheme;
    public event EventHandler? ThemeChanged;

    public ThemeService(IAppSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        LoadThemeDictionaries();
    }

    public async Task InitializeAsync()
    {
        var settings = await _settingsStore.LoadAsync();
        _isDarkTheme = settings.IsDarkTheme;
        ApplyTheme();
    }

    private void LoadThemeDictionaries()
    {
        _darkThemeDict = new ResourceDictionary();
        _lightThemeDict = new ResourceDictionary();

        // Common brushes
        var successBrush = CreateBrush("#10B981");
        var warningBrush = CreateBrush("#F59E0B");
        var errorBrush = CreateBrush("#EF4444");
        var infoBrush = CreateBrush("#3B82F6");
        var accentBrush = CreateBrush("#19A337");

        // Dark theme colors
        _darkThemeDict["PrimaryBrush"] = CreateBrush("#337A2C");
        _darkThemeDict["PrimaryHoverBrush"] = CreateBrush("#4A9C3D");
        _darkThemeDict["PrimaryPressedBrush"] = CreateBrush("#296621");
        _darkThemeDict["AccentBrush"] = accentBrush;
        _darkThemeDict["SuccessBrush"] = successBrush;
        _darkThemeDict["WarningBrush"] = warningBrush;
        _darkThemeDict["ErrorBrush"] = errorBrush;
        _darkThemeDict["InfoBrush"] = infoBrush;
        _darkThemeDict["TextPrimaryBrush"] = CreateBrush("#FAFAFA");
        _darkThemeDict["TextSecondaryBrush"] = CreateBrush("#A1A1AA");
        _darkThemeDict["TextMutedBrush"] = CreateBrush("#71717A");
        _darkThemeDict["TextInverseBrush"] = CreateBrush("#09090B");
        _darkThemeDict["BgWindowBrush"] = CreateBrush("#09090B");
        _darkThemeDict["BgBaseBrush"] = CreateBrush("#18181B");
        _darkThemeDict["BgSurfaceBrush"] = CreateBrush("#27272A");
        _darkThemeDict["BgSidebarBrush"] = CreateBrush("#0F0F11");
        _darkThemeDict["BgSidebarHoverBrush"] = CreateBrush("#27272A");
        _darkThemeDict["BgSidebarActiveBrush"] = CreateBrush("#337A2C");
        _darkThemeDict["BgCardBrush"] = CreateBrush("#27272A");
        _darkThemeDict["BgCardHoverBrush"] = CreateBrush("#27272A");
        _darkThemeDict["BgInputBrush"] = CreateBrush("#3F3F46");
        _darkThemeDict["BgInputFocusedBrush"] = CreateBrush("#52525B");
        _darkThemeDict["BorderLightBrush"] = CreateBrush("#3F3F46");
        _darkThemeDict["BorderMediumBrush"] = CreateBrush("#52525B");
        _darkThemeDict["BorderFocusBrush"] = CreateBrush("#337A2C");
        _darkThemeDict["PrimaryGradient"] = CreateGradient("#337A2C", "#19A337");

        // Light theme colors
        _lightThemeDict["PrimaryBrush"] = CreateBrush("#337A2C");
        _lightThemeDict["PrimaryHoverBrush"] = CreateBrush("#4A9C3D");
        _lightThemeDict["PrimaryPressedBrush"] = CreateBrush("#296621");
        _lightThemeDict["AccentBrush"] = accentBrush;
        _lightThemeDict["SuccessBrush"] = successBrush;
        _lightThemeDict["WarningBrush"] = warningBrush;
        _lightThemeDict["ErrorBrush"] = errorBrush;
        _lightThemeDict["InfoBrush"] = infoBrush;
        _lightThemeDict["TextPrimaryBrush"] = CreateBrush("#1E293B");
        _lightThemeDict["TextSecondaryBrush"] = CreateBrush("#475569");
        _lightThemeDict["TextMutedBrush"] = CreateBrush("#94A3B8");
        _lightThemeDict["TextInverseBrush"] = CreateBrush("#FFFFFF");
        _lightThemeDict["BgWindowBrush"] = CreateBrush("#FFFFFF");
        _lightThemeDict["BgBaseBrush"] = CreateBrush("#F8FAFC");
        _lightThemeDict["BgSurfaceBrush"] = CreateBrush("#F1F5F9");
        _lightThemeDict["BgSidebarBrush"] = CreateBrush("#F1F5F9");
        _lightThemeDict["BgSidebarHoverBrush"] = CreateBrush("#E2E8F0");
        _lightThemeDict["BgSidebarActiveBrush"] = CreateBrush("#337A2C");
        _lightThemeDict["BgCardBrush"] = CreateBrush("#FFFFFF");
        _lightThemeDict["BgCardHoverBrush"] = CreateBrush("#F1F5F9");
        _lightThemeDict["BgInputBrush"] = CreateBrush("#F1F5F9");
        _lightThemeDict["BgInputFocusedBrush"] = CreateBrush("#E2E8F0");
        _lightThemeDict["BorderLightBrush"] = CreateBrush("#E2E8F0");
        _lightThemeDict["BorderMediumBrush"] = CreateBrush("#CBD5E1");
        _lightThemeDict["BorderFocusBrush"] = CreateBrush("#337A2C");
        _lightThemeDict["PrimaryGradient"] = CreateGradient("#337A2C", "#19A337");
    }

public void ToggleTheme()
    {
        SetTheme(!_isDarkTheme);
    }

    public void SetTheme(bool isDark)
    {
        _isDarkTheme = isDark;
        ApplyTheme();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
        
        _ = SaveThemeAsync();
        
        // Force refresh MainWindow
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var window = System.Windows.Application.Current.MainWindow;
            if (window != null)
            {
                var themeDict = WpfApplication.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Contains("BgWindowBrush"));
                if (themeDict != null)
                {
                    var existingInWindow = window.Resources.MergedDictionaries
                        .FirstOrDefault(d => d.Contains("BgWindowBrush"));
                    if (existingInWindow != null)
                        window.Resources.MergedDictionaries.Remove(existingInWindow);
                    
                    var newDict = new ResourceDictionary();
                    foreach (System.Collections.DictionaryEntry entry in themeDict)
                    {
                        newDict[entry.Key] = entry.Value;
                    }
                    window.Resources.MergedDictionaries.Add(newDict);
                }
                
                window.InvalidateVisual();
                window.UpdateLayout();
            }
            
            if (window?.DataContext is ViewModels.MainViewModel mainVm)
            {
                var windowService = mainVm.GetType()
                    .GetField("_windowService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(mainVm) as WindowService;
                windowService?.RefreshCurrentView();
            }
        });
    }

    private async Task SaveThemeAsync()
    {
        try
        {
            var settings = await _settingsStore.LoadAsync();
            settings.IsDarkTheme = _isDarkTheme;
            await _settingsStore.SaveAsync(settings);
        }
        catch { /* Ignore save errors */ }
    }

    private void ApplyTheme()
    {
        var app = WpfApplication.Current;
        if (app == null) return;

        var resources = app.Resources;

        // Remove old theme dictionary if exists
        var existingTheme = resources.MergedDictionaries
            .FirstOrDefault(d => d.Contains("BgWindowBrush"));
        
        if (existingTheme != null)
        {
            resources.MergedDictionaries.Remove(existingTheme);
        }

        // Add new theme dictionary
        var newTheme = _isDarkTheme ? _darkThemeDict! : _lightThemeDict!;
        resources.MergedDictionaries.Add(newTheme);
    }

    private static System.Windows.Media.SolidColorBrush CreateBrush(string hex)
    {
        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex)!;
        var brush = new System.Windows.Media.SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static System.Windows.Media.LinearGradientBrush CreateGradient(string hex1, string hex2)
    {
        var color1 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex1)!;
        var color2 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex2)!;
        var gradient = new System.Windows.Media.LinearGradientBrush(
            System.Windows.Media.Color.FromRgb(color1.R, color1.G, color1.B),
            System.Windows.Media.Color.FromRgb(color2.R, color2.G, color2.B),
            new System.Windows.Point(0, 0),
            new System.Windows.Point(1, 1));
        gradient.Freeze();
        return gradient;
    }
}