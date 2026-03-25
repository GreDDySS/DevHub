# Architecture Overview

## Обзор

DevHub построен на основе **MVVM + Clean Architecture**. Это обеспечивает:

- Разделение ответственности между слоями
- Тестируемость бизнес-логики без UI
- Лёгкую замену инфраструктурных компонентов

## Архитектурные слои

```
┌─────────────────────────────────────────────────────┐
│                    Presentation                      │
│         (WPF Views, ViewModels, Controls)           │
├─────────────────────────────────────────────────────┤
│                   Application                       │
│        (Use Cases, Services, DTOs, Interfaces)      │
├─────────────────────────────────────────────────────┤
│                     Domain                          │
│          (Entities, Value Objects, Rules)           │
├─────────────────────────────────────────────────────┤
│                  Infrastructure                     │
│    (JSON Storage, File System, Hotkey Service)      │
└─────────────────────────────────────────────────────┘
```

## Правила зависимостей

```
Presentation → Application → Domain ← Infrastructure
```

- **Domain** — не зависит ни от какого другого слоя
- **Application** — зависит только от Domain
- **Presentation** — зависит от Application и Domain
- **Infrastructure** — зависит от Domain (реализует интерфейсы из Application)

## Структура решения

```
DevHub.sln
│
├── src/
│   ├── DevHub.Domain/              # Доменный слой
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
│   ├── DevHub.Application/         # Слой применения
│   │   ├── UseCases/
│   │   │   ├── Projects/
│   │   │   │   ├── GetAllProjectsUseCase.cs
│   │   │   │   ├── AddProjectUseCase.cs
│   │   │   │   ├── UpdateProjectUseCase.cs
│   │   │   │   └── DeleteProjectUseCase.cs
│   │   │   └── Links/
│   │   │       ├── CaptureLinkUseCase.cs
│   │   │       └── GetAllLinksUseCase.cs
│   │   ├── Services/
│   │   │   └── ProjectService.cs
│   │   ├── DTOs/
│   │   │   ├── ProjectDto.cs
│   │   │   └── LinkDto.cs
│   │   └── Interfaces/
│   │       ├── IFileDialogService.cs
│   │       ├── IProcessLauncher.cs
│   │       ├── IHotkeyService.cs
│   │       └── IWindowService.cs
│   │
│   ├── DevHub.Infrastructure/       # Инфраструктурный слой
│   │   ├── Storage/
│   │   │   ├── JsonProjectRepository.cs
│   │   │   ├── JsonLinkRepository.cs
│   │   │   └── JsonFileStore.cs
│   │   ├── Services/
│   │   │   ├── WindowsHotkeyService.cs
│   │   │   ├── ProcessLauncher.cs
│   │   │   ├── FileDialogService.cs
│   │   │   └── WindowService.cs
│   │   └── Configuration/
│   │       └── AppSettings.cs
│   │
│   └── DevHub.Presentation/        # Слой представления (WPF)
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
│       │   └── SettingsViewModel.cs
│       ├── Views/
│       │   ├── MainWindow.xaml
│       │   ├── ProjectListView.xaml
│       │   └── SettingsView.xaml
│       ├── Controls/
│       │   └── ProjectCard.xaml
│       ├── Converters/
│       └── App.xaml
│
└── tests/
    ├── DevHub.Domain.Tests/
    ├── DevHub.Application.Tests/
    └── DevHub.Infrastructure.Tests/
```

## Ключевые паттерны

| Паттерн | Применение |
|---------|------------|
| **Repository** | Абстракция доступа к данным (IProjectRepository) |
| **Use Case** | Инкапсуляция бизнес-операций (AddProjectUseCase) |
| **Dependency Injection** | Внедрение зависимостей через Microsoft.Extensions.DependencyInjection |
| **Command** | MVVM-команды (RelayCommand / CommunityToolkit) |
| **Observer** | INotifyPropertyChanged, ObservableCollection |
| **Factory** | Создание сущностей со значениями по умолчанию |

## Взаимодействие слоев — пример

```
User clicks "Add Project" button
        │
        ▼
Presentation: MainViewModel.AddProjectCommand
        │
        ▼
Application: AddProjectUseCase.Execute(dto)
        │  ├── Validates input
        │  ├── Maps DTO → Domain entity
        │  └── Calls repository
        ▼
Infrastructure: JsonProjectRepository.Add(project)
        │  └── Writes to projects.json
        ▼
Presentation: ObservableCollection<ProjectCardViewModel> updated
        │
        ▼
UI: New card appears in list
```

## DI Container — регистрация

```csharp
// App.xaml.cs
var factory = new ViewFactoryService(registry, null!);

// ViewModels — автоскан и регистрация (Singleton/Transient)
factory.RegisterAll(services, typeof(App).Assembly);

// Сервисы — вручную
services.AddSingleton<IWindowService, WindowService>();
services.AddSingleton<IProjectRepository, JsonProjectRepository>();
services.AddSingleton<ILinkRepository, JsonLinkRepository>();
services.AddSingleton<IProcessLauncher, ProcessLauncher>();

// Use Cases
services.AddTransient<AddProjectUseCase>();
services.AddTransient<GetAllProjectsUseCase>();
services.AddTransient<DeleteProjectUseCase>();
services.AddTransient<CaptureLinkUseCase>();
```

## Связанные документы

- [Base Classes & Services](base-classes-and-services.md)
- [Data Model](data-model.md)
- [Tech Stack](tech-stack.md)
- [Database Schema](../04-Design/database-schema.md)
