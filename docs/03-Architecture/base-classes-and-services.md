# Base Classes & Services

## Обзор

Базовые классы и сервисы обеспечивают переиспользуемую инфраструктуру для всего приложения. Они инкапсулируют повторяющуюся логику MVVM, уведомления об ошибках и управление окнами.

## Базовые классы

### Иерархия

```
ObservableObject (CommunityToolkit.Mvvm)
    │
    ├── BaseModel
    │       ├── Project
    │       ├── Link
    │       └── Tag
    │
    └── ViewModelBase
            ├── BaseUserControlViewModel
            │       ├── ProjectCardViewModel
            │       └── LinkCardViewModel
            │
            └── BaseWindowViewModel
                    ├── MainViewModel
                    ├── SettingsViewModel
                    └── ProjectDetailViewModel
```

---

### BaseModel

Базовый класс для всех доменных сущностей. Инкапсулирует общие поля и логику сравнения.

```csharp
public abstract class BaseModel : ObservableObject, IEquatable<BaseModel>
{
    private Guid _id = Guid.NewGuid();

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private DateTime _createdAt = DateTime.UtcNow;

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }

    private DateTime _updatedAt = DateTime.UtcNow;

    public DateTime UpdatedAt
    {
        get => _updatedAt;
        set => SetProperty(ref _updatedAt, value);
    }

    public void MarkUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
        OnPropertyChanged(nameof(UpdatedAt));
    }

    public bool Equals(BaseModel? other)
        => other is not null && Id == other.Id;

    public override int GetHashCode()
        => Id.GetHashCode();

    public override bool Equals(object? obj)
        => obj is BaseModel other && Equals(other);
}
```

**Ответственность:**

| Функция | Описание |
|---------|----------|
| `Id` | Уникальный идентификатор (Guid) |
| `CreatedAt` | Дата создания сущности |
| `UpdatedAt` | Дата последнего обновления |
| `MarkUpdated()` | Утилита для обновления `UpdatedAt` |
| `IEquatable` | Сравнение сущностей по `Id` |

**Пример наследования:**

```csharp
public class Project : BaseModel
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _path = string.Empty;

    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    // ... остальные свойства
}
```

---

### ViewModelBase

Базовый класс для всех ViewModel. Содержит общую логику валидации, загрузки и обработки ошибок.

```csharp
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
            Log.Error(ex, "Error in {ViewModel}", GetType().Name);
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
            Log.Error(ex, "Error in {ViewModel}", GetType().Name);
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
```

**Ответственность:**

| Свойство/Метод | Описание |
|----------------|----------|
| `IsLoading` | Флаг загрузки (для binding `ProgressBar` / `Spinner`) |
| `ErrorMessage` | Текст ошибки для отображения |
| `HasError` | Флаг наличия ошибки |
| `ExecuteWithLoadingAsync` | Обёртка для асинхронных операций с обработкой ошибок |
| `ClearError()` | Сброс состояния ошибки |

---

### BaseUserControlViewModel

Базовый класс для ViewModel, привязанных к `UserControl` (карточки, элементы списка). Не содержит логики навигации или управления окном.

```csharp
public abstract class BaseUserControlViewModel : ViewModelBase
{
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private bool _isVisible = true;

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private double _opacity = 1.0;

    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }
}
```

**Ответственность:**

| Свойство | Описание |
|----------|----------|
| `IsSelected` | Выделен ли элемент (для `DataGrid`, `ListBox`) |
| `IsVisible` | Видимость элемента |
| `Opacity` | Прозрачность (для анимаций) |

**Пример наследования:**

```csharp
public partial class ProjectCardViewModel : BaseUserControlViewModel
{
    private readonly Project _project;

    public string Name => _project.Name;
    public string Language => _project.Language.ToString();
    public string Status => _project.Status.ToString();

    [RelayCommand]
    private void OpenInExplorer()
    {
        // ...
    }
}
```

---

### BaseWindowViewModel

