# Implementation Plan

Пошаговый план реализации DevHub. Каждый шаг — законченная рабочая единица.

---

## Шаг 1 — Инициализация проекта

**Цель:** Готовая solution-структура, которая собирается и запускается.

**Выход:** Пустое WPF-окно с заголовком "DevHub".

| # | Действие | Файлы |
|---|----------|-------|
| 1.1 | `dotnet new sln -n DevHub` | `DevHub.sln` |
| 1.2 | `dotnet new wpf -n DevHub.Presentation -f net10.0` | `src/DevHub.Presentation/` |
| 1.3 | `dotnet new classlib -n DevHub.Domain -f net10.0` | `src/DevHub.Domain/` |
| 1.4 | `dotnet new classlib -n DevHub.Application -f net10.0` | `src/DevHub.Application/` |
| 1.5 | `dotnet new classlib -n DevHub.Infrastructure -f net10.0` | `src/DevHub.Infrastructure/` |
| 1.6 | `dotnet new xunit -n DevHub.Application.Tests` | `tests/DevHub.Application.Tests/` |
| 1.7 | Добавить ссылки между проектами | `.csproj` файлы |
| 1.8 | NuGet: CommunityToolkit.Mvvm, HandyControl, Serilog | `DevHub.Presentation.csproj` |
| 1.9 | Настроить `MainWindow.xaml` — заголовок, размер | `MainWindow.xaml` |
| 1.10 | Проверка: `dotnet build`, `dotnet run` | — |

**Критерий готовности:**
- `dotnet build` без ошибок
- `dotnet run` открывает окно "DevHub"
- `dotnet test` проходит (пустые тесты)

---

## Шаг 2 — Базовые классы и атрибуты

**Цель:** Фундамент MVVM-инфраструктуры.

| # | Действие | Файлы |
|---|----------|-------|
| 2.1 | Создать `BaseModel` в DevHub.Domain | `Models/BaseModel.cs` |
| 2.2 | Создать `ViewModelBase` в Presentation | `Base/ViewModelBase.cs` |
| 2.3 | Создать `BaseUserControlViewModel` | `Base/BaseUserControlViewModel.cs` |
| 2.4 | Создать `BaseWindowViewModel` | `Base/BaseWindowViewModel.cs` |
| 2.5 | Создать `ViewForAttribute` | `Attributes/ViewForAttribute.cs` |
| 2.6 | Создать `NavigationViewAttribute` | `Attributes/NavigationViewAttribute.cs` |
| 2.7 | Создать `SingletonViewAttribute` | `Attributes/SingletonViewAttribute.cs` |
| 2.8 | Создать `SingletonViewModelAttribute` | `Attributes/SingletonViewModelAttribute.cs` |
| 2.9 | Создать `ViewRegistry` + `ViewRegistration` | `Registry/ViewRegistry.cs` |
| 2.10 | Создать `ViewFactoryService` | `Registry/ViewFactoryService.cs` |

**Критерий готовности:**
- Все классы компилируются
- Написаны unit-тесты для ViewRegistry и ViewFactoryService

---

## Шаг 3 — Сервис окон и навигация

**Цель:** Рабочая навигация между страницами без code-behind.

| # | Действие | Файлы |
|---|----------|-------|
| 3.1 | Создать `IWindowService` в Application | `Interfaces/IWindowService.cs` |
| 3.2 | Создать `WindowService` в Infrastructure | `Services/WindowService.cs` |
| 3.3 | Реализовать навигацию (`NavigateTo`, `SetNavigationHost`) | — |
| 3.4 | Реализовать диалоги (`ShowDialog`, `Confirm`) | — |
| 3.5 | Создать пустые View: `MainWindow`, `ProjectListView`, `SettingsView` | `Views/` |
| 3.6 | Создать пустые ViewModel: `MainViewModel`, `ProjectListViewModel`, `SettingsViewModel` | `ViewModels/` |
| 3.7 | Настроить `MainWindow.xaml` — Sidebar + ContentControl | `MainWindow.xaml` |
| 3.8 | Настроить `App.xaml.cs` — автоматическая регистрация | `App.xaml.cs` |
| 3.9 | Проверить навигацию между страницами | — |

**Критерий готовности:**
- Кнопки в Sidebar переключают страницы
- Нет code-behind для привязки DataContext
- ViewFactoryService автоматически регистрирует все ViewModel

---

