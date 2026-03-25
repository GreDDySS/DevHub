# Development Roadmap

## Обзор

Разработка DevHub разбита на 4 фазы. MVP (Phase 1) — минимальный набор для ежедневного использования.

## Фазы разработки

### Phase 1 — MVP (Minimum Viable Product)

**Цель:** Базовое управление проектами для ежедневного использования.

**Длительность:** ~5 недель

| Неделя | Milestone | Deliverables |
|--------|-----------|--------------|
| 1 | Инфраструктура | Solution, проекты слоёв, DI, logging, CI |
| 2 | Domain + Infrastructure | Сущности, интерфейсы, JSON-хранилище |
| 3 | UI — Основа | MainWindow, Sidebar, Project List |
| 4 | UI — Формы | Add/Edit project form, Settings page |
| 5 | Интеграция и тесты | Explorer/IDE launcher, unit tests, bugfix |

**Функционал:**
- CRUD проектов (FR-001..FR-005)
- Открытие в Explorer (FR-101)
- Открытие в IDE (FR-102, FR-103)
- JSON-хранение данных
- Настройки IDE

---

### Phase 2 — Расширение ядра

**Цель:** Ссылки, автозапуск, трей — полноценный daily driver.

**Длительность:** ~4 недели

| Неделя | Milestone | Deliverables |
|--------|-----------|--------------|
| 6 | Модуль ссылок | CaptureLinkUseCase, links.json, UI списка ссылок |
| 7 | Горячие клавиши | Global hotkey (Ctrl+Shift+Y), toast notifications |
| 8 | Трей + автозапуск | System tray, autostart with Windows |
| 9 | Теги и заметки | Tags UI, Notes field, фильтрация по тегам |

**Функционал:**
- Захват ссылок (FR-201..FR-205)
- Сворачивание в трей (FR-302)
- Автозапуск (FR-301)
- Теги (FR-006)
- Заметки (FR-007)

---

### Phase 3 — Улучшение UX

**Цель:** Полировка интерфейса и расширение возможностей.

**Длительность:** ~3 недели

| Неделя | Milestone | Deliverables |
|--------|-----------|--------------|
| 10 | Темы оформления | Тёмная/светлая тема, настройка |
| 11 | Импорт и экспорт | Сканирование директорий, экспорт в JSON/CSV |
| 12 | Избранное + улучшения | Favorites, keyboard shortcuts, UI polish |

**Функционал:**
- Тёмная/светлая тема (FR-303)
- Импорт проектов (FR-009)
- Экспорт данных (FR-304)
- Избранное (FR-008)
- Клавиатурная навигация (NFR-302)

---

### Phase 4 — Модули расширения

**Цель:** Новые модули — сниппеты, задачи, заметки.

**Длительность:** TBD

| Модуль | Описание | Приоритет |
|--------|----------|-----------|
| Snippets | Хранение и быстрый доступ к сниппетам кода | Could |
| Tasks/TODO | Простой трекер задач без привязки к проектам | Could |
| Notes | Блокнот для общих заметок | Could |
| Bookmarks | Расширенные закладки с категориями | Could |

---

## Timeline (Gantt)

```
2026
│
├─ Q2 ──────────────────────────────────────────────────
│  │
│  ├─ Phase 1 (MVP) ──────────────────────────┤
│  │  Week 1-5                                │
│  │                                           │
├─ Q3 ──────────────────────────────────────────────────
│  │                                           │
│  ├─ Phase 2 ────────────────────────────────┤
│  │  Week 6-9                                │
│  │                                           │
│  ├─ Phase 3 ────────────┤                    │
│  │  Week 10-12          │                    │
│  │                       │                    │
├─ Q4 ──────────────────────────────────────────────────
│  │                       │                    │
│  ├─ Phase 4 ─────────────────────────────────────────
│  │  TBD                                       
```

## Milestones Summary

| Milestone | Дата (план) | Критерий готовности |
|-----------|-------------|---------------------|
| **M1 — Foundation** | Week 1 | Solution builds, CI green |
| **M2 — Domain Ready** | Week 2 | Entities + JSON storage tested |
| **M3 — First UI** | Week 3 | Main window with project list |
| **M4 — MVP Alpha** | Week 4 | Full CRUD + settings |
| **M5 — MVP Release** | Week 5 | All MVP criteria met |
| **M6 — Links Module** | Week 7 | Capture + display links |
| **M7 — Phase 2 Release** | Week 9 | Hotkey + tray + tags |
| **M8 — Phase 3 Release** | Week 12 | Themes + import/export |

## Риски

| Риск | Вероятность | Влияние | Митигация |
|------|-------------|---------|-----------|
| Сложность WPF-биндинга | Medium | Medium | Использовать CommunityToolkit.Mvvm |
| Конфликты горячих клавиш | Low | High | Обработка ошибок, настраиваемые комбинации |
| Потеря данных | Low | Critical | Автосохранение, бэкапы |
| Scope creep | High | Medium | Чёткое следование MoSCoW приоритетам |

## Связанные документы

- [Functional Requirements](../02-Requirements/functional-requirements.md)
- [User Stories](../02-Requirements/user-stories.md)
- [Project Management Module](../05-Implementation/project-management-module.md)