Базовый класс для ViewModel, привязанных к `Window` (главное окно, диалоги, настройки). Содержит логику управления окном.

```csharp
public abstract class BaseWindowViewModel : ViewModelBase
{
    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private double _width = 1024;

    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    private double _height = 768;

    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    private WindowState _windowState = WindowState.Normal;

    public WindowState WindowState
    {
        get => _windowState;
        set => SetProperty(ref _windowState, value);
    }

    public virtual void OnWindowLoaded()
    {
        // Override in derived classes
    }

    public virtual void OnWindowClosing(CancelEventArgs e)
    {
        // Override in derived classes
    }

    public virtual void OnWindowClosed()
    {
        // Override in derived classes
    }
}
```

**Ответственность:**

| Свойство/Метод | Описание |
|----------------|----------|
| `Title` | Заголовок окна |
| `Width` / `Height` | Размер окна |
| `WindowState` | Состояние окна (Normal, Maximized, Minimized) |
| `OnWindowLoaded()` | Хук после загрузки окна |
| `OnWindowClosing()` | Хук перед закрытием (можно отменить) |
| `OnWindowClosed()` | Хук после закрытия |

**Пример наследования:**

```csharp
public partial class MainViewModel : BaseWindowViewModel
{
    public MainViewModel()
    {
        Title = "DevHub";
        Width = 1200;
        Height = 800;
    }

    public override void OnWindowLoaded()
    {
        LoadProjectsCommand.ExecuteAsync(null);
    }

    public override void OnWindowClosing(CancelEventArgs e)
    {
        if (minimizeToTray)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
        }
    }
}
```

---

## Сервисы

### Атрибуты

Атрибуты позволяют декларативно связать View с ViewModel и задать метаданные. Сервис окон сканирует ассемблии и автоматически регистрирует все отмеченные классы.

#### ViewForAttribute

Указывает, для какого ViewModel предназначен данный View.

```csharp
/// <summary>
/// Связывает View с конкретным ViewModel.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ViewForAttribute : Attribute
{
    public Type ViewModelType { get; }

    public ViewForAttribute(Type viewModelType)
    {
        ViewModelType = viewModelType;
    }
}
```

**Применение:**

```csharp
[ViewFor(typeof(MainViewModel))]
public partial class MainWindow : Window { }

[ViewFor(typeof(SettingsViewModel))]
public partial class SettingsWindow : Window { }

[ViewFor(typeof(ProjectCardViewModel))]
public partial class ProjectCard : UserControl { }
```

#### NavigationViewAttribute

Помечает View как страницу для навигации (ленивая загрузка).

```csharp
/// <summary>
/// Помечает View как навигационную страницу.
/// Страницы не загружаются до первой навигации.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class NavigationViewAttribute : Attribute
{
    public string Key { get; }

    public NavigationViewAttribute(string key)
    {
        Key = key;
    }
}
```

**Применение:**

```csharp
[ViewFor(typeof(ProjectListViewModel))]
[NavigationView("projects")]
public partial class ProjectListView : UserControl { }

[ViewFor(typeof(LinkListViewModel))]
[NavigationView("links")]
public partial class LinkListView : UserControl { }

[ViewFor(typeof(SettingsViewModel))]
[NavigationView("settings")]
public partial class SettingsView : UserControl { }
```

#### SingletonViewAttribute

Указывает, что экземпляр View/ViewModel должен быть единственным (не пересоздаваться).

```csharp
/// <summary>
/// View/ViewModel будет создан один раз и переиспользоваться.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class SingletonViewAttribute : Attribute { }
```

**Применение:**

```csharp
[ViewFor(typeof(MainViewModel))]
[SingletonView]
public partial class MainWindow : Window { }
```

---

### ViewRegistry

Реестр, который автоматически сканирует ассемблии и строит карту ViewModel → View. Запускается один раз при старте приложения.

