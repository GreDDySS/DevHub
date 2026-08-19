use crate::models::*;
use crate::storage;
use crate::ide_detection;

#[tauri::command]
pub fn force_exit(app: tauri::AppHandle) {
    app.exit(0);
}

#[tauri::command]
pub fn refresh_projects() -> Result<Vec<Project>, String> {
    storage::remove_missing_projects()?;
    Ok(storage::get_projects())
}

#[tauri::command]
pub fn get_projects(filter: Option<ProjectFilter>) -> Vec<Project> {
    let mut projects = storage::get_projects();
    
    if let Some(filter) = filter {
        if let Some(search) = filter.search_query {
            let search = search.to_lowercase();
            projects.retain(|p| {
                p.name.to_lowercase().contains(&search)
                    || p.path.to_lowercase().contains(&search)
                    || p.description.to_lowercase().contains(&search)
            });
        }
        
        if let Some(status) = filter.status {
            projects.retain(|p| p.status == status);
        }
        
        if let Some(languages) = filter.languages {
            if !languages.is_empty() {
                projects.retain(|p| languages.contains(&p.language));
            }
        }
        
        if let Some(show_hidden) = filter.show_hidden {
            if !show_hidden {
                projects.retain(|p| !p.is_hidden);
            }
        }
        
        match filter.sort_by.as_deref() {
            Some("name_asc") => {
                projects.sort_by(|a, b| a.name.to_lowercase().cmp(&b.name.to_lowercase()));
            }
            Some("name_desc") => {
                projects.sort_by(|a, b| b.name.to_lowercase().cmp(&a.name.to_lowercase()));
            }
            _ => {
                projects.sort_by(|a, b| {
                    b.is_favorite.cmp(&a.is_favorite)
                        .then_with(|| b.updated_at.cmp(&a.updated_at))
                });
            }
        }
    } else {
        projects.sort_by(|a, b| {
            b.is_favorite.cmp(&a.is_favorite)
                .then_with(|| b.updated_at.cmp(&a.updated_at))
        });
    }
    
    projects
}

#[tauri::command]
pub fn add_project(request: CreateProjectRequest) -> Result<Project, String> {
    let mut project = Project::new(request.name, request.path)?;
    
    if let Some(description) = request.description {
        project.description = description;
    }
    if let Some(language) = request.language {
        project.language = language;
    }
    
    storage::add_project(project.clone())?;
    Ok(project)
}

#[tauri::command]
pub fn update_project(id: String, request: UpdateProjectRequest) -> Result<Project, String> {
    storage::update_project(&id, request)
}

#[tauri::command]
pub fn delete_project(id: String) -> Result<(), String> {
    storage::delete_project(&id)
}

#[tauri::command]
pub fn toggle_favorite(id: String) -> Result<Project, String> {
    let projects = storage::get_projects();
    let project = projects.iter().find(|p| p.id == id)
        .ok_or_else(|| format!("Project not found: {}", id))?;
    
    let new_value = !project.is_favorite;
    storage::update_project(&id, UpdateProjectRequest {
        is_favorite: Some(new_value),
        ..Default::default()
    })
}

#[tauri::command]
pub fn toggle_hidden(id: String) -> Result<Project, String> {
    let projects = storage::get_projects();
    let project = projects.iter().find(|p| p.id == id)
        .ok_or_else(|| format!("Project not found: {}", id))?;
    
    let new_value = !project.is_hidden;
    storage::update_project(&id, UpdateProjectRequest {
        is_hidden: Some(new_value),
        ..Default::default()
    })
}

#[tauri::command]
pub fn get_links() -> Vec<Link> {
    storage::get_links()
}

#[tauri::command]
pub fn capture_link(url: String) -> Result<Link, String> {
    let mut link = Link::new(url)?;
    link.title = extract_title(&link.url);
    storage::add_link(link.clone())?;
    Ok(link)
}

