use crate::models::*;
use crate::storage;
use crate::ide_detection;
use crate::constants::{GIT_LOG_FIELD_SEPARATOR, GIT_LOG_RECORD_SEPARATOR, STATS_EXCLUDED_DIRS};

#[tauri::command]
pub fn force_exit(app: tauri::AppHandle) {
    app.exit(0);
}

#[tauri::command]
pub fn get_data_dir() -> String {
    storage::get_data_dir_path()
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
    link.title = link.url.clone();
    storage::add_link(link.clone())?;
    Ok(link)
}

#[tauri::command]
pub fn add_link(
    url: String,
    title: Option<String>,
    project_id: Option<String>,
) -> Result<Link, String> {
    let mut link = Link::new(url)?;
    if let Some(t) = title {
        link.title = t;
    } else {
        link.title = link.url.clone();
    }
    link.project_id = project_id;
    storage::add_link(link.clone())?;
    Ok(link)
}

#[tauri::command]
pub fn delete_link(id: String) -> Result<(), String> {
    storage::delete_link(&id)
}

#[tauri::command]
pub fn get_todos(project_id: Option<String>) -> Vec<Todo> {
    storage::get_todos()
        .into_iter()
        .filter(|t| t.project_id == project_id)
        .collect()
}

#[tauri::command]
pub fn add_todo(title: String, project_id: Option<String>) -> Result<Todo, String> {
    let todo = Todo::new(title, project_id)?;
    storage::add_todo(todo.clone())?;
    Ok(todo)
}

#[tauri::command]
pub fn update_todo(id: String, request: UpdateTodoRequest) -> Result<Todo, String> {
    storage::update_todo(&id, request)
}

#[tauri::command]
pub fn toggle_todo(id: String) -> Result<Todo, String> {
    let todos = storage::get_todos();
    let todo = todos.iter().find(|t| t.id == id)
        .ok_or_else(|| format!("Todo not found: {}", id))?;

    let new_value = !todo.is_completed;
    storage::update_todo(&id, UpdateTodoRequest {
        is_completed: Some(new_value),
        ..Default::default()
    })
}

#[tauri::command]
pub fn delete_todo(id: String) -> Result<(), String> {
    storage::delete_todo(&id)
}

