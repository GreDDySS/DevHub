using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DevHub.Presentation.Services;
using DevHub.Presentation.ViewModels;

namespace DevHub.Presentation;

public partial class MainWindow : Window
{
    private ThemeService? _themeService;

    public MainWindow() => InitializeComponent();

    public MainWindow(MainViewModel viewModel, WindowService windowService, ThemeService themeService) : this()
    {
        DataContext = viewModel;
        windowService.SetNavigationHost(NavigationHost);
        _themeService = themeService;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            ToggleMaximize();
        else if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();
    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void ToggleMaximize() =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.ToggleSidebar();
            
            var chevron = FindName("ChevronPath") as Path;
            if (chevron?.RenderTransform is RotateTransform rt)
            {
                var animation = new DoubleAnimation
                {
                    To = vm.ShowFullLogo ? 0 : 180,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                rt.BeginAnimation(RotateTransform.AngleProperty, animation);
            }
        }
    }

    private void ToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        if (_themeService != null)
        {
            _themeService.ToggleTheme();
        }
    }

    }