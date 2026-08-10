use crate::models::*;
use crate::storage;

#[tauri::command]
pub fn force_exit(app: tauri::AppHandle) {
    app.exit(0);
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
        
        if let Some(show_hidden) = filter.show_hidden {
            if !show_hidden {
                projects.retain(|p| !p.is_hidden);
            }
        }
    }
    
    // Sort: favorites first, then by updated_at
    projects.sort_by(|a, b| {
        b.is_favorite.cmp(&a.is_favorite)
            .then_with(|| b.updated_at.cmp(&a.updated_at))
    });
    
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
    let mut ides = Vec::new();
    
    // Windows IDE paths
    #[cfg(target_os = "windows")]
    {
        let program_files = std::env::var("ProgramFiles").unwrap_or_default();
        let _program_files_x86 = std::env::var("ProgramFiles(x86)").unwrap_or_default();
        let local_app_data = std::env::var("LOCALAPPDATA").unwrap_or_default();
        
        let ide_paths = vec![
            ("Visual Studio Code", format!("{}\\Microsoft VS Code\\Code.exe", local_app_data)),
            ("Visual Studio 2022", format!("{}\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe", program_files)),
            ("JetBrains Rider", format!("{}\\JetBrains\\apps\\Rider\\*\\bin\\rider64.exe", local_app_data)),
            ("JetBrains IntelliJ IDEA", format!("{}\\JetBrains\\apps\\IDEA\\*\\bin\\idea64.exe", local_app_data)),
            ("JetBrains WebStorm", format!("{}\\JetBrains\\apps\\WebStorm\\*\\bin\\webstorm64.exe", local_app_data)),
            ("JetBrains PyCharm", format!("{}\\JetBrains\\apps\\PyCharm\\*\\bin\\pycharm64.exe", local_app_data)),
            ("JetBrains CLion", format!("{}\\JetBrains\\apps\\CLion\\*\\bin\\clion64.exe", local_app_data)),
            ("JetBrains GoLand", format!("{}\\JetBrains\\apps\\GoLand\\*\\bin\\goland64.exe", local_app_data)),
            ("Notepad++", format!("{}\\Notepad++\\notepad++.exe", program_files)),
            ("Sublime Text", format!("{}\\Sublime Text\\sublime_text.exe", program_files)),
        ];
        
        for (name, path) in ide_paths {
            // Handle wildcard paths
            if path.contains('*') {
                if let Some(parent) = std::path::Path::new(&path).parent() {
                    if let Ok(entries) = std::fs::read_dir(parent) {
                        for entry in entries.flatten() {
                            let entry_path = entry.path();
                            if entry_path.is_dir() {
                                let full_path = entry_path.join(
                                    std::path::Path::new(&path)
                                        .file_name()
                                        .unwrap_or_default()
                                );
                                if full_path.exists() {
                                    ides.push(IdeEntry {
                                        name: name.to_string(),
                                        path: full_path.to_string_lossy().to_string(),
                                    });
                                }
                            }
                        }
                    }
                }
            } else if std::path::Path::new(&path).exists() {
                ides.push(IdeEntry {
                    name: name.to_string(),
                    path,
                });
            }
        }
    }
    
    // Linux IDE paths
    #[cfg(target_os = "linux")]
    {
        let home = dirs::home_dir().unwrap_or_default();
        let ide_paths = vec![
            ("Visual Studio Code", "/usr/bin/code".to_string()),
            ("JetBrains Rider", format!("{}/.local/share/JetBrains/Toolbox/apps/Rider/*/bin/rider.sh", home.display())),
            ("JetBrains IntelliJ IDEA", format!("{}/.local/share/JetBrains/Toolbox/apps/IDEA/*/bin/idea.sh", home.display())),
            ("JetBrains WebStorm", format!("{}/.local/share/JetBrains/Toolbox/apps/WebStorm/*/bin/webstorm.sh", home.display())),
            ("JetBrains PyCharm", format!("{}/.local/share/JetBrains/Toolbox/apps/PyCharm/*/bin/pycharm.sh", home.display())),
            ("Sublime Text", "/usr/bin/subl".to_string()),
            ("Vim", "/usr/bin/vim".to_string()),
            ("NeoVim", "/usr/bin/nvim".to_string()),
        ];
        
        for (name, path) in ide_paths {
            if path.contains('*') {
                if let Some(parent) = std::path::Path::new(&path).parent() {
                    if let Ok(entries) = std::fs::read_dir(parent) {
                        for entry in entries.flatten() {
                            let entry_path = entry.path();
                            if entry_path.is_dir() {
                                let full_path = entry_path.join(
                                    std::path::Path::new(&path)
                                        .file_name()
                                        .unwrap_or_default()
                                );
                                if full_path.exists() {
                                    ides.push(IdeEntry {
                                        name: name.to_string(),
                                        path: full_path.to_string_lossy().to_string(),
                                    });
                                }
                            }
                        }
                    }
                }
            } else if std::path::Path::new(&path).exists() {
                ides.push(IdeEntry {
                    name: name.to_string(),
                    path,
                });
            }
        }
    }
    
    // macOS IDE paths
    #[cfg(target_os = "macos")]
    {
        let home = dirs::home_dir().unwrap_or_default();
        let ide_paths = vec![
            ("Visual Studio Code", "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code".to_string()),
            ("JetBrains Rider", "/Applications/Rider.app/Contents/MacOS/rider".to_string()),
            ("JetBrains IntelliJ IDEA", "/Applications/IntelliJ IDEA.app/Contents/MacOS/idea".to_string()),
            ("JetBrains WebStorm", "/Applications/WebStorm.app/Contents/MacOS/webstorm".to_string()),
            ("JetBrains PyCharm", "/Applications/PyCharm.app/Contents/MacOS/pycharm".to_string()),
            ("Sublime Text", "/Applications/Sublime Text.app/Contents/SharedSupport/bin/subl".to_string()),
            ("Vim", "/usr/bin/vim".to_string()),
            ("NeoVim", "/opt/homebrew/bin/nvim".to_string()),
        ];
        
        for (name, path) in ide_paths {
            if std::path::Path::new(&path).exists() {
                ides.push(IdeEntry {
                    name: name.to_string(),
                    path,
                });
            }
        }
    }
    
    ides
}

