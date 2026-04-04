using System.Windows;
using DevHub.Presentation.Services;
using DevHub.Presentation.ViewModels;

namespace DevHub.Presentation;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel viewModel, WindowService windowService) : this()
    {
        DataContext = viewModel;
        windowService.SetNavigationHost(NavigationHost);
    }
}
