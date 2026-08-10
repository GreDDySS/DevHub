use std::fs;
use std::path::PathBuf;
use std::sync::Mutex;
use once_cell::sync::Lazy;

use crate::models::{Project, Link, AppSettings};

static PROJECTS: Lazy<Mutex<Vec<Project>>> = Lazy::new(|| Mutex::new(Vec::new()));
static LINKS: Lazy<Mutex<Vec<Link>>> = Lazy::new(|| Mutex::new(Vec::new()));
static SETTINGS: Lazy<Mutex<AppSettings>> = Lazy::new(|| Mutex::new(AppSettings::default()));

fn get_data_dir() -> PathBuf {
    let data_dir = dirs::data_local_dir()
        .unwrap_or_else(|| PathBuf::from("."))
        .join("DevHub");
    fs::create_dir_all(&data_dir).ok();
    data_dir
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
    
    // Load settings
    let settings_file = data_dir.join("settings.json");
    if settings_file.exists() {
        let data = fs::read_to_string(&settings_file)?;
        let settings: AppSettings = serde_json::from_str(&data)?;
        *SETTINGS.lock().unwrap() = settings;
    }
    
    Ok(())
}

pub fn save_projects() -> Result<(), String> {
    let data_dir = get_data_dir();
    let projects_file = data_dir.join("projects.json");
    let projects = PROJECTS.lock().unwrap();
    let data = serde_json::to_string_pretty(&*projects)
        .map_err(|e| format!("Failed to serialize projects: {}", e))?;
    fs::write(&projects_file, data)
        .map_err(|e| format!("Failed to write projects file: {}", e))?;
    Ok(())
}

pub fn save_links() -> Result<(), String> {
    let data_dir = get_data_dir();
    let links_file = data_dir.join("links.json");
    let links = LINKS.lock().unwrap();
    let data = serde_json::to_string_pretty(&*links)
        .map_err(|e| format!("Failed to serialize links: {}", e))?;
    fs::write(&links_file, data)
        .map_err(|e| format!("Failed to write links file: {}", e))?;
    Ok(())
}

pub fn save_settings() -> Result<(), String> {
    let data_dir = get_data_dir();
    let settings_file = data_dir.join("settings.json");
    let settings = SETTINGS.lock().unwrap();
    let data = serde_json::to_string_pretty(&*settings)
        .map_err(|e| format!("Failed to serialize settings: {}", e))?;
    fs::write(&settings_file, data)
        .map_err(|e| format!("Failed to write settings file: {}", e))?;
    Ok(())
}

pub fn get_projects() -> Vec<Project> {
    PROJECTS.lock().unwrap().clone()
}

pub fn add_project(project: Project) -> Result<(), String> {
    PROJECTS.lock().unwrap().push(project);
    save_projects()
}

pub fn update_project(id: &str, update: crate::models::UpdateProjectRequest) -> Result<Project, String> {
    let mut projects = PROJECTS.lock().unwrap();
    let project = projects.iter_mut().find(|p| p.id == id)
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
    if let Some(notes) = update.notes {
        project.notes = notes;
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
    if let Some(auto_status_enabled) = update.auto_status_enabled {
        project.auto_status_enabled = auto_status_enabled;
    }
    
    project.updated_at = chrono::Utc::now();
    
    let result = project.clone();
    drop(projects);
    save_projects()?;
    Ok(result)
}

pub fn delete_project(id: &str) -> Result<(), String> {
    let mut projects = PROJECTS.lock().unwrap();
    let initial_len = projects.len();
    projects.retain(|p| p.id != id);
    
    if projects.len() == initial_len {
        return Err(format!("Project not found: {}", id));
    }
    
    drop(projects);
    save_projects()
}

pub fn get_links() -> Vec<Link> {
    LINKS.lock().unwrap().clone()
}

pub fn add_link(link: Link) -> Result<(), String> {
    LINKS.lock().unwrap().push(link);
    save_links()
}

pub fn delete_link(id: &str) -> Result<(), String> {
    let mut links = LINKS.lock().unwrap();
    let initial_len = links.len();
    links.retain(|l| l.id != id);
    
    if links.len() == initial_len {
        return Err(format!("Link not found: {}", id));
    }
    
    drop(links);
    save_links()
}

pub fn get_settings() -> AppSettings {
    SETTINGS.lock().unwrap().clone()
}

pub fn update_settings(new_settings: AppSettings) -> Result<(), String> {
    *SETTINGS.lock().unwrap() = new_settings;
    save_settings()
}
