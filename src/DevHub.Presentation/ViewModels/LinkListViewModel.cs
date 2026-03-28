using System.Collections.ObjectModel;
using System.Windows.Threading;
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
    private readonly DispatcherTimer _debounceTimer;

    public LinkListViewModel(ILinkRepository repository)
    {
        _repository = repository;
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _debounceTimer.Tick += async (_, _) =>
        {
            _debounceTimer.Stop();
            await SafeLoadLinksAsync();
        };
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await SafeLoadLinksAsync();
    }

    public override async Task OnNavigatedToAsync()
    {
        await LoadLinksAsync();
    }

    private async Task SafeLoadLinksAsync()
    {
        try
        {
            await LoadLinksAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to load links: {ex.Message}";
        }
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
    private void OpenInBrowser(string url)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
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
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    partial void OnSelectedTypeIndexChanged(int value)
    {
        _ = SafeLoadLinksAsync();
    }
}
