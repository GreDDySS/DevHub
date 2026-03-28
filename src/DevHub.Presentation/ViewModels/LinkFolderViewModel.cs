using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DevHub.Domain.Models;

namespace DevHub.Presentation.ViewModels;

public partial class LinkFolderViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private ObservableCollection<Link> _links = [];

    [ObservableProperty]
    private int _count;

    [ObservableProperty]
    private bool _isExpanded;

    public LinkFolderViewModel(string name, List<Link> links)
    {
        Name = name;
        _links = new ObservableCollection<Link>(links);
        Count = links.Count;
    }

    public void UpdateCount()
    {
        Count = Links.Count;
    }
}
