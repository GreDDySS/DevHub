using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

    private void ScrollViewer_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var element = e.OriginalSource as DependencyObject;
        while (element != null)
        {
            if (element is FrameworkElement fe && fe.DataContext is ProjectCardViewModel card)
            {
                card.EditCommand.Execute(null);
                e.Handled = true;
                return;
            }
            element = VisualTreeHelper.GetParent(element);
        }
    }
}
