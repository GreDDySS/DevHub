using System.Windows;
using DevHub.Presentation.Attributes;
using DevHub.Presentation.ViewModels;

namespace DevHub.Presentation.Views;

[NavigationView("settings")]
public partial class SettingsView : System.Windows.Controls.UserControl
{
    public SettingsView() => InitializeComponent();

    private void IdeRadioButton_Checked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel vm) return;
        if (sender is not System.Windows.Controls.RadioButton rb) return;

        var parent = System.Windows.Media.VisualTreeHelper.GetParent(rb);
        while (parent != null && parent is not System.Windows.Controls.ContentPresenter)
            parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);

        if (parent is not System.Windows.Controls.ContentPresenter presenter) return;
        var itemsControl = FindName("IdeItemsControl") as System.Windows.Controls.ItemsControl;
        if (itemsControl == null) return;
        var index = itemsControl.ItemContainerGenerator.IndexFromContainer(presenter);
        if (index >= 0)
            vm.SelectedIdeIndex = index;
    }
}