```csharp
public class ViewRegistry
{
    private readonly Dictionary<Type, ViewRegistration> _viewModelToView = new();
    private readonly Dictionary<string, ViewRegistration> _navigationViews = new();

    /// <summary>
    /// Сканирует указанные ассемблии и регистрирует все View с атрибутами.
    /// </summary>
    public void ScanAndRegister(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var viewTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => typeof(Window).IsAssignableFrom(t)
                         || typeof(UserControl).IsAssignableFrom(t));

            foreach (var viewType in viewTypes)
            {
                var viewForAttr = viewType.GetCustomAttribute<ViewForAttribute>();
                if (viewForAttr is null) continue;

                var registration = new ViewRegistration
                {
                    ViewType = viewType,
                    ViewModelType = viewForAttr.ViewModelType,
                    IsNavigation = viewType.GetCustomAttribute<NavigationViewAttribute>() is NavigationViewAttribute navAttr,
                    NavigationKey = viewType.GetCustomAttribute<NavigationViewAttribute>()?.Key,
                    IsSingleton = viewType.GetCustomAttribute<SingletonViewAttribute>() is not null
                };

                _viewModelToView[viewForAttr.ViewModelType] = registration;

                if (registration.IsNavigation && registration.NavigationKey is not null)
                {
                    _navigationViews[registration.NavigationKey] = registration;
                }
            }
        }
    }

    public ViewRegistration? GetByViewModel<TViewModel>() where TViewModel : ViewModelBase
        => GetByViewModel(typeof(TViewModel));

    public ViewRegistration? GetByViewModel(Type viewModelType)
        => _viewModelToView.GetValueOrDefault(viewModelType);

    public ViewRegistration? GetByKey(string key)
        => _navigationViews.GetValueOrDefault(key);

    public IReadOnlyDictionary<string, ViewRegistration> GetAllNavigationViews()
        => _navigationViews;
}

public class ViewRegistration
{
    public required Type ViewType { get; init; }
    public required Type ViewModelType { get; init; }
    public bool IsNavigation { get; init; }
    public string? NavigationKey { get; init; }
    public bool IsSingleton { get; init; }
}
```

---

### IWindowService

Абстракция для управления окнами и навигацией. Сервис автоматически находит нужный View по ViewModel через `ViewRegistry`.

```csharp
public interface IWindowService
{
    // Навигация (для UserControl-страниц)
    void NavigateTo(string key);
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    object? GetCurrentView();

    // Окна и диалоги
    bool? ShowDialog<TViewModel>() where TViewModel : ViewModelBase;
    bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
    void Show<TViewModel>() where TViewModel : ViewModelBase;
    void Show<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;

    // Утилиты
    bool Confirm(string title, string message);
    string? OpenFolderDialog(string initialDirectory = "");
    string? OpenFileDialog(string filter = "All files (*.*)|*.*", string initialDirectory = "");
    void ShowNotification(string title, string message);

    // Окно
    void CloseWindow(object viewModel);
    void MinimizeToTray();
    void RestoreFromTray();
}
```

---

### WindowService — реализация

Ключевые особенности:
- **Автоматическое сканирование** — View регистрируются по атрибутам, ручная регистрация не нужна
- **Ленивая загрузка** — View создаются только при первом обращении
- **Singleton-поддержка** — отмеченные View создаются один раз
- **Навигация** — переключение между UserControl-страницами через `ContentControl`

