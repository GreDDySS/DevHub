using DevHub.Shell.ViewModels;
using System.Windows;

namespace DevHub.Shell.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}