using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace DevHub.Presentation.Services;

public interface IHotkeyService
{
    void Register(int id, uint modifiers, uint key);
    void Unregister(int id);
    event Action<int>? HotkeyPressed;
}

public partial class WindowsHotkeyService : IHotkeyService, IDisposable
{
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint VK_Y = 0x59;

    private nint _hwnd;
    private HwndSource? _source;
    private readonly Dictionary<int, bool> _registeredIds = [];

    public event Action<int>? HotkeyPressed;

    public void Initialize(nint hwnd)
    {
        _hwnd = hwnd;
        _source = HwndSource.FromHwnd(hwnd);
        _source?.AddHook(WndProc);
    }

    public void Register(int id, uint modifiers, uint key)
    {
        if (RegisterHotKey(_hwnd, id, modifiers, key))
            _registeredIds[id] = true;
    }

    public void Unregister(int id)
    {
        if (_registeredIds.Remove(id))
            UnregisterHotKey(_hwnd, id);
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        if (msg == WM_HOTKEY)
        {
            HotkeyPressed?.Invoke(wParam.ToInt32());
            handled = true;
        }
        return nint.Zero;
    }

    public void Dispose()
    {
        foreach (var id in _registeredIds.Keys.ToList())
            Unregister(id);
        _source?.RemoveHook(WndProc);
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(nint hWnd, int id);
}
