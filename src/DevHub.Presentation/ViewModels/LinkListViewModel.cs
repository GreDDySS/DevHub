using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Presentation.Attributes;
using DevHub.Presentation.Base;
using DevHub.Presentation.Converters;

namespace DevHub.Presentation.ViewModels;

[SingletonViewModel]
[NavigationView("links")]
public partial class LinkListViewModel : BaseUserControlViewModel
{
    private const int DebounceMs = 300;
    private const int MinDomainGroupSize = 5;

    private readonly ILinkRepository _repository;
    private readonly IClipboardService _clipboardService;
    private readonly DispatcherTimer _debounceTimer;

    public LinkListViewModel(ILinkRepository repository, IClipboardService clipboardService)
    {
        _repository = repository;
        _clipboardService = clipboardService;
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(DebounceMs) };
        _debounceTimer.Tick += DebounceTimer_Tick;
    }

    private async void DebounceTimer_Tick(object? sender, EventArgs e)
    {
        _debounceTimer.Stop();
        await SafeLoadLinksAsync();
    }

    public override async Task OnNavigatedToAsync() => await LoadLinksAsync();

    private async Task SafeLoadLinksAsync()
    {
        try { await LoadLinksAsync(); }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to load links: {ex.Message}";
        }
    }

    [ObservableProperty] private ObservableCollection<Link> _links = [];
    [ObservableProperty] private ObservableCollection<LinkFolderViewModel> _folders = [];
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private int _selectedTypeIndex = -1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private ViewMode _viewMode = ViewMode.List;

    [RelayCommand]
    private void ToggleViewMode()
        => ViewMode = ViewMode == ViewMode.List ? ViewMode.Folders : ViewMode.List;

    [RelayCommand]
    private async Task CaptureLinkAsync()
    {
        var text = await _clipboardService.GetTextAsync();
        if (!string.IsNullOrWhiteSpace(text) && Uri.TryCreate(text, UriKind.Absolute, out _))
        {
            var link = new Link
            {
                Id = Guid.NewGuid(),
                Url = text,
                Title = text,
                CapturedAt = DateTime.Now,
                Type = LinkType.Other
            };
            await _repository.AddAsync(link);
            await LoadLinksAsync();
        }
    }

    [RelayCommand]
    private async Task LoadLinksAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var allLinks = await _repository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
                allLinks = allLinks.Where(l =>
                    (l.Url?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (l.Title?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();

            if (SelectedTypeIndex >= 0)
            {
                var filterType = (LinkType)SelectedTypeIndex;
                allLinks = allLinks.Where(l => l.Type == filterType).ToList();
            }

            var sorted = allLinks.OrderByDescending(l => l.CapturedAt).ToList();

            Links.Clear();
            foreach (var link in sorted)
                Links.Add(link);

            BuildFolders(sorted);
            TotalCount = Links.Count;
        });
    }

    private void BuildFolders(List<Link> links)
    {
        Folders.Clear();
        var groups = links.GroupBy(l => l.Type).OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var folderLinks = group.ToList();

            if (group.Key == LinkType.Other)
            {
                var domainGroups = folderLinks
                    .GroupBy(l => ExtractDomain(l.Url))
                    .OrderByDescending(g => g.Count())
                    .ToList();

                foreach (var dg in domainGroups)
                {
                    if (dg.Count() >= MinDomainGroupSize)
                        Folders.Add(new LinkFolderViewModel(dg.Key, dg.ToList()));
                    else
                    {
                        var otherFolder = Folders.FirstOrDefault(f => f.Name == "Other");
                        if (otherFolder == null)
                        {
                            otherFolder = new LinkFolderViewModel("Other", []);
                            Folders.Add(otherFolder);
                        }
                        foreach (var link in dg)
                            otherFolder.Links.Add(link);
                    }
                }
            }
            else
            {
                Folders.Add(new LinkFolderViewModel(group.Key.ToString(), folderLinks));
            }
        }

        foreach (var folder in Folders)
            folder.UpdateCount();
    }

    private static string ExtractDomain(string url)
    {
        try { return new Uri(url).Host; }
        catch { return url; }
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
        try { System.Windows.Clipboard.SetText(link.Url); }
        catch { /* Clipboard may be unavailable */ }
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
        catch { /* URL may be invalid */ }
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

    partial void OnSelectedTypeIndexChanged(int value) => _ = SafeLoadLinksAsync();
}
