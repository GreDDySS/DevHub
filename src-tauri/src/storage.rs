use chrono::Utc;
use once_cell::sync::Lazy;
use std::fs;
use std::path::PathBuf;
use std::sync::Mutex;

use crate::models::{AppSettings, Link, Project, Todo, CURRENT_SETTINGS_VERSION};

static PROJECTS: Lazy<Mutex<Vec<Project>>> = Lazy::new(|| Mutex::new(Vec::new()));
static LINKS: Lazy<Mutex<Vec<Link>>> = Lazy::new(|| Mutex::new(Vec::new()));
static TODOS: Lazy<Mutex<Vec<Todo>>> = Lazy::new(|| Mutex::new(Vec::new()));
static SETTINGS: Lazy<Mutex<AppSettings>> = Lazy::new(|| Mutex::new(AppSettings::default()));

pub fn get_data_dir() -> PathBuf {
    #[cfg(test)]
    {
        if let Ok(dir) = std::env::var("DEVHUB_TEST_DIR") {
            let path = PathBuf::from(dir);
            fs::create_dir_all(&path).ok();
            return path;
        }
    }
    let data_dir = dirs::data_local_dir()
        .unwrap_or_else(|| PathBuf::from("."))
        .join("DevHub");
    fs::create_dir_all(&data_dir).ok();
    data_dir
}

pub fn get_data_dir_path() -> String {
    get_data_dir().to_string_lossy().to_string()
}

pub fn init_storage() -> Result<(), Box<dyn std::error::Error>> {
    let data_dir = get_data_dir();

    // Load projects
    let projects_file = data_dir.join("projects.json");
    if projects_file.exists() {
        let data = fs::read_to_string(&projects_file)?;
        let projects: Vec<Project> = serde_json::from_str(&data)?;
        *PROJECTS.lock().unwrap() = projects;
    }

    // Load links
    let links_file = data_dir.join("links.json");
    if links_file.exists() {
        let data = fs::read_to_string(&links_file)?;
        let links: Vec<Link> = serde_json::from_str(&data)?;
        *LINKS.lock().unwrap() = links;
    }

    // Load todos
    let todos_file = data_dir.join("todos.json");
    if todos_file.exists() {
        let data = fs::read_to_string(&todos_file)?;
        let todos: Vec<Todo> = serde_json::from_str(&data)?;
        *TODOS.lock().unwrap() = todos;
    }

    // Load settings
    let settings_file = data_dir.join("settings.json");
    if settings_file.exists() {
        let data = fs::read_to_string(&settings_file)?;
        let settings: AppSettings = serde_json::from_str(&data)?;
        let settings = migrate_settings(settings);
        *SETTINGS.lock().unwrap() = settings;
    }

    Ok(())
}

pub fn get_projects() -> Vec<Project> {
    let settings = SETTINGS.lock().unwrap().clone();
    let inactive_days = settings.inactive_days;
    let statuses_enabled = settings.statuses_enabled;
    PROJECTS
        .lock()
        .unwrap()
        .iter()
        .map(|p| {
            let mut project = p.clone();
            if statuses_enabled {
                let last_modified = std::fs::metadata(&p.path)
                    .and_then(|m| m.modified())
                    .ok()
                    .and_then(|t| {
                        let datetime: chrono::DateTime<Utc> = t.into();
                        Some(datetime)
                    })
                    .unwrap_or(p.updated_at);
                project.status = crate::models::calculate_status(last_modified, inactive_days);
            }
            project
        })
        .collect()
}

pub fn add_project(project: Project) -> Result<(), String> {
    let data_dir = get_data_dir();
    let projects_file = data_dir.join("projects.json");
    let data = {
        let mut projects = PROJECTS.lock().unwrap();
        projects.push(project);
        serde_json::to_string_pretty(&*projects)
            .map_err(|e| format!("Failed to serialize projects: {}", e))?
    };
    fs::write(&projects_file, data).map_err(|e| format!("Failed to write projects file: {}", e))?;
    Ok(())
}

