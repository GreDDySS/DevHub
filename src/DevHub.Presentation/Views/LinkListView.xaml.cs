using DevHub.Presentation.Attributes;
using DevHub.Presentation.ViewModels;

namespace DevHub.Presentation.Views;

[ViewFor(typeof(LinkListViewModel))]
[NavigationView("links")]
public partial class LinkListView : System.Windows.Controls.UserControl
{
    public LinkListView()
    {
        InitializeComponent();
    }
}
