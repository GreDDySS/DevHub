# DevHub

Personal development center — desktop application for managing projects, links, and IDE configurations.

## Features

- **Project Management** — organize, search, filter projects with status tracking
- **Link Capture** — capture URLs from clipboard with Ctrl+Shift+Y
- **IDE Launcher** — open projects in VS Code, Rider, Visual Studio, or any custom IDE
- **System Tray** — minimize to tray, autostart with Windows
- **Settings** — dynamic IDE configuration with auto-detection

## Tech Stack

- .NET 10, WPF
- CommunityToolkit.Mvvm (MVVM)
- HandyControl (UI)
- Serilog (logging)
- System.Text.Json (storage)

## Architecture

```
DevHub.sln
├── src/
│   ├── DevHub.Presentation/    (WPF, ViewModels, Services)
│   ├── DevHub.Application/     (Use Cases, DTOs, Interfaces)
│   ├── DevHub.Domain/          (Models, Enums, Interfaces)
│   └── DevHub.Infrastructure/  (JSON storage, ProcessLauncher, Services)
└── tests/
    ├── DevHub.Domain.Tests/        (Model tests, InMemory repos)
    ├── DevHub.Application.Tests/   (ViewRegistry tests)
    ├── DevHub.Presentation.Tests/  (ViewFactoryService tests)
    └── DevHub.Integration.Tests/   (Use Case integration tests)
```

## Build & Run

```bash
dotnet build
dotnet run --project src/DevHub.Presentation
```

## Data Storage

All data stored in `%AppData%/DevHub/`:
- `projects.json` — project list
- `links.json` — captured links
- `settings.json` — app settings (IDEs, autostart, tray)
- `logs/` — Serilog logs

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+Shift+Y | Capture URL from clipboard |