pub fn update_project(
    id: &str,
    update: crate::models::UpdateProjectRequest,
) -> Result<Project, String> {
    let data_dir = get_data_dir();
    let projects_file = data_dir.join("projects.json");
    let (result, data) = {
        let mut projects = PROJECTS.lock().unwrap();
        let project = projects
            .iter_mut()
            .find(|p| p.id == id)
            .ok_or_else(|| format!("Project not found: {}", id))?;

        if let Some(name) = update.name {
            project.name = name;
        }
        if let Some(path) = update.path {
            project.path = path;
        }
        if let Some(description) = update.description {
            project.description = description;
        }
        if let Some(language) = update.language {
            project.language = language;
        }
        if let Some(status) = update.status {
            project.status = status;
        }
        if let Some(tags) = update.tags {
            project.tags = tags;
        }
        if let Some(preferred_ide) = update.preferred_ide {
            project.preferred_ide = preferred_ide;
        }
        if let Some(is_favorite) = update.is_favorite {
            project.is_favorite = is_favorite;
        }
        if let Some(is_hidden) = update.is_hidden {
            project.is_hidden = is_hidden;
        }

        project.updated_at = chrono::Utc::now();

        let result = project.clone();
        let data = serde_json::to_string_pretty(&*projects)
            .map_err(|e| format!("Failed to serialize projects: {}", e))?;
        (result, data)
    };
    fs::write(&projects_file, data).map_err(|e| format!("Failed to write projects file: {}", e))?;
    Ok(result)
}

pub fn delete_project(id: &str) -> Result<(), String> {
    let data_dir = get_data_dir();
    let projects_file = data_dir.join("projects.json");
    let links_file = data_dir.join("links.json");
    let links_data = {
        let mut links = LINKS.lock().unwrap();
        let mut changed = false;
        for link in links.iter_mut() {
            if link.project_id.as_deref() == Some(id) {
                link.project_id = None;
                changed = true;
            }
        }
        if changed {
            serde_json::to_string_pretty(&*links)
                .map_err(|e| format!("Failed to serialize links: {}", e))?
        } else {
            String::new()
        }
    };
    let data = {
        let mut projects = PROJECTS.lock().unwrap();
        let initial_len = projects.len();
        projects.retain(|p| p.id != id);

        if projects.len() == initial_len {
            return Err(format!("Project not found: {}", id));
        }

        serde_json::to_string_pretty(&*projects)
            .map_err(|e| format!("Failed to serialize projects: {}", e))?
    };
    fs::write(&projects_file, data).map_err(|e| format!("Failed to write projects file: {}", e))?;
    if !links_data.is_empty() {
        fs::write(&links_file, links_data)
            .map_err(|e| format!("Failed to write links file: {}", e))?;
    }
    Ok(())
}

pub fn remove_missing_projects() -> Result<usize, String> {
    let data_dir = get_data_dir();
    let projects_file = data_dir.join("projects.json");
    let (data, removed) = {
        let mut projects = PROJECTS.lock().unwrap();
        let initial_len = projects.len();
        projects.retain(|p| std::path::Path::new(&p.path).exists());
        let removed = initial_len - projects.len();
        let data = serde_json::to_string_pretty(&*projects)
            .map_err(|e| format!("Failed to serialize projects: {}", e))?;
        (data, removed)
    };
    if removed > 0 {
        fs::write(&projects_file, data)
            .map_err(|e| format!("Failed to write projects file: {}", e))?;
    }
    Ok(removed)
}

pub fn get_links() -> Vec<Link> {
    LINKS.lock().unwrap().clone()
}

pub fn add_link(link: Link) -> Result<(), String> {
    let data_dir = get_data_dir();
    let links_file = data_dir.join("links.json");
    let data = {
        let mut links = LINKS.lock().unwrap();
        links.push(link);
        serde_json::to_string_pretty(&*links)
            .map_err(|e| format!("Failed to serialize links: {}", e))?
    };
    fs::write(&links_file, data).map_err(|e| format!("Failed to write links file: {}", e))?;
    Ok(())
}

pub fn delete_link(id: &str) -> Result<(), String> {
    let data_dir = get_data_dir();
    let links_file = data_dir.join("links.json");
    let data = {
        let mut links = LINKS.lock().unwrap();
        let initial_len = links.len();
        links.retain(|l| l.id != id);

        if links.len() == initial_len {
            return Err(format!("Link not found: {}", id));
        }

        serde_json::to_string_pretty(&*links)
            .map_err(|e| format!("Failed to serialize links: {}", e))?
    };
    fs::write(&links_file, data).map_err(|e| format!("Failed to write links file: {}", e))?;
    Ok(())
}

pub fn get_todos() -> Vec<Todo> {
    let mut todos = TODOS.lock().unwrap().clone();
    todos.sort_by(|a, b| {
        a.is_completed
            .cmp(&b.is_completed)
            .then_with(|| priority_rank(&b.priority).cmp(&priority_rank(&a.priority)))
            .then_with(|| b.created_at.cmp(&a.created_at))
    });
    todos
}

