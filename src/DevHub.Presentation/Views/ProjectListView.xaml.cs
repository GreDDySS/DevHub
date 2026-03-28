using DevHub.Presentation.Attributes;
using DevHub.Presentation.ViewModels;

namespace DevHub.Presentation.Views;

[ViewFor(typeof(ProjectListViewModel))]
[NavigationView("projects")]
public partial class ProjectListView : System.Windows.Controls.UserControl
{
    public ProjectListView()
    {
        InitializeComponent();
    }
}
