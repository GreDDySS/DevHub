using Microsoft.Win32;

namespace DevHub.Presentation.Services;

public class AutostartService : IAutostartService
{
    private const string AppName = "DevHub";
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public bool IsEnabled
    {
        get
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            return key?.GetValue(AppName) is not null;
        }
    }

    public void Enable()
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath)) return;

        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);
        key?.SetValue(AppName, $"\"{exePath}\"");
    }

    public void Disable()
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);
        key?.DeleteValue(AppName, throwOnMissingValue: false);
    }

    public void SetEnabled(bool enabled)
    {
        if (enabled) Enable();
        else Disable();
    }
}