## Шаг 4 — Domain: сущности и enums

**Цель:** Доменная модель готова к использованию.

| # | Действие | Файлы |
|---|----------|-------|
| 4.1 | Создать `ProjectStatus` enum | `Enums/ProjectStatus.cs` |
| 4.2 | Создать `ProgrammingLanguage` enum | `Enums/ProgrammingLanguage.cs` |
| 4.3 | Создать `LinkType` enum | `Enums/LinkType.cs` |
| 4.4 | Создать `Project : BaseModel` | `Models/Project.cs` |
| 4.5 | Создать `Link : BaseModel` | `Models/Link.cs` |
| 4.6 | Создать `Tag` | `Models/Tag.cs` |
| 4.7 | Создать интерфейсы: `IProjectRepository`, `ILinkRepository` | `Interfaces/` |
| 4.8 | Создать `IClipboardService`, `IProcessLauncher` | `Interfaces/` |
| 4.9 | Unit-тесты для моделей | `tests/DevHub.Domain.Tests/` |

**Критерий готовности:**
- Все сущности наследуют BaseModel
- Все enum-ы определены
- Все интерфейсы задекларированы

---

## Шаг 5 — JSON-хранилище

**Цель:** Данные сохраняются и загружаются из JSON-файлов.

| # | Действие | Файлы |
|---|----------|-------|
| 5.1 | Создать `JsonFileStore` — базовый класс для JSON I/O | `Storage/JsonFileStore.cs` |
| 5.2 | Создать `JsonProjectRepository` | `Storage/JsonProjectRepository.cs` |
| 5.3 | Создать `JsonLinkRepository` | `Storage/JsonLinkRepository.cs` |
| 5.4 | Определить путь: `%AppData%/DevHub/` | `Configuration/AppPaths.cs` |
| 5.5 | Автосоздание директории при первом запуске | — |
| 5.6 | Unit-тесты с in-memory реализациями | `tests/` |

**Критерий готовности:**
- `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` работают
- Данные переживают перезапуск приложения
- JSON-файлы создаются автоматически

---

## Шаг 6 — Use Cases

**Цель:** Бизнес-логика CRUD для проектов.

| # | Действие | Файлы |
|---|----------|-------|
| 6.1 | Создать DTO: `ProjectDto`, `CreateProjectRequest`, `UpdateProjectRequest`, `ProjectFilter` | `DTOs/` |
| 6.2 | Создать `GetAllProjectsUseCase` | `UseCases/Projects/` |
| 6.3 | Создать `AddProjectUseCase` с валидацией | `UseCases/Projects/` |
| 6.4 | Создать `UpdateProjectUseCase` | `UseCases/Projects/` |
| 6.5 | Создать `DeleteProjectUseCase` | `UseCases/Projects/` |
| 6.6 | Unit-тесты для каждого Use Case | `tests/DevHub.Application.Tests/` |

**Критерий готовности:**
- Каждый Use Case покрыт тестами
- Валидация работает (пустое имя, несуществующий путь)

---

## Шаг 7 — UI: Project List

**Цель:** Экран списка проектов с поиском и фильтрацией.

| # | Действие | Файлы |
|---|----------|-------|
| 7.1 | Создать `ProjectListViewModel` с командами | `ViewModels/ProjectListViewModel.cs` |
| 7.2 | Создать `ProjectCardViewModel` (Transient) | `ViewModels/ProjectCardViewModel.cs` |
| 7.3 | Создать `ProjectListView.xaml` | `Views/ProjectListView.xaml` |
| 7.4 | Создать `ProjectCardView.xaml` (UserControl) | `Controls/ProjectCardView.xaml` |
| 7.5 | Привязать поиск к `SearchQuery` с debounce | — |
| 7.6 | Привязать фильтр по статусу | — |
| 7.7 | Стилизация карточек (цвета статусов) | XAML resources |

**Критерий готовности:**
- Проекты отображаются в списке
- Поиск фильтрует в реальном времени
- Фильтр по статусу работает

---

## Шаг 8 — UI: Add/Edit Project

**Цель:** Форма добавления и редактирования проекта.

| # | Действие | Файлы |
|---|----------|-------|
| 8.1 | Создать `AddProjectViewModel` | `ViewModels/AddProjectViewModel.cs` |
| 8.2 | Создать `AddProjectView.xaml` (Window) | `Views/AddProjectView.xaml` |
| 8.3 | Поля: название, путь (с диалогом), язык, описание | XAML |
| 8.4 | Валидация полей в UI | — |
| 8.5 | Кнопка «Сохранить» → `AddProjectUseCase` | — |
| 8.6 | Режим редактирования (предзаполнение полей) | — |

