# Global Hotkey Implementation

## Обзор

Модуль глобальной горячей клавиши позволяет захватывать ссылки из буфера обмена по сочетанию `Ctrl+Shift+Y` из любого приложения Windows.

## Технический подход

### Варианты реализации

| Подход | Библиотека | Плюсы | Минусы |
|--------|------------|-------|--------|
| Win32 API | P/Invoke `RegisterHotKey` | Без зависимостей, нативный | Низкоуровневый, boilerplate |
| NHotkey | NHotkey.Wpf | Простой API, интеграция с WPF | Доп. зависимость |
| SharpHook | SharpHook | Глобальный мониторинг клавиш | Overkill для одной горячей клавиши |

**Рекомендация:** Win32 P/Invoke — минимум зависимостей, полный контроль.

### Выбранный подход: Win32 P/Invoke + обёртка

Причина: минимум зависимостей, полный контроль, .NET 10 хорошо работает с P/Invoke.

## Архитектура

```
┌─────────────────────────────────────────────┐
│              Presentation                    │
│  MainViewModel                              │
│      │                                      │
│      ├── subscribes to LinkCaptured event    │
│      └── calls CaptureLinkUseCase            │
├─────────────────────────────────────────────┤
│              Application                     │
│  CaptureLinkUseCase                         │
│      │                                      │
│      ├── reads clipboard                    │
│      ├── validates URL                      │
│      ├── creates Link entity                │
│      └── saves via ILinkRepository          │
├─────────────────────────────────────────────┤
│              Infrastructure                  │
│  WindowsHotkeyService : IHotkeyService      │
│      │                                      │
│      ├── RegisterHotKey (P/Invoke)          │
│      ├── WndProc hook (HwndSource)          │
│      └── fires HotkeyPressed event          │
└─────────────────────────────────────────────┘
```

## IHotkeyService — интерфейс

```csharp
public interface IHotkeyService : IDisposable
{
    event EventHandler? HotkeyPressed;
    void Register(ModifierKeys modifiers, Key key);
    void Unregister();
}
```

## WindowsHotkeyService — реализация

```csharp
public class WindowsHotkeyService : IHotkeyService
{
    private const int HOTKEY_ID = 9000;
    private const int WM_HOTKEY = 0x0312;
    
    private HwndSource? _source;
    private nint _handle;

    public event EventHandler? HotkeyPressed;

    public void Register(ModifierKeys modifiers, Key key)
    {
        _handle = GetMainWindowHandle();
        _source = HwndSource.FromHwnd(_handle);
        _source?.AddHook(WndProc);

        RegisterHotKey(_handle, HOTKEY_ID, (uint)modifiers, (uint)KeyInterop.VirtualKeyFromKey(key));
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam == HOTKEY_ID)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }
        return nint.Zero;
    }

    public void Unregister()
    {
        UnregisterHotKey(_handle, HOTKEY_ID);
        _source?.RemoveHook(WndProc);
    }

    public void Dispose() => Unregister();

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(nint hWnd, int id);
}
```

## CaptureLinkUseCase

```csharp
public class CaptureLinkUseCase
{
    private readonly ILinkRepository _linkRepository;
    private readonly IClipboardService _clipboard;

    public async Task<Link?> ExecuteAsync()
    {
        var text = await _clipboard.GetTextAsync();
        
        if (!IsValidUrl(text))
            return null;

        var link = new Link
        {
            Id = Guid.NewGuid(),
            Url = text,
            Type = DetectLinkType(text),
            CapturedAt = DateTime.UtcNow,
            Tags = new List<string>()
        };

        await _linkRepository.AddAsync(link);
        return link;
    }

    private static bool IsValidUrl(string? text)
    {
        return Uri.TryCreate(text, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static LinkType DetectLinkType(string url)
    {
        if (url.Contains("youtube.com") || url.Contains("youtu.be"))
            return LinkType.YouTube;
        if (url.Contains("github.com") || url.Contains("gitlab.com"))
            return LinkType.Repository;
        
        return LinkType.Other;
    }
}
```

## Поток данных

```
Нажатие Ctrl+Shift+Y (глобально)
        │
        ▼
WindowsHotkeyService.WndProc
        │
        ▼
HotkeyPressed event
        │
        ▼
MainViewModel.OnHotkeyPressed()
        │
        ▼
CaptureLinkUseCase.ExecuteAsync()
        │  ├── Читает буфер обмена
        │  ├── Валидирует URL
        │  ├── Определяет тип ссылки
        │  └── Сохраняет в links.json
        ▼
Уведомление (toast): "Ссылка сохранена!"
        │
        ▼
LinksCollectionView обновляется
```

## Уведомления (Toast)

```csharp
// Использование Windows Toast Notification API
new ToastContentBuilder()
    .AddText("DevHub")
    .AddText($"Ссылка сохранена: {link.Title ?? link.Url}")
    .Show();
```

Требует NuGet-пакет: `Microsoft.Toolkit.Uwp.Notifications` (для WPF на Windows 10/11).

## Обработка ошибок

| Ситуация | Поведение |
|----------|-----------|
| Буфер пуст | Молча игнорировать |
| Буфер не содержит URL | Молча игнорировать |
| URL уже сохранён | Дубликат не создаётся, показать уведомление |
| Ошибка записи JSON | Log error, показать уведомление об ошибке |
| Горячая клавиша уже занята | Уведомить пользователя, предложить другую |

## Настройка горячей клавиши

В MVP — фиксированная комбинация `Ctrl+Shift+Y`.

Phase 2 — настраиваемая в settings.json:

```json
{
  "hotkey": {
    "captureLink": {
      "modifiers": ["Ctrl", "Shift"],
      "key": "Y"
    }
  }
}
```

## Зависимости

| Пакет | Назначение |
|-------|------------|
| Microsoft.Toolkit.Uwp.Notifications | Toast-уведомления |
| P/Invoke (user32.dll) | Регистрация горячей клавиши |

## Связанные документы

- [Functional Requirements](../02-Requirements/functional-requirements.md) — FR-201..FR-205
- [User Stories](../02-Requirements/user-stories.md) — US-008, US-009
- [Project Management Module](project-management-module.md)
