using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Presentation.Attributes;
using DevHub.Presentation.Base;

namespace DevHub.Presentation.ViewModels;

[SingletonViewModel]
[NavigationView("links")]
public partial class LinkListViewModel : BaseUserControlViewModel
{
    private readonly ILinkRepository _repository;

    public LinkListViewModel(ILinkRepository repository)
    {
        _repository = repository;
        _ = LoadLinksAsync();
    }

    [ObservableProperty]
    private ObservableCollection<Link> _links = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private LinkType? _typeFilter;

    [ObservableProperty]
    private int _totalCount;

    public Array LinkTypes => Enum.GetValues<LinkType>();

    [RelayCommand]
    private async Task LoadLinksAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var links = await _repository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
                links = links.Where(l =>
                    (l.Url?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (l.Title?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();

            if (_typeFilter is not null)
                links = links.Where(l => l.Type == _typeFilter).ToList();

            Links.Clear();
            foreach (var link in links.OrderByDescending(l => l.CapturedAt))
                Links.Add(link);

            TotalCount = Links.Count;
        });
    }

    [RelayCommand]
    private async Task RemoveLink(Guid id)
    {
        await _repository.DeleteAsync(id);
        await LoadLinksAsync();
    }

    [RelayCommand]
    private void CopyUrl(Link link)
    {
        try
        {
            System.Windows.Clipboard.SetText(link.Url);
        }
        catch { }
    }

    partial void OnSearchQueryChanged(string value)
    {
        _ = LoadLinksAsync();
    }

    partial void OnTypeFilterChanged(LinkType? value)
    {
        _ = LoadLinksAsync();
    }
}
