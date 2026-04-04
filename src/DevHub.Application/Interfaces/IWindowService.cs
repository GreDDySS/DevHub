namespace DevHub.Application.Interfaces;

public interface IWindowService
{
    void NavigateTo(string key);
    object? GetCurrentView();

    bool? ShowDialog(System.Type viewModelType);
    bool? ShowDialog(System.Type viewModelType, System.Action<object> configure);
    void Show(System.Type viewModelType);

    bool Confirm(string title, string message);
    string? OpenFolderDialog(string initialDirectory = "");
    string? OpenFileDialog(string filter = "All files (*.*)|*.*", string initialDirectory = "");
    void ShowNotification(string title, string message);

    void CloseWindow(object viewModel);
    void MinimizeToTray();
    void RestoreFromTray();
}