fn priority_rank(priority: &crate::models::TodoPriority) -> u8 {
    match priority {
        crate::models::TodoPriority::High => 2,
        crate::models::TodoPriority::Normal => 1,
        crate::models::TodoPriority::Low => 0,
    }
}

pub fn add_todo(todo: Todo) -> Result<(), String> {
    let data_dir = get_data_dir();
    let todos_file = data_dir.join("todos.json");
    let data = {
        let mut todos = TODOS.lock().unwrap();
        todos.push(todo);
        serde_json::to_string_pretty(&*todos)
            .map_err(|e| format!("Failed to serialize todos: {}", e))?
    };
    fs::write(&todos_file, data).map_err(|e| format!("Failed to write todos file: {}", e))?;
    Ok(())
}

pub fn update_todo(id: &str, update: crate::models::UpdateTodoRequest) -> Result<Todo, String> {
    let data_dir = get_data_dir();
    let todos_file = data_dir.join("todos.json");
    let (result, data) = {
        let mut todos = TODOS.lock().unwrap();
        let todo = todos
            .iter_mut()
            .find(|t| t.id == id)
            .ok_or_else(|| format!("Todo not found: {}", id))?;

        if let Some(title) = update.title {
            let title = title.trim().to_string();
            if title.is_empty() {
                return Err("Todo title cannot be empty".to_string());
            }
            todo.title = title;
        }
        if let Some(priority) = update.priority {
            todo.priority = priority;
        }
        if let Some(is_completed) = update.is_completed {
            if is_completed != todo.is_completed {
                todo.is_completed = is_completed;
                todo.completed_at = if is_completed {
                    Some(chrono::Utc::now())
                } else {
                    None
                };
            }
        }

        todo.updated_at = chrono::Utc::now();

        let result = todo.clone();
        let data = serde_json::to_string_pretty(&*todos)
            .map_err(|e| format!("Failed to serialize todos: {}", e))?;
        (result, data)
    };
    fs::write(&todos_file, data).map_err(|e| format!("Failed to write todos file: {}", e))?;
    Ok(result)
}

pub fn delete_todo(id: &str) -> Result<(), String> {
    let data_dir = get_data_dir();
    let todos_file = data_dir.join("todos.json");
    let data = {
        let mut todos = TODOS.lock().unwrap();
        let initial_len = todos.len();
        todos.retain(|t| t.id != id);

        if todos.len() == initial_len {
            return Err(format!("Todo not found: {}", id));
        }

        serde_json::to_string_pretty(&*todos)
            .map_err(|e| format!("Failed to serialize todos: {}", e))?
    };
    fs::write(&todos_file, data).map_err(|e| format!("Failed to write todos file: {}", e))?;
    Ok(())
}

pub fn clear_completed_todos() -> Result<usize, String> {
    let data_dir = get_data_dir();
    let todos_file = data_dir.join("todos.json");
    let (data, removed) = {
        let mut todos = TODOS.lock().unwrap();
        let initial_len = todos.len();
        todos.retain(|t| !t.is_completed);
        let removed = initial_len - todos.len();
        let data = serde_json::to_string_pretty(&*todos)
            .map_err(|e| format!("Failed to serialize todos: {}", e))?;
        (data, removed)
    };
    if removed > 0 {
        fs::write(&todos_file, data).map_err(|e| format!("Failed to write todos file: {}", e))?;
    }
    Ok(removed)
}

pub fn get_settings() -> AppSettings {
    SETTINGS.lock().unwrap().clone()
}

pub fn update_settings(new_settings: AppSettings) -> Result<(), String> {
    let data_dir = get_data_dir();
    let settings_file = data_dir.join("settings.json");
    {
        *SETTINGS.lock().unwrap() = new_settings;
    }
    let data = {
        let settings = SETTINGS.lock().unwrap();
        serde_json::to_string_pretty(&*settings)
            .map_err(|e| format!("Failed to serialize settings: {}", e))?
    };
    fs::write(&settings_file, data).map_err(|e| format!("Failed to write settings file: {}", e))?;
    Ok(())
}

fn migrate_settings(mut settings: AppSettings) -> AppSettings {
    if settings.version < CURRENT_SETTINGS_VERSION {
        // Future migrations go here, e.g.:
        // if settings.version < 2 { ... settings.version = 2; }
        settings.version = CURRENT_SETTINGS_VERSION;
    }
    settings
}

#[cfg(test)]
#[path = "tests/storage_tests.rs"]
mod tests;
