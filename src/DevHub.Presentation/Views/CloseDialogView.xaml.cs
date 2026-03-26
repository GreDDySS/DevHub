using System.Windows;

namespace DevHub.Presentation.Views;

public partial class CloseDialogView : Window
{
    public bool MinimizeToTray { get; private set; }
    public bool ShouldRemember { get; private set; }

    public CloseDialogView()
    {
        InitializeComponent();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        MinimizeToTray = true;
        ShouldRemember = RememberChoice.IsChecked == true;
        DialogResult = true;
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        MinimizeToTray = false;
        ShouldRemember = RememberChoice.IsChecked == true;
        DialogResult = true;
    }
}
