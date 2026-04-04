using System.Windows;
using System.Windows.Input;
using DevHub.Presentation.Services;
using DevHub.Presentation.ViewModels;

namespace DevHub.Presentation;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    public MainWindow(MainViewModel viewModel, WindowService windowService) : this()
    {
        DataContext = viewModel;
        windowService.SetNavigationHost(NavigationHost);
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
            vm.ToggleSidebar();
    }
}
