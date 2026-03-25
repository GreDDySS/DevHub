using DevHub.Presentation.Attributes;
using DevHub.Presentation.ViewModels;

namespace DevHub.Presentation.Views;

[ViewFor(typeof(SettingsViewModel))]
[NavigationView("settings")]
public partial class SettingsView : System.Windows.Controls.UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }
}
