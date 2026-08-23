<h1 align="center">DevHub</h1>

<p align="center">
  <strong>Developer productivity hub — organize projects, track tasks, capture links and launch IDEs from one place.</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue" alt="Platform">
  <img src="https://img.shields.io/badge/version-0.3.0-green" alt="Version">
  <img src="https://img.shields.io/badge/license-MIT-orange" alt="License">
</p>

---

## What is DevHub?

DevHub is a desktop app for developers who juggle multiple projects and links. It keeps everything in one place so you don't lose track.

**Core features:**

- **Project catalog** — add your projects, mark favorites, filter by status, search instantly
- **Project detail dashboard** — per-project TODO list, attached links, git activity and stats in one view
- **TODO lists** — priorities (Low/Normal/High), inline editing, filters, progress bar, quick add via `+`
- **Project links** — attach URLs to a project, open/copy/delete in one click; all links also visible on the Links page with project badges
- **Git activity** — current branch, total commit count and latest commits with relative timestamps (shown only for git repositories)
- **Project stats** — file/dir counts, source size vs full disk size, last change time
- **Link capture** — press **Ctrl+Shift+Y** anywhere to save a URL from clipboard with auto-detected type (YouTube, GitHub, docs, article)
- **IDE launcher** — auto-detects installed IDEs (VS Code, Rider, IntelliJ, and more) and opens projects directly
- **Project scanner** — finds codebases on your disk by detecting project files (package.json, Cargo.toml, go.mod, etc.)
- **System tray** — lives in the tray, always one click away
- **Dark/Light theme** — toggle in settings

## Getting Started

### Prerequisites

- [Bun](https://bun.sh) (JavaScript runtime & package manager)
- [Rust](https://rustup.rs) (for Tauri backend)
- Platform-specific dependencies: see [Tauri prerequisites](https://v2.tauri.app/start/prerequisites/)

### Install & Run

```bash
git clone https://github.com/GreDDySS/DevHub.git
cd DevHub
bun install
bun run tauri dev
```

### Build

```bash
bun run tauri build
```

Output will be in `src-tauri/target/release/bundle/`.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | Rust + [Tauri 2](https://v2.tauri.app) |
| Frontend | React 19 + TypeScript + Vite |
| Styling | Tailwind CSS 4 |
| State | Zustand |
| Testing | Vitest (frontend) + cargo test (backend) |

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+Shift+Y | Capture URL from clipboard |

## Data Storage

All data is stored locally:

| File | Contents |
|------|----------|
| `projects.json` | Your project catalog |
| `links.json` | Captured links (with optional project attachments) |
| `todos.json` | TODO tasks per project |
| `settings.json` | IDE config, close behavior, autostart, theme |

Location: `%LOCALAPPDATA%/DevHub/` (Windows), `~/.local/share/DevHub/` (Linux), `~/Library/Application Support/DevHub/` (macOS).

## Running Tests

```bash
# Frontend tests
bun run test

# Backend tests
cd src-tauri && cargo test

# Watch mode
bun run test:watch
```

## License

[MIT](LICENSE)