```csharp
public class WindowService : IWindowService
{
    private readonly ViewRegistry _registry;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, object> _singletonCache = new();
    private NotifyIcon? _trayIcon;

    public WindowService(ViewRegistry registry, IServiceProvider serviceProvider)
    {
        _registry = registry;
        _serviceProvider = serviceProvider;
        InitializeTrayIcon();
    }

    // ═══════════════════════════════════════
    //  НАВИГАЦИЯ
    // ═══════════════════════════════════════

    private ContentControl? _navigationHost;

    /// <summary>
    /// Привязывает ContentControl как хост для навигации.
    /// Вызывается один раз при загрузке MainWindow.
    /// </summary>
    public void SetNavigationHost(ContentControl host)
    {
        _navigationHost = host;
    }

    public void NavigateTo(string key)
    {
        var registration = _registry.GetByKey(key)
            ?? throw new InvalidOperationException($"Navigation view '{key}' not found");

        var view = ResolveView(registration);
        _navigationHost!.Content = view;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var registration = _registry.GetByViewModel<TViewModel>()
            ?? throw new InvalidOperationException($"View for {typeof(TViewModel).Name} not found");

        var view = ResolveView(registration);
        _navigationHost!.Content = view;
    }

    public object? GetCurrentView()
        => _navigationHost?.Content;

    // ═══════════════════════════════════════
    //  ОКНА И ДИАЛОГИ
    // ═══════════════════════════════════════

    public bool? ShowDialog<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        return ShowDialog(viewModel);
    }

    public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        var window = CreateWindow(viewModel);
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        window.Owner = Application.Current.MainWindow;
        return window.ShowDialog();
    }

    public void Show<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        Show(viewModel);
    }

    public void Show<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        var window = CreateWindow(viewModel);
        window.Show();
    }

    // ═══════════════════════════════════════
    //  УТИЛИТЫ
    // ═══════════════════════════════════════

    public bool Confirm(string title, string message)
    {
        return MessageBox.Show(message, title,
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public string? OpenFolderDialog(string initialDirectory = "")
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Выберите папку",
            InitialDirectory = initialDirectory
        };
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    public string? OpenFileDialog(string filter = "All files (*.*)|*.*", string initialDirectory = "")
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            InitialDirectory = initialDirectory
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public void ShowNotification(string title, string message)
    {
        _trayIcon?.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
    }

    public void CloseWindow(object viewModel)
    {
        Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.DataContext == viewModel)
            ?.Close();
    }

    public void MinimizeToTray()
    {
        if (Application.Current.MainWindow is { } main)
        {
            main.Hide();
            _trayIcon!.Visible = true;
        }
    }

    public void RestoreFromTray()
    {
        if (Application.Current.MainWindow is { } main)
        {
            main.Show();
            main.WindowState = WindowState.Normal;
            main.Activate();
            _trayIcon!.Visible = false;
        }
    }

    // ═══════════════════════════════════════
    //  ВНУТРЕННИЕ МЕТОДЫ
    // ═══════════════════════════════════════

    private Window CreateWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        var registration = _registry.GetByViewModel(viewModel.GetType())
            ?? throw new InvalidOperationException($"View for {viewModel.GetType().Name} not registered");

        var window = (Window)Activator.CreateInstance(registration.ViewType)!;
        window.DataContext = viewModel;

        if (viewModel is BaseWindowViewModel vm)
        {
            window.Loaded += (_, _) => vm.OnWindowLoaded();
            window.Closing += (_, e) => vm.OnWindowClosing(e);
            window.Closed += (_, _) => vm.OnWindowClosed();
        }

        return window;
    }

    private object ResolveView(ViewRegistration registration)
    {
        // Singleton — возвращаем из кэша
        if (registration.IsSingleton)
        {
            if (_singletonCache.TryGetValue(registration.ViewType, out var cached))
                return cached;

            var singletonView = CreateViewWithViewModel(registration);
            _singletonCache[registration.ViewType] = singletonView;
            return singletonView;
        }

        // Обычный — создаём новый экземпляр
        return CreateViewWithViewModel(registration);
    }

    private object CreateViewWithViewModel(ViewRegistration registration)
    {
        var view = Activator.CreateInstance(registration.ViewType)!;
        var viewModel = _serviceProvider.GetRequiredService(registration.ViewModelType);
        ((FrameworkElement)view).DataContext = viewModel;
        return view;
    }

    private void InitializeTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = new Icon("Assets/devhub.ico"),
            Text = "DevHub",
            Visible = false
        };

        _trayIcon.Click += (_, _) => RestoreFromTray();

        var menu = new ContextMenuStrip();
        menu.Items.Add("Открыть", null, (_, _) => RestoreFromTray());
        menu.Items.Add("-");
        menu.Items.Add("Выход", null, (_, _) => Application.Current.Shutdown());
        _trayIcon.ContextMenuStrip = menu;
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}
```