#[tauri::command]
pub fn add_link(url: String, title: Option<String>) -> Result<Link, String> {
    let mut link = Link::new(url)?;
    if let Some(t) = title {
        link.title = t;
    } else {
        link.title = extract_title(&link.url);
    }
    storage::add_link(link.clone())?;
    Ok(link)
}

fn extract_title(url: &str) -> String {
    if url.contains("youtube.com") || url.contains("youtu.be") {
        "YouTube Video".to_string()
    } else if url.contains("github.com") {
        let parts: Vec<&str> = url.split('/').collect();
        if parts.len() >= 5 {
            format!("{}/{}", parts[3], parts[4])
        } else {
            "GitHub Repository".to_string()
        }
    } else if url.contains("docs.") || url.contains("/docs/") {
        "Documentation".to_string()
    } else {
        // Extract domain from URL
        url.split("://")
            .nth(1)
            .and_then(|s| s.split('/').next())
            .unwrap_or("Link")
            .to_string()
    }
}

#[tauri::command]
pub fn delete_link(id: String) -> Result<(), String> {
    storage::delete_link(&id)
}

#[tauri::command]
pub fn get_settings() -> AppSettings {
    storage::get_settings()
}

#[tauri::command]
pub fn save_settings(settings: AppSettings) -> Result<(), String> {
    storage::update_settings(settings)
}

#[tauri::command]
pub fn scan_ides() -> Vec<IdeEntry> {
    ide_detection::scan_ides()
}

#[tauri::command]
pub fn open_in_explorer(path: String) -> Result<(), String> {
    open::that(&path).map_err(|e| format!("Failed to open in explorer: {}", e))
}

#[tauri::command]
pub fn open_in_ide(project_path: String, ide_path: String) -> Result<(), String> {
    ide_detection::validate_ide_path(&ide_path)?;
    std::process::Command::new(&ide_path)
        .arg(&project_path)
        .spawn()
        .map_err(|e| format!("Failed to open in IDE: {}", e))?;
    Ok(())
}

#[tauri::command]
pub fn open_in_console(path: String) -> Result<(), String> {
    #[cfg(target_os = "windows")]
    {
        std::process::Command::new("cmd")
            .args(["/c", "start", "cmd", "/k", "cd", "/d", &path])
            .spawn()
            .map_err(|e| format!("Failed to open console: {}", e))?;
    }
    
    #[cfg(target_os = "linux")]
    {
        std::process::Command::new("gnome-terminal")
            .args(["--working-directory", &path])
            .spawn()
            .or_else(|_| {
                std::process::Command::new("konsole")
                    .args(["--workdir", &path])
                    .spawn()
            })
            .map_err(|e| format!("Failed to open console: {}", e))?;
    }
    
    #[cfg(target_os = "macos")]
    {
        std::process::Command::new("open")
            .args(["-a", "Terminal", &path])
            .spawn()
            .map_err(|e| format!("Failed to open console: {}", e))?;
    }
    
    Ok(())
}

#[tauri::command]
pub fn open_in_browser(url: String) -> Result<(), String> {
    open::that(&url).map_err(|e| format!("Failed to open in browser: {}", e))
}

#[tauri::command]
pub async fn detect_projects(root_path: String, app: tauri::AppHandle) -> Result<Vec<Project>, String> {
    use tauri::Emitter;

    let app_clone = app.clone();
    let result = tauri::async_runtime::spawn_blocking(move || {
        let mut emit_progress = |progress: crate::scanner::ScanProgress| {
            let _ = app_clone.emit("scan-progress", serde_json::json!({
                "currentPath": progress.current_path,
                "projectsFound": progress.projects_found,
            }));
        };
        crate::scanner::detect_projects_with_progress(root_path, &mut emit_progress)
    })
    .await
    .map_err(|e| format!("Scan task failed: {}", e))?;

    let _ = app.emit("scan-complete", ());
    result
}

impl Default for UpdateProjectRequest {
    fn default() -> Self {
        Self {
            name: None,
            path: None,
            description: None,
            language: None,
            status: None,
            tags: None,
            preferred_ide: None,
            is_favorite: None,
            is_hidden: None,
        }
    }
}


