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
    private readonly System.Threading.Timer _debounceTimer;

    public LinkListViewModel(ILinkRepository repository)
    {
        _repository = repository;
        _debounceTimer = new System.Threading.Timer(_ => _ = LoadLinksAsync(), null, Timeout.Infinite, Timeout.Infinite);
        _ = LoadLinksAsync();
    }

    [ObservableProperty]
    private ObservableCollection<Link> _links = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private int _selectedTypeIndex = -1;

    [ObservableProperty]
    private int _totalCount;

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

            if (SelectedTypeIndex >= 0)
            {
                var filterType = (LinkType)SelectedTypeIndex;
                links = links.Where(l => l.Type == filterType).ToList();
            }

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

    [RelayCommand]
    private void ResetFilters()
    {
        SearchQuery = string.Empty;
        SelectedTypeIndex = -1;
    }

    partial void OnSearchQueryChanged(string value)
    {
        _debounceTimer.Change(300, Timeout.Infinite);
    }

    partial void OnSelectedTypeIndexChanged(int value)
    {
        _ = LoadLinksAsync();
    }
}