---

### IViewFactoryService

Сервис автоматически сканирует ассембли, находит все ViewModel, регистрирует их в DI-контейнере и связывает с View. Не нужно писать `services.AddTransient<>()` для каждого класса.

```csharp
public interface IViewFactoryService
{
    void RegisterAll(IServiceCollection services, Assembly assembly);
    FrameworkElement GetView<TViewModel>() where TViewModel : ViewModelBase;
    Window GetWindow<TViewModel>() where TViewModel : ViewModelBase;
}
```

### ViewFactoryService — реализация

```csharp
public class ViewFactoryService : IViewFactoryService
{
    private readonly ViewRegistry _registry;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, object> _singletonCache = new();

    public ViewFactoryService(ViewRegistry registry, IServiceProvider serviceProvider)
    {
        _registry = registry;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Автоматически находит все ViewModel в ассембли и регистрирует их в DI.
    /// Определяет Singleton/Transient по атрибуту [SingletonViewModel].
    /// Связывает View с ViewModel через naming convention: ProjectListViewModel → ProjectListView.
    /// </summary>
    public void RegisterAll(IServiceCollection services, Assembly assembly)
    {
        var baseTypesToExclude = new[]
        {
            typeof(ViewModelBase),
            typeof(BaseUserControlViewModel),
            typeof(BaseWindowViewModel)
        };

        var viewModels = assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => typeof(ViewModelBase).IsAssignableFrom(t))
            .Where(t => !baseTypesToExclude.Contains(t))
            .OrderByDescending(t => typeof(BaseWindowViewModel).IsAssignableFrom(t))
            .ToList();

        foreach (var vmType in viewModels)
        {
            var isSingleton = vmType.GetCustomAttribute<SingletonViewModelAttribute>() is not null;

            if (isSingleton)
                services.AddSingleton(vmType);
            else
                services.AddTransient(vmType);

            // Поиск View по naming convention
            var viewTypeName = vmType.FullName?
                .Replace("ViewModels.", "Views.")
                .Replace("ViewModel", "View");

            var viewType = assembly.GetType(viewTypeName);

            if (viewType is null) continue;

            // Регистрация в ViewRegistry
            _registry.Register(new ViewRegistration
            {
                ViewType = viewType,
                ViewModelType = vmType,
                IsNavigation = viewType.GetCustomAttribute<NavigationViewAttribute>() is not null,
                NavigationKey = viewType.GetCustomAttribute<NavigationViewAttribute>()?.Key,
                IsSingleton = isSingleton
            });
        }
    }

    public FrameworkElement GetView<TViewModel>() where TViewModel : ViewModelBase
    {
        var registration = _registry.GetByViewModel<TViewModel>()
            ?? throw new InvalidOperationException($"View for {typeof(TViewModel).Name} not found");

        return ResolveView(registration);
    }

    public Window GetWindow<TViewModel>() where TViewModel : ViewModelBase
    {
        var registration = _registry.GetByViewModel<TViewModel>()
            ?? throw new InvalidOperationException($"View for {typeof(TViewModel).Name} not found");

        var viewModel = ResolveViewModel(registration);
        var window = (Window)Activator.CreateInstance(registration.ViewType)!;
        window.DataContext = viewModel;

        if (viewModel is BaseWindowViewModel vm)
        {
            window.Loaded += (_, _) => vm.OnWindowLoaded();
            window.Closing += (_, e) => vm.OnWindowClosing(e);
            window.Closed += (_, _) => vm.OnWindowClosed();
        }

        return window;
    }

    private FrameworkElement ResolveView(ViewRegistration registration)
    {
        var viewModel = ResolveViewModel(registration);
        var view = (FrameworkElement)Activator.CreateInstance(registration.ViewType)!;
        view.DataContext = viewModel;
        return view;
    }

    private ViewModelBase ResolveViewModel(ViewRegistration registration)
    {
        if (registration.IsSingleton)
        {
            if (_singletonCache.TryGetValue(registration.ViewModelType, out var cached))
                return (ViewModelBase)cached;

            var vm = (ViewModelBase)_serviceProvider.GetRequiredService(registration.ViewModelType);
            _singletonCache[registration.ViewModelType] = vm;
            return vm;
        }

        return (ViewModelBase)_serviceProvider.GetRequiredService(registration.ViewModelType);
    }
}
```

