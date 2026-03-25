using CommunityToolkit.Mvvm.ComponentModel;

namespace DevHub.Presentation.Base;

public abstract class ViewModelBase : ObservableValidator
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

    protected async Task<T?> ExecuteWithLoadingAsync<T>(Func<Task<T>> action)
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = null;

            return await action();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            return default;
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
}
