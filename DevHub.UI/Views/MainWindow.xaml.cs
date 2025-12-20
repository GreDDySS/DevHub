using DevHub.UI.ViewModels;
using System.Windows;

namespace DevHub.UI.Views
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