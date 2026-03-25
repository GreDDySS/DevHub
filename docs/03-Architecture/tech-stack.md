# Tech Stack

## Основной стек

| Компонент | Технология | Версия | Назначение |
|-----------|------------|--------|------------|
| **Платформа** | .NET | 10 | Runtime и SDK |
| **Язык** | C# | 14 | Основной язык разработки |
| **UI Framework** | WPF | (в составе .NET) | Графический интерфейс |
| **UI Library** | HandyControl | 3.x | Fluent/Material компоненты |
| **MVVM Toolkit** | CommunityToolkit.Mvvm | 8.x | MVVM infrastructure |
| **DI Container** | Microsoft.Extensions.DependencyInjection | 9.x | Внедрение зависимостей |
| **Logging** | Microsoft.Extensions.Logging + Serilog | — | Структурированное логирование |
| **JSON** | System.Text.Json | (в составе .NET) | Сериализация данных |
| **Testing** | xUnit + Moq | — | Unit-тестирование |
| **Installer** | Inno Setup / MSIX | — | Установщик |

## Почему именно этот стек

### .NET 10 + WPF

| Критерий | Обоснование |
|----------|-------------|
| Актуальность | .NET 10 — текущая версия (LTS-цикл) |
| WPF | Лучший выбор для rich desktop UI на Windows |
| Производительность | Native AOT, минимальный startup time |
| Экосистема | Огромная библиотека NuGet-пакетов |

### HandyControl

- Современные Fluent-style компоненты
- Лёгкая альтернатива Material Design In XAML Toolkit
- Хорошая документация и активное сообщество
- TreeView, DataGrid, Navigation, Theme switching

### CommunityToolkit.Mvvm

- `[ObservableProperty]` — автогенерация INPC
- `[RelayCommand]` — автогенерация ICommand
- `ObservableObject` — базовый класс для ViewModel
- Минимизирует boilerplate-код

### System.Text.Json

- Встроен в .NET, без внешних зависимостей
- Высокая производительность
- Source generators для AOT-совместимости

## Структура NuGet-зависимостей

```
DevHub.Domain
  └── (нет внешних зависимостей)

DevHub.Application
  ├── CommunityToolkit.Mvvm
  └── Microsoft.Extensions.Logging.Abstractions

DevHub.Infrastructure
  ├── Microsoft.Extensions.Logging
  ├── Serilog.Sinks.File
  └── Serilog.Sinks.Console

DevHub.Presentation
  ├── HandyControl
  ├── CommunityToolkit.Mvvm
  ├── Microsoft.Extensions.DependencyInjection
  └── Serilog
```

## Инструменты разработки

| Инструмент | Назначение |
|------------|------------|
| Visual Studio 2026 | Основная IDE |
| VS Code | Редактор документации и конфигов |
| Git + GitHub | Система контроля версий |
| GitHub Actions | CI/CD |
| dotnet CLI | Сборка и запуск тестов |

## Связанные документы

- [Architecture Overview](architecture-overview.md)
- [Data Model](data-model.md)
- [Non-Functional Requirements](../02-Requirements/non-functional-requirements.md)