---

### SingletonViewModelAttribute

Указывает, что ViewModel должна быть зарегистрирована как Singleton (один экземпляр на всё время жизни приложения).

```csharp
/// <summary>
/// ViewModel будет зарегистрирована как Singleton в DI.
/// Без этого атрибута — Transient (новый экземпляр при каждом запросе).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class SingletonViewModelAttribute : Attribute { }
```

**Применение:**

```csharp
// Transient — каждый раз новый экземпляр (по умолчанию)
public partial class ProjectCardViewModel : BaseUserControlViewModel { }

// Singleton — один экземпляр на всё приложение
[SingletonViewModel]
public partial class MainViewModel : BaseWindowViewModel { }

[SingletonViewModel]
public partial class ProjectListViewModel : BaseUserControlViewModel { }
```

**Когда Singleton, когда Transient:**

| Тип | Когда использовать | Примеры |
|-----|---------------------|---------|
| **Singleton** | Главное окно, списки, навигационные страницы | `MainViewModel`, `ProjectListViewModel`, `SettingsViewModel` |
| **Transient** | Карточки, диалоги, элементы списка | `ProjectCardViewModel`, `LinkCardViewModel`, `AddProjectDialogViewModel` |

---

### ViewRegistry — обновлённый

```csharp
public class ViewRegistry
{
    private readonly Dictionary<Type, ViewRegistration> _viewModelToView = new();
    private readonly Dictionary<string, ViewRegistration> _navigationViews = new();

    public void Register(ViewRegistration registration)
    {
        _viewModelToView[registration.ViewModelType] = registration;

        if (registration.IsNavigation && registration.NavigationKey is not null)
        {
            _navigationViews[registration.NavigationKey] = registration;
        }
    }

    public ViewRegistration? GetByViewModel<TViewModel>() where TViewModel : ViewModelBase
        => GetByViewModel(typeof(TViewModel));

    public ViewRegistration? GetByViewModel(Type viewModelType)
        => _viewModelToView.GetValueOrDefault(viewModelType);

    public ViewRegistration? GetByKey(string key)
        => _navigationViews.GetValueOrDefault(key);

    public IReadOnlyDictionary<string, ViewRegistration> GetAllNavigationViews()
        => _navigationViews;
}
```

---

### Naming Convention

View ищется автоматически по имени ViewModel:

| ViewModel | View (автоматически) |
|-----------|---------------------|
| `MainViewModel` | `MainWindow` |
| `ProjectListViewModel` | `ProjectListView` |
| `ProjectCardViewModel` | `ProjectCardView` |
| `SettingsViewModel` | `SettingsView` |
| `AddProjectDialogViewModel` | `AddProjectDialogView` |

```
Namespace: DevHub.Presentation.ViewModels.ProjectListViewModel
           ↓ заменяем
Namespace: DevHub.Presentation.Views.ProjectListView
```

Если View не найден по naming convention — используется атрибут `[ViewFor]`.

---

### Настройка при старте

Полностью автоматическая регистрация. Руками ничего писать не нужно.