**Критерий готовности:**
- Форма открывается по кнопке «Добавить»
- Проект сохраняется в JSON
- После закрытия формы список обновляется

---

## Шаг 9 — Explorer/IDE Launcher

**Цель:** Открытие папки и IDE одной кнопкой.

| # | Действие | Файлы |
|---|----------|-------|
| 9.1 | Создать `ProcessLauncher : IProcessLauncher` | `Services/ProcessLauncher.cs` |
| 9.2 | Реализовать `OpenInExplorer` | — |
| 9.3 | Реализовать `OpenInIde` | — |
| 9.4 | Добавить кнопки на карточку проекта | `ProjectCardView.xaml` |
| 9.5 | Обработка ошибок (путь не найден, IDE не настроена) | — |

**Критерий готовности:**
- Кнопка Explorer открывает папку
- Кнопка IDE запускает выбранную IDE
- Ошибки показывают уведомление

---

## Шаг 10 — Settings

**Цель:** Страница настроек с IDE-путями.

| # | Действие | Файлы |
|---|----------|-------|
| 10.1 | Создать `SettingsViewModel` | `ViewModels/SettingsViewModel.cs` |
| 10.2 | Создать `SettingsView.xaml` | `Views/SettingsView.xaml` |
| 10.3 | Секция IDE: пути к VS Code, VS, Rider | XAML |
| 10.4 | Валидация путей (файл существует) | — |
| 10.5 | Сохранение в `settings.json` | — |
| 10.6 | Загрузка настроек при запуске | — |

**Критерий готовности:**
- Настройки сохраняются и загружаются
- Пути к IDE валидируются
- `settings.json` создаётся автоматически

---

## Шаг 11 — Tray + Autostart (Phase 2)

**Цель:** Сворачивание в трей и автозапуск.

| # | Действие | Файлы |
|---|----------|-------|
| 11.1 | Добавить трей-иконку в `WindowService` | — |
| 11.2 | Обработка закрытия окна → сворачивание в трей | — |
| 11.3 | Контекстное меню трей-иконки | — |
| 11.4 | Настройка «Запускать с Windows» (реестр) | — |
| 11.5 | Тумблер в Settings | `SettingsView.xaml` |

---

## Шаг 12 — Global Hotkey + Link Capture (Phase 2)

**Цель:** Захват ссылок по Ctrl+Shift+Y.

| # | Действие | Файлы |
|---|----------|-------|
| 12.1 | Создать `IHotkeyService` | `Interfaces/IHotkeyService.cs` |
| 12.2 | Создать `WindowsHotkeyService` (P/Invoke) | `Services/WindowsHotkeyService.cs` |
| 12.3 | Создать `IClipboardService` | `Interfaces/IClipboardService.cs` |
| 12.4 | Создать `CaptureLinkUseCase` | `UseCases/Links/` |
| 12.5 | Toast-уведомления | — |
| 12.6 | UI: страница ссылок | `Views/LinkListView.xaml` |

---

## Шаг 13 — Финальная полировка

| # | Действие |
|---|----------|
| 13.1 | Интеграционные тесты |
| 13.2 | Обработка edge cases |
| 13.3 | Логирование всех операций |
| 13.4 | Обновление документации |
| 13.5 | Code review |

---

## Итоговая последовательность

```
Шаг 1  ─→ Шаг 2  ─→ Шаг 3  ─→ Шаг 4  ─→ Шаг 5
Init      Base      Window    Domain    Storage
Classes   Service   Model     JSON

Шаг 6  ─→ Шаг 7  ─→ Шаг 8  ─→ Шаг 9  ─→ Шаг 10
Use       Project   Add/Edit  Explorer  Settings
Cases     List      Form      IDE

Шаг 11 ─→ Шаг 12 ─→ Шаг 13
Tray      Hotkey    Polish
Autostart Links
```

**MVP готов после Шага 10.**
Phase 2 готов после Шага 12.

## Связанные документы

- [Development Roadmap](development-roadmap.md)
- [Architecture Overview](../03-Architecture/architecture-overview.md)
- [Base Classes & Services](../03-Architecture/base-classes-and-services.md)
