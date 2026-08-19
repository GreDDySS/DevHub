use std::fs;
use std::path::PathBuf;
use std::sync::Mutex;
use once_cell::sync::Lazy;
use chrono::Utc;

use crate::models::{Project, Link, AppSettings, CURRENT_SETTINGS_VERSION};

static PROJECTS: Lazy<Mutex<Vec<Project>>> = Lazy::new(|| Mutex::new(Vec::new()));
static LINKS: Lazy<Mutex<Vec<Link>>> = Lazy::new(|| Mutex::new(Vec::new()));
static SETTINGS: Lazy<Mutex<AppSettings>> = Lazy::new(|| Mutex::new(AppSettings::default()));

fn get_data_dir() -> PathBuf {
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
    fs::write(&projects_file, data)
        .map_err(|e| format!("Failed to write projects file: {}", e))?;
    Ok(())
}

pub fn update_project(id: &str, update: crate::models::UpdateProjectRequest) -> Result<Project, String> {
    let data_dir = get_data_dir();
    let projects_file = data_dir.join("projects.json");
    let (result, data) = {
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
    fs::write(&projects_file, data)
        .map_err(|e| format!("Failed to write projects file: {}", e))?;
    Ok(result)
}

pub fn delete_project(id: &str) -> Result<(), String> {
    let data_dir = get_data_dir();
    let projects_file = data_dir.join("projects.json");
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
    fs::write(&projects_file, data)
        .map_err(|e| format!("Failed to write projects file: {}", e))?;
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
    fs::write(&links_file, data)
        .map_err(|e| format!("Failed to write links file: {}", e))?;
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
    fs::write(&links_file, data)
        .map_err(|e| format!("Failed to write links file: {}", e))?;
    Ok(())
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
    fs::write(&settings_file, data)
        .map_err(|e| format!("Failed to write settings file: {}", e))?;
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
mod tests {
    use super::*;
    use crate::models::{Project, Link, UpdateProjectRequest};

    fn setup_test_dir() -> PathBuf {
        let dir = std::env::temp_dir().join(format!("devhub_test_{}", uuid::Uuid::new_v4()));
        fs::create_dir_all(&dir).unwrap();
        std::env::set_var("DEVHUB_TEST_DIR", dir.to_str().unwrap());
        dir
    }

    fn cleanup_test_dir(dir: &PathBuf) {
        std::env::remove_var("DEVHUB_TEST_DIR");
        let _ = fs::remove_dir_all(dir);
    }

    fn reset_storage() {
        *PROJECTS.lock().unwrap() = Vec::new();
        *LINKS.lock().unwrap() = Vec::new();
        *SETTINGS.lock().unwrap() = AppSettings::default();
    }

    #[test]
    fn test_migrate_settings_from_old_version() {
        let mut settings = AppSettings::default();
        settings.version = 0;
        let migrated = migrate_settings(settings);
        assert_eq!(migrated.version, CURRENT_SETTINGS_VERSION);
    }

    #[test]
    fn test_migrate_settings_already_current() {
        let mut settings = AppSettings::default();
        settings.version = CURRENT_SETTINGS_VERSION;
        let migrated = migrate_settings(settings);
        assert_eq!(migrated.version, CURRENT_SETTINGS_VERSION);
    }

    #[test]
    fn test_init_storage_missing_files() {
        let dir = setup_test_dir();
        reset_storage();
        let result = init_storage();
        assert!(result.is_ok());
        let projects = get_projects();
        assert_eq!(projects.len(), 0);
        let settings = get_settings();
        assert_eq!(settings.version, CURRENT_SETTINGS_VERSION);
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_corrupted_settings_json() {
        let dir = setup_test_dir();
        reset_storage();
        let data_dir = get_data_dir();
        fs::create_dir_all(&data_dir).unwrap();
        fs::write(data_dir.join("settings.json"), "not valid json {{{").unwrap();
        let result = init_storage();
        assert!(result.is_err());
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_corrupted_projects_json() {
        let dir = setup_test_dir();
        reset_storage();
        let data_dir = get_data_dir();
        fs::create_dir_all(&data_dir).unwrap();
        fs::write(data_dir.join("projects.json"), "definitely not json").unwrap();
        let result = init_storage();
        assert!(result.is_err());
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_corrupted_links_json() {
        let dir = setup_test_dir();
        reset_storage();
        let data_dir = get_data_dir();
        fs::create_dir_all(&data_dir).unwrap();
        fs::write(data_dir.join("links.json"), "{bad json").unwrap();
        let result = init_storage();
        assert!(result.is_err());
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_add_and_get_project() {
        let dir = setup_test_dir();
        reset_storage();
        let project = Project::new("Test".to_string(), "/test/path".to_string()).unwrap();
        add_project(project).unwrap();
        let projects = get_projects();
        assert_eq!(projects.len(), 1);
        assert_eq!(projects[0].name, "Test");
        assert_eq!(projects[0].path, "/test/path");
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_add_multiple_projects() {
        let dir = setup_test_dir();
        reset_storage();
        for i in 0..5 {
            let p = Project::new(format!("Project {}", i), format!("/path/{}", i)).unwrap();
            add_project(p).unwrap();
        }
        let projects = get_projects();
        assert_eq!(projects.len(), 5);
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_update_project_single_field() {
        let dir = setup_test_dir();
        reset_storage();
        let project = Project::new("Original".to_string(), "/path".to_string()).unwrap();
        let id = project.id.clone();
        add_project(project).unwrap();

        let update = UpdateProjectRequest {
            name: Some("Renamed".to_string()),
            ..Default::default()
        };
        let updated = update_project(&id, update).unwrap();
        assert_eq!(updated.name, "Renamed");
        assert_eq!(updated.path, "/path");
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_update_project_multiple_fields() {
        let dir = setup_test_dir();
        reset_storage();
        let project = Project::new("Original".to_string(), "/path".to_string()).unwrap();
        let id = project.id.clone();
        add_project(project).unwrap();

        let update = UpdateProjectRequest {
            name: Some("New Name".to_string()),
            description: Some("A description".to_string()),
            is_favorite: Some(true),
            ..Default::default()
        };
        let updated = update_project(&id, update).unwrap();
        assert_eq!(updated.name, "New Name");
        assert_eq!(updated.description, "A description");
        assert!(updated.is_favorite);
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_update_project_not_found() {
        let dir = setup_test_dir();
        reset_storage();
        let result = update_project(
            "nonexistent-id",
            UpdateProjectRequest {
                name: Some("X".to_string()),
                ..Default::default()
            },
        );
        assert!(result.is_err());
        assert!(result.unwrap_err().contains("not found"));
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_delete_project() {
        let dir = setup_test_dir();
        reset_storage();
        let project = Project::new("ToDelete".to_string(), "/path".to_string()).unwrap();
        let id = project.id.clone();
        add_project(project).unwrap();
        assert_eq!(get_projects().len(), 1);

        delete_project(&id).unwrap();
        assert_eq!(get_projects().len(), 0);
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_delete_project_not_found() {
        let dir = setup_test_dir();
        reset_storage();
        let result = delete_project("nonexistent-id");
        assert!(result.is_err());
        assert!(result.unwrap_err().contains("not found"));
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_add_and_get_link() {
        let dir = setup_test_dir();
        reset_storage();
        let link = Link::new("https://example.com".to_string()).unwrap();
        add_link(link).unwrap();
        let links = get_links();
        assert_eq!(links.len(), 1);
        assert_eq!(links[0].url, "https://example.com");
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_delete_link() {
        let dir = setup_test_dir();
        reset_storage();
        let link = Link::new("https://example.com".to_string()).unwrap();
        let id = link.id.clone();
        add_link(link).unwrap();
        assert_eq!(get_links().len(), 1);

        delete_link(&id).unwrap();
        assert_eq!(get_links().len(), 0);
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_delete_link_not_found() {
        let dir = setup_test_dir();
        reset_storage();
        let result = delete_link("nonexistent-id");
        assert!(result.is_err());
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_update_settings_roundtrip() {
        let dir = setup_test_dir();
        reset_storage();
        let mut settings = AppSettings::default();
        settings.is_dark_theme = true;
        settings.autostart_enabled = true;
        update_settings(settings.clone()).unwrap();

        let loaded = get_settings();
        assert!(loaded.is_dark_theme);
        assert!(loaded.autostart_enabled);
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_concurrent_add_projects() {
        let dir = setup_test_dir();
        reset_storage();
        let mut handles = vec![];
        for i in 0..10 {
            handles.push(std::thread::spawn(move || {
                let p = Project::new(format!("Project {}", i), format!("/path/{}", i)).unwrap();
                add_project(p).unwrap();
            }));
        }
        for h in handles {
            h.join().unwrap();
        }
        let projects = get_projects();
        assert_eq!(projects.len(), 10);
        cleanup_test_dir(&dir);
    }

    #[test]
    fn test_project_roundtrip_serialization() {
        let project = Project::new("Roundtrip".to_string(), "/test".to_string()).unwrap();
        let json = serde_json::to_string(&project).unwrap();
        let deserialized: Project = serde_json::from_str(&json).unwrap();
        assert_eq!(deserialized.name, project.name);
        assert_eq!(deserialized.path, project.path);
        assert_eq!(deserialized.id, project.id);
    }

    #[test]
    fn test_link_roundtrip_serialization() {
        let link = Link::new("https://github.com/test".to_string()).unwrap();
        let json = serde_json::to_string(&link).unwrap();
        let deserialized: Link = serde_json::from_str(&json).unwrap();
        assert_eq!(deserialized.url, link.url);
        assert_eq!(deserialized.id, link.id);
    }
}
