namespace DevHub.Presentation.Base;

public abstract class BaseUserControlViewModel : ViewModelBase
{
    public virtual Task OnNavigatedToAsync() => Task.CompletedTask;
}
