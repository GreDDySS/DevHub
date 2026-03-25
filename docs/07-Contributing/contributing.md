# Contributing

## Добро пожаловать

Этот документ описывает процессы разработки, кодстайл и workflow для проекта DevHub.

## Development Environment

### Требования

| Компонент | Минимальная версия |
|-----------|-------------------|
| .NET SDK | 10.0 |
| Visual Studio | 2026 (или Rider / VS Code + C# Dev Kit) |
| Git | 2.40+ |
| OS | Windows 10 (1809+) / Windows 11 |

### Настройка окружения

```bash
# Клонировать репозиторий
git clone https://github.com/your-username/DevHub.git
cd DevHub

# Восстановить зависимости
dotnet restore

# Собрать решение
dotnet build

# Запустить тесты
dotnet test
```

## Git Workflow

### Branching model

```
main ← стабильный релиз
 │
 ├── develop ← интеграционная ветка
 │    │
 │    ├── feature/FR-001-add-project
 │    ├── feature/FR-201-global-hotkey
 │    └── fix/issue-42-json-corruption
 │
 └── release/v1.0.0 ← подготовка релиза
```

| Тип ветки | Префикс | Пример |
|-----------|---------|--------|
| Feature | `feature/` | `feature/FR-001-add-project` |
| Bugfix | `fix/` | `fix/issue-42-json-corruption` |
| Hotfix | `hotfix/` | `hotfix/critical-data-loss` |
| Release | `release/` | `release/v1.0.0` |

### Commit Convention

Формат: `<type>(<scope>): <description>`

| Тип | Описание |
|-----|----------|
| `feat` | Новая функциональность |
| `fix` | Исправление бага |
| `docs` | Изменения в документации |
| `refactor` | Рефакторинг без изменения функциональности |
| `test` | Добавление/изменение тестов |
| `chore` | Обслуживание (dependencies, CI) |

Примеры:
```
feat(projects): add project filtering by status
fix(storage): handle corrupted JSON gracefully
docs(readme): update installation instructions
refactor(domain): extract ProjectStatus enum
test(projects): add unit tests for AddProjectUseCase
```

### Pull Request Process

1. Создать ветку от `develop`: `feature/FR-XXX-description`
2. Сделать изменения, написать тесты
3. Убедиться что все тесты проходят: `dotnet test`
4. Убедиться что нет ошибок компиляции: `dotnet build`
5. Создать PR в `develop`
6. Заполнить PR-шаблон (автоматически)
7. Дождаться review и approval
8. Squash-merge в `develop`

### PR Template

```markdown
## Описание
<!-- Что делает этот PR -->

## Тип изменения
- [ ] Новая функциональность
- [ ] Исправление бага
- [ ] Рефакторинг
- [ ] Документация

## Checklist
- [ ] Код компилируется без ошибок
- [ ] Все тесты проходят
- [ ] Написаны новые тесты (если применимо)
- [ ] Обновлена документация (если применимо)
- [ ] Нет предупреждений линтера
```

## Code Style

### Общие правила

| Правило | Значение |
|---------|----------|
| Отступы | 4 пробела (spaces) |
| Кодировка | UTF-8 |
| Конец строки | CRLF (Windows) |
| Максимальная длина строки | 120 символов |
| Использование `var` | Когда тип очевиден из контекста |
| Braces | Allman style |

### Naming conventions

| Элемент | Стиль | Пример |
|---------|-------|--------|
| Namespace | PascalCase | `DevHub.Application.UseCases` |
| Class | PascalCase | `AddProjectUseCase` |
| Interface | `I` + PascalCase | `IProjectRepository` |
| Method | PascalCase | `ExecuteAsync` |
| Property | PascalCase | `ProjectName` |
| Field (private) | `_camelCase` | `_repository` |
| Parameter | camelCase | `projectId` |
| Local variable | camelCase | `filteredProjects` |
| Constant | PascalCase | `MaxTagNameLength` |
| Enum member | PascalCase | `ProjectStatus.Active` |

### Файловая структура

- Один класс = один файл
- Имя файла = имя класса: `AddProjectUseCase.cs`
- Namespace соответствует директории

### Асинхронность

- Все I/O-операции — асинхронные (`async/await`)
- Суффикс `Async` для асинхронных методов
- Использовать `Task<T>` вместо `async void` (кроме event handlers)

### Обработка ошибок

```csharp
// Хорошо: конкретные исключения
throw new ValidationException("Project name is required");
throw new NotFoundException($"Project {id} not found");

// Плохо: общие исключения
throw new Exception("Error");
```

## Testing

### Структура тестов

```
tests/
├── DevHub.Domain.Tests/
│   └── Entities/
│       └── ProjectTests.cs
├── DevHub.Application.Tests/
│   └── UseCases/
│       ├── AddProjectUseCaseTests.cs
│       └── GetAllProjectsUseCaseTests.cs
└── DevHub.Infrastructure.Tests/
    └── Storage/
        └── JsonProjectRepositoryTests.cs
```

### Конвенции тестов

- Формат имени: `{MethodName}_Should{Expected}_When{Condition}`
- AAA: Arrange → Act → Assert
- Один assert на тест (допускается несколько связанных)

```csharp
[Fact]
public async Task ExecuteAsync_ShouldCreateProject_WhenValidRequest()
{
    // Arrange
    var request = new CreateProjectRequest("MyProject", "D:\\repos\\MyProject", null, ProgrammingLanguage.CSharp);
    var repository = new InMemoryProjectRepository();
    var useCase = new AddProjectUseCase(repository);

    // Act
    var result = await useCase.ExecuteAsync(request);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("MyProject", result.Name);
    Assert.Equal(ProjectStatus.Active, result.Status);
}
```

### Запуск тестов

```bash
# Все тесты
dotnet test

# Конкретный проект
dotnet test tests/DevHub.Application.Tests

# С фильтром
dotnet test --filter "FullyQualifiedName~AddProject"

# С покрытием (после установки coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

## Логирование

Используется Serilog с двумя sinks:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: Path.Combine(AppPaths.LogsDirectory, "devhub-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();
```

### Уровни логирования

| Уровень | Применение |
|---------|------------|
| `Verbose` | Детали отладки (только в Debug) |
| `Debug` | Отладочная информация |
| `Information` | Нормальные операции (запуск, CRUD) |
| `Warning` | Потенциальные проблемы (файл не найден) |
| `Error` | Ошибки, требующие внимания |
| `Fatal` | Критические ошибки (приложение не может работать) |

## Issue Templates

### Bug Report

```markdown
**Описание**
Краткое описание бага

**Шаги воспроизведения**
1. Открыть...
2. Нажать...
3. Увидеть...

**Ожидаемое поведение**
Что должно было произойти

**Фактическое поведение**
Что произошло

**Окружение**
- OS: Windows 11
- .NET: 10.0
- DevHub version: 1.0.0
```

### Feature Request

```markdown
**Описание фичи**
Что нужно добавить

**Обоснование**
Зачем это нужно

**Предлагаемая реализация**
Как это можно сделать

**Приоритет**
Must / Should / Could
```

## Связанные документы

- [Architecture Overview](../03-Architecture/architecture-overview.md)
- [Tech Stack](../03-Architecture/tech-stack.md)
- [Development Roadmap](../06-Roadmap/development-roadmap.md)
