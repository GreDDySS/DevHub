namespace DevHub.Presentation.Services;

public interface IAutostartService
{
    bool IsEnabled { get; }
    void SetEnabled(bool enabled);
}