#[tauri::command]
pub fn clear_completed_todos() -> Result<usize, String> {
    storage::clear_completed_todos()
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
        .stdout(std::process::Stdio::null())
        .stderr(std::process::Stdio::null())
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

fn is_git_repo(project_path: &str) -> bool {
    std::path::Path::new(project_path).join(".git").exists()
}

fn run_git(project_path: &str, args: &[&str]) -> Result<String, String> {
    let output = std::process::Command::new("git")
        .arg("-C")
        .arg(project_path)
        .args(args)
        .output()
        .map_err(|e| format!("Failed to run git (is it installed?): {}", e))?;

    if !output.status.success() {
        return Err(String::from_utf8_lossy(&output.stderr).trim().to_string());
    }
    Ok(String::from_utf8_lossy(&output.stdout).to_string())
}

#[tauri::command]
pub fn get_git_activity(
    project_path: String,
    limit: Option<usize>,
) -> Result<Option<GitActivity>, String> {
    if !is_git_repo(&project_path) {
        return Ok(None);
    }

    let limit = limit.unwrap_or(10).min(50);

    let branch = run_git(&project_path, &["rev-parse", "--abbrev-ref", "HEAD"])
        .map(|s| s.trim().to_string())
        .unwrap_or_else(|_| "unknown".to_string());

    let total_commits = run_git(&project_path, &["rev-list", "--count", "HEAD"])
        .ok()
        .and_then(|s| s.trim().parse::<u64>().ok())
        .unwrap_or(0);

    let format = format!(
        "%H{sep}%h{sep}%an{sep}%at{sep}%s{rec}",
        sep = GIT_LOG_FIELD_SEPARATOR,
        rec = GIT_LOG_RECORD_SEPARATOR
    );
    let max_count = format!("--max-count={}", limit);
    let out = match run_git(
        &project_path,
        &["log", "--date=unix", &format!("--pretty=format:{}", format), &max_count],
    ) {
        Ok(out) => out,
        Err(_) => String::new(),
    };

    let commits: Vec<GitCommit> = out
        .split(GIT_LOG_RECORD_SEPARATOR)
        .filter(|record| !record.trim().is_empty())
        .filter_map(|record| {
            let fields: Vec<&str> = record.split(GIT_LOG_FIELD_SEPARATOR).collect();
            if fields.len() < 5 {
                return None;
            }
            let timestamp = fields[3].parse::<i64>().ok()?;
            Some(GitCommit {
                hash: fields[0].to_string(),
                short_hash: fields[1].to_string(),
                author: fields[2].to_string(),
                message: fields[4].to_string(),
                timestamp,
            })
        })
        .collect();

    if commits.is_empty() && total_commits == 0 {
        return Err("Failed to read git history".to_string());
    }

    Ok(Some(GitActivity {
        branch,
        total_commits,
        commits,
    }))
}

fn walk_stats(dir: &std::path::Path, stats: &mut ProjectStats, depth: u32, in_artifact: bool) {
    if depth > 32 {
        return;
    }
    let Ok(entries) = std::fs::read_dir(dir) else {
        return;
    };
    for entry in entries.flatten() {
        let Ok(file_type) = entry.file_type() else {
            continue;
        };
        if file_type.is_dir() {
            let name = entry.file_name().to_string_lossy().to_lowercase();
            let enters_artifact = in_artifact || STATS_EXCLUDED_DIRS.contains(&name.as_ref());
            if !enters_artifact {
                stats.dir_count += 1;
            }
            walk_stats(&entry.path(), stats, depth + 1, enters_artifact);
        } else if file_type.is_file() {
            if let Ok(meta) = entry.metadata() {
                let size = meta.len();
                stats.total_size += size;
                if !in_artifact {
                    stats.file_count += 1;
                    stats.source_size += size;
                    if let Ok(mtime) = meta.modified() {
                        if let Ok(secs) = mtime.duration_since(std::time::UNIX_EPOCH) {
                            stats.last_modified = stats.last_modified.max(secs.as_secs() as i64);
                        }
                    }
                }
            }
        }
    }
}

#[tauri::command]
pub async fn get_project_stats(
    project_path: String,
) -> Result<Option<ProjectStats>, String> {
    tauri::async_runtime::spawn_blocking(move || {
        let root = std::path::PathBuf::from(&project_path);
        if !root.is_dir() {
            return Ok(None);
        }
        let mut stats = ProjectStats::default();
        walk_stats(&root, &mut stats, 0, false);
        Ok(Some(stats))
    })
    .await
    .map_err(|e| format!("Stats task failed: {}", e))?
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

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_walk_stats_separates_source_and_artifacts() {
        let root = std::env::temp_dir().join(format!("devhub_stats_{}", uuid::Uuid::new_v4()));
        std::fs::create_dir_all(root.join("src")).unwrap();
        std::fs::create_dir_all(root.join("node_modules").join("pkg")).unwrap();
        std::fs::create_dir_all(root.join(".git")).unwrap();

        std::fs::write(root.join("README.md"), "hello").unwrap();
        std::fs::write(root.join("src").join("main.rs"), "fn main() {}").unwrap();
        std::fs::write(root.join("node_modules").join("pkg").join("x.js"), "// heavy").unwrap();
        std::fs::write(root.join(".git").join("index"), "binary".as_bytes()).unwrap();

        let mut stats = ProjectStats::default();
        walk_stats(&root, &mut stats, 0, false);

        let source_len = "hello".len() as u64 + "fn main() {}".len() as u64;
        let artifact_len = "// heavy".len() as u64 + "binary".len() as u64;

        assert_eq!(stats.file_count, 2);
        assert_eq!(stats.dir_count, 1);
        assert_eq!(stats.source_size, source_len);

        assert_eq!(stats.total_size, source_len + artifact_len);
        assert!(stats.last_modified > 0);

        std::fs::remove_dir_all(&root).ok();
    }

    #[test]
    fn test_walk_stats_missing_dir_is_noop() {
        let mut stats = ProjectStats::default();
        walk_stats(std::path::Path::new("Z:/definitely/not/here"), &mut stats, 0, false);
        assert_eq!(stats.file_count, 0);
        assert_eq!(stats.dir_count, 0);
        assert_eq!(stats.total_size, 0);
    }
}