#[tauri::command]
pub fn open_in_explorer(path: String) -> Result<(), String> {
    open::that(&path).map_err(|e| format!("Failed to open in explorer: {}", e))
}

#[tauri::command]
pub fn open_in_ide(project_path: String, ide_path: String) -> Result<(), String> {
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
pub fn copy_to_clipboard(_text: String) -> Result<(), String> {
    // Use the clipboard plugin from the frontend
    Ok(())
}

#[tauri::command]
pub fn detect_projects(root_path: String) -> Result<Vec<Project>, String> {
    let mut projects = Vec::new();
    let excluded_folders: Vec<&str> = vec![
        "node_modules", ".git", "bin", "obj", ".vs", ".vscode",
        "target", "dist", "build", ".next", "__pycache__",
    ];
    
    let extension_map: std::collections::HashMap<&str, ProgrammingLanguage> = [
        (".cs", ProgrammingLanguage::CSharp),
        (".py", ProgrammingLanguage::Python),
        (".rs", ProgrammingLanguage::Rust),
        (".js", ProgrammingLanguage::JavaScript),
        (".ts", ProgrammingLanguage::TypeScript),
        (".go", ProgrammingLanguage::Go),
        (".java", ProgrammingLanguage::Java),
        (".cpp", ProgrammingLanguage::Cpp),
        (".c", ProgrammingLanguage::Cpp),
        (".h", ProgrammingLanguage::Cpp),
    ].iter().cloned().collect();
    
    fn scan_dir(
        path: &std::path::Path,
        depth: usize,
        max_depth: usize,
        excluded_folders: &[&str],
        extension_map: &std::collections::HashMap<&str, ProgrammingLanguage>,
        projects: &mut Vec<Project>,
    ) {
        if depth > max_depth {
            return;
        }
        
        if let Ok(entries) = std::fs::read_dir(path) {
            for entry in entries.flatten() {
                let entry_path = entry.path();
                if entry_path.is_dir() {
                    let dir_name = entry_path.file_name()
                        .unwrap_or_default()
                        .to_string_lossy();
                    
                    if excluded_folders.contains(&dir_name.as_ref()) {
                        continue;
                    }
                    
                    // Check for project indicators
                    let has_indicator = entry_path.join("Cargo.toml").exists()
                        || entry_path.join("package.json").exists()
                        || entry_path.join("go.mod").exists()
                        || entry_path.join("pom.xml").exists()
                        || entry_path.join("build.gradle").exists()
                        || entry_path.join("CMakeLists.txt").exists()
                        || entry_path.join("sln").exists();
                    
                    if has_indicator {
                        let language = detect_language(&entry_path, extension_map);
                        if let Ok(project) = Project::new(
                            dir_name.to_string(),
                            entry_path.to_string_lossy().to_string(),
                        ) {
                            let mut p = project;
                            p.language = language;
                            projects.push(p);
                        }
                    } else {
                        scan_dir(
                            &entry_path,
                            depth + 1,
                            max_depth,
                            excluded_folders,
                            extension_map,
                            projects,
                        );
                    }
                }
            }
        }
    }
    
    fn detect_language(
        path: &std::path::Path,
        extension_map: &std::collections::HashMap<&str, ProgrammingLanguage>,
    ) -> ProgrammingLanguage {
        if let Ok(entries) = std::fs::read_dir(path) {
            for entry in entries.flatten() {
                if let Some(ext) = entry.path().extension() {
                    if let Some(lang) = extension_map.get(ext.to_string_lossy().as_ref()) {
                        return lang.clone();
                    }
                }
            }
        }
        ProgrammingLanguage::Other
    }
    
    let root = std::path::Path::new(&root_path);
    if !root.exists() {
        return Err(format!("Path does not exist: {}", root_path));
    }
    
    scan_dir(root, 0, 5, &excluded_folders, &extension_map, &mut projects);
    
    Ok(projects)
}

impl Default for UpdateProjectRequest {
    fn default() -> Self {
        Self {
            name: None,
            path: None,
            description: None,
            notes: None,
            language: None,
            status: None,
            tags: None,
            preferred_ide: None,
            is_favorite: None,
            is_hidden: None,
            auto_status_enabled: None,
        }
    }
}
