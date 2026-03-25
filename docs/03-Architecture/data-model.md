# Data Model

## Обзор

Все данные хранятся в JSON-файлах в директории `%AppData%/DevHub/`.

## Доменные сущности

### Project

```csharp
public class Project : BaseModel
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Description { get; set; }
    public string Notes { get; set; }
    public ProgrammingLanguage Language { get; set; }
    public ProjectStatus Status { get; set; }
    public List<string> Tags { get; set; }
    public string PreferredIde { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}
```

### Link

```csharp
public class Link : BaseModel
{
    public string Url { get; set; }
    public string Title { get; set; }
    public LinkType Type { get; set; }
    public Guid? ProjectId { get; set; }
    public List<string> Tags { get; set; }
    public string Notes { get; set; }
    public DateTime CapturedAt { get; set; }
}
```

### Tag

```csharp
public class Tag
{
    public string Name { get; set; }
    public string Color { get; set; }
    public string Category { get; set; }
}
```

## Value Objects

### ProjectStatus

```csharp
public enum ProjectStatus
{
    Active,
    Completed,
    Paused,
    Archived
}
```

### ProgrammingLanguage

```csharp
public enum ProgrammingLanguage
{
    CSharp,
    Python,
    Rust,
    JavaScript,
    TypeScript,
    Go,
    Java,
    Cpp,
    Other
}
```

### LinkType

```csharp
public enum LinkType
{
    YouTube,
    Article,
    Repository,
    Documentation,
    Other
}
```

## Связи между сущностями

```
┌──────────┐       ┌──────────┐
│  Project │1─────*│   Link   │
│          │       │          │
│ Id       │       │ Id       │
│ Name     │       │ Url      │
│ ...      │       │ ...      │
└──────────┘       └──────────┘
     │                    |
     │                    │
     │*                   │*
     ▼                    ▼
┌──────────┐       ┌──────────┐
│   Tag    │       │   Tag    │
│ (inline) │       │ (inline) │
└──────────┘       └──────────┘
```

Теги хранятся **inline** (как `List<string>`) в каждой сущности. Отдельная таблица тегов не используется для упрощения JSON-структуры.

## Сервисы (интерфейсы)

```csharp
public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync();
    Task<Project?> GetByIdAsync(Guid id);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Guid id);
    Task<List<Project>> SearchAsync(string query);
}

public interface ILinkRepository
{
    Task<List<Link>> GetAllAsync();
    Task<Link?> GetByIdAsync(Guid id);
    Task<List<Link>> GetByProjectIdAsync(Guid projectId);
    Task AddAsync(Link link);
    Task DeleteAsync(Guid id);
    Task<List<Link>> SearchAsync(string query);
}

public interface IClipboardService
{
    Task<string?> GetTextAsync();
    Task SetTextAsync(string text);
    bool ContainsUrl();
}

public interface IProcessLauncher
{
    void OpenInExplorer(string path);
    void OpenInIde(string idePath, string projectPath);
}
```

## Связанные документы

- [Architecture Overview](architecture-overview.md)
- [Database Schema](../04-Design/database-schema.md)
- [Functional Requirements](../02-Requirements/functional-requirements.md)
