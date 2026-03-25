# Database Schema

## Обзор

Данные хранятся в JSON-файлах в директории `%AppData%/DevHub/`.

## Файлы данных

| Файл | Назначение | Обновление |
|------|------------|------------|
| `projects.json` | Каталог всех проектов | При каждом изменении |
| `links.json` | Сохранённые ссылки | При захвате/добавлении |
| `settings.json` | Настройки приложения | При изменении настроек |
| `backup/` | Резервные копии | По расписанию / вручную |

## JSON Schema — projects.json

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "version": { "type": "integer", "const": 1 },
    "updatedAt": { "type": "string", "format": "date-time" },
    "projects": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "name", "path", "language", "status", "createdAt", "updatedAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "name": { "type": "string", "minLength": 1, "maxLength": 200 },
          "path": { "type": "string", "minLength": 1 },
          "description": { "type": "string", "maxLength": 2000 },
          "notes": { "type": "string", "maxLength": 10000 },
          "language": {
            "type": "string",
            "enum": ["CSharp", "Python", "Rust", "JavaScript", "TypeScript", "Go", "Java", "Cpp", "Other"]
          },
          "status": {
            "type": "string",
            "enum": ["Active", "Completed", "Paused", "Archived"]
          },
          "tags": {
            "type": "array",
            "items": { "type": "string", "maxLength": 50 },
            "maxItems": 20
          },
          "preferredIde": { "type": "string", "enum": ["VSCode", "VisualStudio", "Rider", ""] },
          "isFavorite": { "type": "boolean", "default": false },
          "createdAt": { "type": "string", "format": "date-time" },
          "updatedAt": { "type": "string", "format": "date-time" },
          "lastAccessedAt": { "type": ["string", "null"], "format": "date-time" }
        }
      }
    }
  },
  "required": ["version", "updatedAt", "projects"]
}
```

## Пример projects.json

```json
{
  "version": 1,
  "updatedAt": "2026-03-25T14:30:00Z",
  "projects": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "name": "MyApi",
      "path": "D:\\repos\\MyApi",
      "description": "REST API для мобильного приложения",
      "notes": "TODO: добавить авторизацию JWT",
      "language": "CSharp",
      "status": "Active",
      "tags": ["api", "backend", "dotnet"],
      "preferredIde": "VisualStudio",
      "isFavorite": true,
      "createdAt": "2025-11-15T10:00:00Z",
      "updatedAt": "2026-03-24T18:45:00Z",
      "lastAccessedAt": "2026-03-24T18:45:00Z"
    },
    {
      "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
      "name": "DataParser",
      "path": "D:\\repos\\DataParser",
      "description": "Парсер CSV-файлов для аналитики",
      "notes": "",
      "language": "Python",
      "status": "Completed",
      "tags": ["parser", "data"],
      "preferredIde": "VSCode",
      "isFavorite": false,
      "createdAt": "2025-08-20T09:00:00Z",
      "updatedAt": "2025-12-01T12:00:00Z",
      "lastAccessedAt": null
    }
  ]
}
```

## JSON Schema — links.json

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "version": { "type": "integer", "const": 1 },
    "updatedAt": { "type": "string", "format": "date-time" },
    "links": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "url", "capturedAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "url": { "type": "string", "format": "uri" },
          "title": { "type": "string", "maxLength": 500 },
          "type": {
            "type": "string",
            "enum": ["YouTube", "Article", "Repository", "Documentation", "Other"]
          },
          "projectId": { "type": ["string", "null"], "format": "uuid" },
          "tags": {
            "type": "array",
            "items": { "type": "string" }
          },
          "notes": { "type": "string", "maxLength": 2000 },
          "capturedAt": { "type": "string", "format": "date-time" }
        }
      }
    }
  },
  "required": ["version", "updatedAt", "links"]
}
```

## JSON Schema — settings.json

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "version": { "type": "integer", "const": 1 },
    "theme": { "type": "string", "enum": ["Light", "Dark", "System"], "default": "System" },
    "launchOnStartup": { "type": "boolean", "default": false },
    "minimizeToTray": { "type": "boolean", "default": true },
    "showNotifications": { "type": "boolean", "default": true },
    "ides": {
      "type": "object",
      "properties": {
        "vscode": { "type": "string", "default": "" },
        "visualStudio": { "type": "string", "default": "" },
        "rider": { "type": "string", "default": "" }
      }
    },
    "defaultIde": { "type": "string", "enum": ["VSCode", "VisualStudio", "Rider", ""], "default": "" },
    "backup": {
      "type": "object",
      "properties": {
        "enabled": { "type": "boolean", "default": true },
        "maxCopies": { "type": "integer", "minimum": 1, "maximum": 30, "default": 7 }
      }
    }
  },
  "required": ["version"]
}
```

## Пример settings.json

```json
{
  "version": 1,
  "theme": "Dark",
  "launchOnStartup": false,
  "minimizeToTray": true,
  "showNotifications": true,
  "ides": {
    "vscode": "C:\\Users\\misha\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe",
    "visualStudio": "C:\\Program Files\\Microsoft Visual Studio\\2026\\Community\\Common7\\IDE\\devenv.exe",
    "rider": ""
  },
  "defaultIde": "VSCode",
  "backup": {
    "enabled": true,
    "maxCopies": 7
  }
}
```

## Стратегия миграций

| Версия схемы | Описание | Миграция |
|--------------|----------|----------|
| 1 | Начальная версия | — |
| 2+ | Будущие изменения | Автоматическая при чтении, с бэкапом |

При обновлении `version` в файле:
1. Создаётся бэкап текущего файла
2. Выполняется миграция данных
3. Файл сохраняется с новой версией

## Связанные документы

- [Data Model](../03-Architecture/data-model.md)
- [Architecture Overview](../03-Architecture/architecture-overview.md)
- [Functional Requirements](../02-Requirements/functional-requirements.md)