```csharp
// App.xaml.cs
public partial class App : Application
{
    public static IHost Host { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        // ── Сервисы (вручную, т.к. это не ViewModel) ──
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<IProcessLauncher, ProcessLauncher>();
        services.AddSingleton<IProjectRepository, JsonProjectRepository>();
        services.AddSingleton<ILinkRepository, JsonLinkRepository>();

        // ── Use Cases ──
        services.AddTransient<AddProjectUseCase>();
        services.AddTransient<GetAllProjectsUseCase>();
        services.AddTransient<DeleteProjectUseCase>();
        services.AddTransient<CaptureLinkUseCase>();

        // ── Сборка провайдера ──
        Host = services.BuildServiceProvider();

        // ── Реестр View + фабрика ──
        var registry = new ViewRegistry();
        var factory = new ViewFactoryService(registry, Host.Services);
        factory.RegisterAll(services, typeof(App).Assembly);

        // ── Запуск главного окна ──
        var window = factory.GetWindow<MainViewModel>();
        window.Show();
    }
}
```

**Что происходит при запуске:**

```
1. ScanAndRegister находит:
   ├── MainViewModel          → MainWindow          [Singleton]
   ├── ProjectListViewModel   → ProjectListView     [Singleton]
   ├── ProjectCardViewModel   → ProjectCardView     [Transient]
   ├── LinkListViewModel      → LinkListView        [Singleton]
   ├── SettingsViewModel      → SettingsView        [Singleton]
   └── ProjectDetailViewModel → ProjectDetailView   [Transient]

2. DI-контейнер автоматически регистрирует все найденные ViewModel

3. ViewFactoryService.GetWindow<MainViewModel>() создаёт:
   └── MainWindow с DataContext = MainViewModel

4. MainWindow.Show()
```

---

### Использование в ViewModel

#### Навигация между страницами

```csharp
[SingletonViewModel]
public partial class MainViewModel : BaseWindowViewModel
{
    private readonly IWindowService _windowService;

    public MainViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        Title = "DevHub";
    }

    [RelayCommand]
    private void GoToProjects() => _windowService.NavigateTo("projects");

    [RelayCommand]
    private void GoToLinks() => _windowService.NavigateTo("links");

    [RelayCommand]
    private void GoToSettings() => _windowService.NavigateTo("settings");
}
```

#### Карточка (Transient — создаётся для каждого элемента)

```csharp
// Нет атрибута [SingletonViewModel] → Transient
public partial class ProjectCardViewModel : BaseUserControlViewModel
{
    public ProjectCardViewModel(Project project)
    {
        Name = project.Name;
        Language = project.Language.ToString();
        Status = project.Status.ToString();
    }

    [ObservableProperty] private string _name;
    [ObservableProperty] private string _language;
    [ObservableProperty] private string _status;
}
```

#### Страница списка (Singleton — один экземпляр)

```csharp
[SingletonViewModel]
public partial class ProjectListViewModel : BaseUserControlViewModel
{
    private readonly IProjectRepository _repository;

    public ProjectListViewModel(IProjectRepository repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        var projects = await _repository.GetAllAsync();
        Projects.Clear();
        foreach (var p in projects)
            Projects.Add(new ProjectCardViewModel(p)); // Transient
    }
}
```

#### View — без code-behind

```xml
<!-- ProjectListView.xaml — ничего в code-behind не нужно -->
<UserControl x:Class="DevHub.Presentation.Views.ProjectListView">
    <Grid>
        <TextBox Text="{Binding SearchQuery, UpdateSourceTrigger=PropertyChanged}" />
        <ItemsControl ItemsSource="{Binding Projects}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <!-- DataContext = ProjectCardViewModel (Transient) -->
                    <local:ProjectCardView />
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </Grid>
</UserControl>
```

```csharp
// ProjectListView.xaml.cs — пустой
public partial class ProjectListView : UserControl
{
    public ProjectListView()
    {
        InitializeComponent();
        // DataContext привязывается автоматически через ViewFactoryService
    }
}
```

---

