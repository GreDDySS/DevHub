using CommunityToolkit.Mvvm.ComponentModel;

namespace DevHub.Presentation.Base;

public abstract class ViewModelBase : ObservableValidator, IDisposable
{
    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _hasError;

    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    private readonly List<IDisposable> _disposables = [];

    protected void TrackDisposable(IDisposable disposable) => _disposables.Add(disposable);

    protected async Task ExecuteWithLoadingAsync(Func<Task> action)
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = null;

            await action();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
            disposable.Dispose();
        _disposables.Clear();
        OnDispose();
        GC.SuppressFinalize(this);
    }

    protected virtual void OnDispose() { }
}