### Сводка атрибутов

| Атрибут | Где | Что делает |
|---------|-----|------------|
| `[ViewFor(typeof(MainViewModel))]` | View | Ручная привязка View → ViewModel (если naming convention не подходит) |
| `[NavigationView("projects")]` | View | Помечает View как навигационную страницу |
| `[SingletonView]` | View | View создаётся один раз |
| `[SingletonViewModel]` | ViewModel | Регистрация в DI как Singleton (иначе Transient) |

---

## Пример полной навигационной страницы

```csharp
// Views/ProjectListView.xaml.cs
[ViewFor(typeof(ProjectListViewModel))]
[NavigationView("projects")]
[SingletonView]
public partial class ProjectListView : UserControl
{
    public ProjectListView()
    {
        InitializeComponent();
    }
}
```

```xml
<!-- Views/ProjectListView.xaml -->
<UserControl x:Class="DevHub.Presentation.Views.ProjectListView">
    <Grid>
        <TextBox Text="{Binding SearchQuery, UpdateSourceTrigger=PropertyChanged}"
                 PlaceholderText="Поиск проектов..." />
        <ItemsControl ItemsSource="{Binding Projects}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <local:ProjectCardView />
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </Grid>
</UserControl>
```

---

## Структура решения (обновлённая)

```
DevHub.sln
│
├── src/
│   ├── DevHub.Domain/
│   │   ├── Models/
│   │   │   ├── BaseModel.cs
│   │   │   ├── Project.cs
│   │   │   ├── Link.cs
│   │   │   └── Tag.cs
│   │   ├── Enums/
│   │   │   ├── ProjectStatus.cs
│   │   │   ├── ProgrammingLanguage.cs
│   │   │   └── LinkType.cs
│   │   └── Interfaces/
│   │       ├── IProjectRepository.cs
│   │       └── ILinkRepository.cs
│   │
│   ├── DevHub.Application/
│   │   ├── UseCases/
│   │   │   ├── Projects/
│   │   │   └── Links/
│   │   ├── DTOs/
│   │   └── Interfaces/
│   │       ├── IWindowService.cs
│   │       ├── IProcessLauncher.cs
│   │       └── IHotkeyService.cs
│   │
│   ├── DevHub.Infrastructure/
│   │   ├── Storage/
│   │   │   ├── JsonProjectRepository.cs
│   │   │   └── JsonFileStore.cs
│   │   └── Services/
│   │       ├── WindowService.cs
│   │       ├── WindowsHotkeyService.cs
│   │       └── ProcessLauncher.cs
│   │
│   └── DevHub.Presentation/
│       ├── Attributes/
│       │   ├── ViewForAttribute.cs
│       │   ├── NavigationViewAttribute.cs
│       │   ├── SingletonViewAttribute.cs
│       │   └── SingletonViewModelAttribute.cs
│       ├── Base/
│       │   ├── ViewModelBase.cs
│       │   ├── BaseUserControlViewModel.cs
│       │   └── BaseWindowViewModel.cs
│       ├── Registry/
│       │   ├── ViewRegistry.cs
│       │   ├── ViewRegistration.cs
│       │   └── ViewFactoryService.cs
│       ├── ViewModels/
│       │   ├── MainViewModel.cs
│       │   ├── ProjectListViewModel.cs
│       │   ├── ProjectCardViewModel.cs
│       │   ├── LinkListViewModel.cs
│       │   └── SettingsViewModel.cs
│       ├── Views/
│       │   ├── MainWindow.xaml
│       │   ├── ProjectListView.xaml
│       │   ├── LinkListView.xaml
│       │   └── SettingsView.xaml
│       ├── Controls/
│       │   └── ProjectCard.xaml
│       ├── Converters/
│       └── App.xaml
│
└── tests/
```

---

## Связанные документы

- [Architecture Overview](architecture-overview.md)
- [Data Model](data-model.md)
- [Tech Stack](tech-stack.md)
