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
    
    #[cfg(target_os = "windows")]
    {
        use winreg::enums::*;
        use winreg::RegKey;

        // Known IDE patterns: display name contains -> friendly name
        let known_ides: Vec<(&str, &str)> = vec![
            ("Visual Studio Code", "Visual Studio Code"),
            ("Visual Studio", "Visual Studio"),
            ("Rider", "JetBrains Rider"),
            ("IntelliJ", "JetBrains IntelliJ IDEA"),
            ("WebStorm", "JetBrains WebStorm"),
            ("PyCharm", "JetBrains PyCharm"),
            ("CLion", "JetBrains CLion"),
            ("GoLand", "JetBrains GoLand"),
            ("RustRover", "JetBrains RustRover"),
            ("Notepad++", "Notepad++"),
            ("Sublime Text", "Sublime Text"),
        ];

        let hives = [HKEY_LOCAL_MACHINE, HKEY_CURRENT_USER];
        let subkeys = [
            r"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            r"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
        ];

        for hive in &hives {
            for subkey_path in &subkeys {
                let key = RegKey::predef(*hive).open_subkey(subkey_path);
                if let Ok(key) = key {
                    for entry_name in key.enum_keys().filter_map(|k| k.ok()) {
                        if let Ok(subkey) = key.open_subkey(&entry_name) {
                            let display_name: String = subkey.get_value("DisplayName").unwrap_or_default();
                            let install_location: String = subkey.get_value("InstallLocation").unwrap_or_default();

                            if display_name.is_empty() || install_location.is_empty() {
                                continue;
                            }

                            let display_lower = display_name.to_lowercase();

                            for (pattern, friendly_name) in &known_ides {
                                if display_lower.contains(&pattern.to_lowercase()) {
                                    let install_path = std::path::Path::new(&install_location);
                                    if install_path.exists() {
                                        // Search bin/ and root for exe
                                        let search_dirs = [
                                            install_path.join("bin"),
                                            install_path.to_path_buf(),
                                        ];
                                        for dir in &search_dirs {
                                            if let Ok(entries) = std::fs::read_dir(dir) {
                                                for entry in entries.flatten() {
                                                    let p = entry.path();
                                                    if p.extension().map_or(false, |e| e == "exe") {
                                                        let name = p.file_stem().unwrap_or_default().to_string_lossy();
                                                        // Match known exe names
                                                        let is_match = match *friendly_name {
                                                            "Visual Studio Code" => name == "Code",
                                                            "Visual Studio" => name == "devenv",
                                                            "JetBrains Rider" => name == "rider64",
                                                            "JetBrains IntelliJ IDEA" => name == "idea64",
                                                            "JetBrains WebStorm" => name == "webstorm64",
                                                            "JetBrains PyCharm" => name == "pycharm64",
                                                            "JetBrains CLion" => name == "clion64",
                                                            "JetBrains GoLand" => name == "goland64",
                                                            "JetBrains RustRover" => name == "rustrover64",
                                                            "Notepad++" => name == "notepad++",
                                                            "Sublime Text" => name == "sublime_text",
                                                            _ => false,
                                                        };
                                                        if is_match {
                                                            ides.push(IdeEntry {
                                                                name: friendly_name.to_string(),
                                                                path: p.to_string_lossy().to_string(),
                                                            });
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    #[cfg(target_os = "linux")]
    {
        // Known IDE desktop file patterns
        let known_ides: Vec<(&str, &str)> = vec![
            ("code", "Visual Studio Code"),
            ("rider", "JetBrains Rider"),
            ("idea", "JetBrains IntelliJ IDEA"),
            ("webstorm", "JetBrains WebStorm"),
            ("pycharm", "JetBrains PyCharm"),
            ("clion", "JetBrains CLion"),
            ("goland", "JetBrains GoLand"),
            ("rustrover", "JetBrains RustRover"),
        ("sublime_text", "Sublime Text"),
        ("subl", "Sublime Text"),
        ];

        // Scan .desktop files in standard locations
        let desktop_dirs = vec![
            "/usr/share/applications",
            "/usr/local/share/applications",
            "/var/lib/flatpak/exports/share/applications",
            "/var/lib/snapd/desktop/applications",
        ];

        // Also check user-local desktop files
        if let Some(home) = dirs::home_dir() {
            let local_dir = home.join(".local/share/applications");
            if local_dir.exists() {
                // We'll process this below
            }
        }

        let mut all_desktop_dirs: Vec<std::path::PathBuf> = desktop_dirs
            .iter()
            .map(|p| std::path::PathBuf::from(p))
            .collect();
        if let Some(home) = dirs::home_dir() {
            all_desktop_dirs.push(home.join(".local/share/applications"));
        }

        for dir in &all_desktop_dirs {
            if !dir.exists() {
                continue;
            }
            if let Ok(entries) = std::fs::read_dir(dir) {
                for entry in entries.flatten() {
                    let path = entry.path();
                    if path.extension().map_or(false, |e| e == "desktop") {
                        if let Ok(content) = std::fs::read_to_string(&path) {
                            let mut name_value = String::new();
                            let mut exec_value = String::new();
                            for line in content.lines() {
                                if let Some(v) = line.strip_prefix("Name=") {
                                    name_value = v.to_string();
                                }
                                if let Some(v) = line.strip_prefix("Exec=") {
                                    // Exec can have %f, %u, etc. - take only the first part
                                    exec_value = v.split_whitespace().next().unwrap_or("").to_string();
                                }
                            }

                            if name_value.is_empty() || exec_value.is_empty() {
                                continue;
                            }

                            let name_lower = name_value.to_lowercase();
                            let exec_lower = exec_value.to_lowercase();

                            for (pattern, friendly_name) in &known_ides {
                                if name_lower.contains(&pattern.to_lowercase())
                                    || exec_lower.contains(&pattern.to_lowercase())
                                {
                                    // Resolve full path
                                    let exe_path = if exec_value.starts_with('/') {
                                        exec_value.clone()
                                    } else {
                                        // Try which to find it
                                        std::process::Command::new("which")
                                            .arg(&exec_value)
                                            .output()
                                            .ok()
                                            .and_then(|o| {
                                                if o.status.success() {
                                                    String::from_utf8(o.stdout).ok().map(|s| s.trim().to_string())
                                                } else {
                                                    None
                                                }
                                            })
                                            .unwrap_or_default()
                                    };

                                    if !exe_path.is_empty() && std::path::Path::new(&exe_path).exists() {
                                        ides.push(IdeEntry {
                                            name: friendly_name.to_string(),
                                            path: exe_path,
                                        });
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    #[cfg(target_os = "macos")]
    {
        // Known IDE app names
        let known_ides: Vec<(&str, &str, &str)> = vec![
            ("Visual Studio Code.app", "Visual Studio Code", "Contents/Resources/app/bin/code"),
            ("Rider.app", "JetBrains Rider", "Contents/MacOS/rider"),
            ("IntelliJ IDEA.app", "JetBrains IntelliJ IDEA", "Contents/MacOS/idea"),
            ("WebStorm.app", "JetBrains WebStorm", "Contents/MacOS/webstorm"),
            ("PyCharm.app", "JetBrains PyCharm", "Contents/MacOS/pycharm"),
            ("PyCharm CE.app", "JetBrains PyCharm Community", "Contents/MacOS/pycharm"),
            ("CLion.app", "JetBrains CLion", "Contents/MacOS/clion"),
            ("GoLand.app", "JetBrains GoLand", "Contents/MacOS/goland"),
            ("RustRover.app", "JetBrains RustRover", "Contents/MacOS/rustrover"),
            ("Sublime Text.app", "Sublime Text", "Contents/SharedSupport/bin/subl"),
        ];

        // Scan /Applications and ~/Applications
        let mut app_dirs = vec![std::path::PathBuf::from("/Applications")];
        if let Some(home) = dirs::home_dir() {
            app_dirs.push(home.join("Applications"));
        }

        for dir in &app_dirs {
            if !dir.exists() {
                continue;
            }
            if let Ok(entries) = std::fs::read_dir(dir) {
                for entry in entries.flatten() {
                    let path = entry.path();
                    if path.extension().map_or(false, |e| e == "app") {
                        let app_name = path.file_name().unwrap_or_default().to_string_lossy();
                        
                        for (app_pattern, friendly_name, exe_suffix) in &known_ides {
                            if app_name == *app_pattern {
                                let exe_path = path.join(exe_suffix);
                                if exe_path.exists() {
                                    ides.push(IdeEntry {
                                        name: friendly_name.to_string(),
                                        path: exe_path.to_string_lossy().to_string(),
                                    });
                                }
                                break;
                            }
                        }
                    }
                }
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

/// Matches a display name against known IDE patterns, returns friendly name if matched
#[allow(dead_code)]
pub fn match_ide_pattern(display_name: &str) -> Option<&'static str> {
    let lower = display_name.to_lowercase();
    let patterns: Vec<(&str, &str)> = vec![
        ("visual studio code", "Visual Studio Code"),
        ("visual studio", "Visual Studio"),
        ("rider", "JetBrains Rider"),
        ("intellij", "JetBrains IntelliJ IDEA"),
        ("webstorm", "JetBrains WebStorm"),
        ("pycharm community", "JetBrains PyCharm Community"),
        ("pycharm", "JetBrains PyCharm"),
        ("clion", "JetBrains CLion"),
        ("goland", "JetBrains GoLand"),
        ("rustrover", "JetBrains RustRover"),
        ("notepad++", "Notepad++"),
        ("sublime text", "Sublime Text"),
    ];

    for (pattern, friendly_name) in &patterns {
        if lower.contains(pattern) {
            return Some(friendly_name);
        }
    }
    None
}

/// Matches an exe file stem against expected executable names for a given IDE
#[allow(dead_code)]
pub fn match_exe_name(friendly_name: &str, exe_stem: &str) -> bool {
    match friendly_name {
        "Visual Studio Code" => exe_stem == "Code",
        "Visual Studio" => exe_stem == "devenv",
        "JetBrains Rider" => exe_stem == "rider64",
        "JetBrains IntelliJ IDEA" => exe_stem == "idea64",
        "JetBrains WebStorm" => exe_stem == "webstorm64",
        "JetBrains PyCharm" | "JetBrains PyCharm Community" => exe_stem == "pycharm64",
        "JetBrains CLion" => exe_stem == "clion64",
        "JetBrains GoLand" => exe_stem == "goland64",
        "JetBrains RustRover" => exe_stem == "rustrover64",
        "Notepad++" => exe_stem == "notepad++",
        "Sublime Text" => exe_stem == "sublime_text",
        _ => false,
    }
}

/// Parses a .desktop file content and returns (Name, Exec) values
#[allow(dead_code)]
pub fn parse_desktop_file(content: &str) -> (String, String) {
    let mut name = String::new();
    let mut exec = String::new();

    for line in content.lines() {
        if let Some(v) = line.strip_prefix("Name=") {
            name = v.to_string();
        }
        if let Some(v) = line.strip_prefix("Exec=") {
            exec = v.split_whitespace().next().unwrap_or("").to_string();
        }
    }

    (name, exec)
}

/// Matches a Linux desktop file name/exec against known IDE patterns
#[allow(dead_code)]
pub fn match_linux_ide(name: &str, exec: &str) -> Option<&'static str> {
    let name_lower = name.to_lowercase();
    let exec_lower = exec.to_lowercase();

    let patterns: Vec<(&str, &str)> = vec![
        ("code", "Visual Studio Code"),
        ("rider", "JetBrains Rider"),
        ("idea", "JetBrains IntelliJ IDEA"),
        ("webstorm", "JetBrains WebStorm"),
        ("pycharm", "JetBrains PyCharm"),
        ("clion", "JetBrains CLion"),
        ("goland", "JetBrains GoLand"),
        ("rustrover", "JetBrains RustRover"),
        ("sublime_text", "Sublime Text"),
        ("subl", "Sublime Text"),
    ];

    for (pattern, friendly_name) in &patterns {
        if name_lower.contains(pattern) || exec_lower.contains(pattern) {
            return Some(friendly_name);
        }
    }
    None
}

#[cfg(test)]
mod ide_tests {
    use super::*;

    #[test]
    fn test_match_ide_pattern_vscode() {
        assert_eq!(match_ide_pattern("Visual Studio Code"), Some("Visual Studio Code"));
        assert_eq!(match_ide_pattern("Microsoft Visual Studio Code"), Some("Visual Studio Code"));
    }

    #[test]
    fn test_match_ide_pattern_visual_studio() {
        assert_eq!(match_ide_pattern("Visual Studio 2022 Community"), Some("Visual Studio"));
        assert_eq!(match_ide_pattern("Microsoft Visual Studio Enterprise"), Some("Visual Studio"));
    }

    #[test]
    fn test_match_ide_pattern_jetbrains() {
        assert_eq!(match_ide_pattern("JetBrains Rider 2026.1"), Some("JetBrains Rider"));
        assert_eq!(match_ide_pattern("JetBrains IntelliJ IDEA 2025.1"), Some("JetBrains IntelliJ IDEA"));
        assert_eq!(match_ide_pattern("JetBrains WebStorm 2025.1"), Some("JetBrains WebStorm"));
        assert_eq!(match_ide_pattern("JetBrains PyCharm 2025.1"), Some("JetBrains PyCharm"));
        assert_eq!(match_ide_pattern("JetBrains PyCharm Community Edition"), Some("JetBrains PyCharm Community"));
        assert_eq!(match_ide_pattern("JetBrains CLion 2025.1"), Some("JetBrains CLion"));
        assert_eq!(match_ide_pattern("JetBrains GoLand 2025.1"), Some("JetBrains GoLand"));
        assert_eq!(match_ide_pattern("JetBrains RustRover 2026.1"), Some("JetBrains RustRover"));
    }

    #[test]
    fn test_match_ide_pattern_other() {
        assert_eq!(match_ide_pattern("Notepad++"), Some("Notepad++"));
        assert_eq!(match_ide_pattern("Sublime Text Build 4180"), Some("Sublime Text"));
    }

    #[test]
    fn test_match_ide_pattern_no_match() {
        assert_eq!(match_ide_pattern("Google Chrome"), None);
        assert_eq!(match_ide_pattern("Mozilla Firefox"), None);
        assert_eq!(match_ide_pattern(""), None);
    }

    #[test]
    fn test_match_ide_pattern_case_insensitive() {
        assert_eq!(match_ide_pattern("VISUAL STUDIO CODE"), Some("Visual Studio Code"));
        assert_eq!(match_ide_pattern("notepad++"), Some("Notepad++"));
    }

    #[test]
    fn test_match_exe_name_vscode() {
        assert!(match_exe_name("Visual Studio Code", "Code"));
        assert!(!match_exe_name("Visual Studio Code", "code"));
        assert!(!match_exe_name("Visual Studio Code", "devenv"));
    }

    #[test]
    fn test_match_exe_name_visual_studio() {
        assert!(match_exe_name("Visual Studio", "devenv"));
        assert!(!match_exe_name("Visual Studio", "Code"));
    }

    #[test]
    fn test_match_exe_name_jetbrains() {
        assert!(match_exe_name("JetBrains Rider", "rider64"));
        assert!(match_exe_name("JetBrains IntelliJ IDEA", "idea64"));
        assert!(match_exe_name("JetBrains WebStorm", "webstorm64"));
        assert!(match_exe_name("JetBrains PyCharm", "pycharm64"));
        assert!(match_exe_name("JetBrains PyCharm Community", "pycharm64"));
        assert!(match_exe_name("JetBrains CLion", "clion64"));
        assert!(match_exe_name("JetBrains GoLand", "goland64"));
        assert!(match_exe_name("JetBrains RustRover", "rustrover64"));
    }

    #[test]
    fn test_match_exe_name_notepad() {
        assert!(match_exe_name("Notepad++", "notepad++"));
        assert!(!match_exe_name("Notepad++", "notepad"));
    }

    #[test]
    fn test_match_exe_name_sublime() {
        assert!(match_exe_name("Sublime Text", "sublime_text"));
        assert!(!match_exe_name("Sublime Text", "sublime"));
    }

    #[test]
    fn test_match_exe_name_unknown() {
        assert!(!match_exe_name("Unknown IDE", "something"));
    }

    #[test]
    fn test_parse_desktop_file_basic() {
        let content = "[Desktop Entry]
Name=Visual Studio Code
Exec=/usr/bin/code --unity-launch %F
Type=Application";
        let (name, exec) = parse_desktop_file(content);
        assert_eq!(name, "Visual Studio Code");
        assert_eq!(exec, "/usr/bin/code");
    }

    #[test]
    fn test_parse_desktop_file_with_args() {
        let content = "[Desktop Entry]
Name=JetBrains Rider
Exec=rider %f
Type=Application";
        let (name, exec) = parse_desktop_file(content);
        assert_eq!(name, "JetBrains Rider");
        assert_eq!(exec, "rider");
    }

    #[test]
    fn test_parse_desktop_file_empty() {
        let (name, exec) = parse_desktop_file("");
        assert_eq!(name, "");
        assert_eq!(exec, "");
    }

    #[test]
    fn test_parse_desktop_file_no_exec() {
        let content = "[Desktop Entry]
Name=My App
Type=Application";
        let (name, exec) = parse_desktop_file(content);
        assert_eq!(name, "My App");
        assert_eq!(exec, "");
    }

    #[test]
    fn test_match_linux_ide() {
        assert_eq!(match_linux_ide("Visual Studio Code", "/usr/bin/code"), Some("Visual Studio Code"));
        assert_eq!(match_linux_ide("code-oss", "/usr/bin/code-oss"), Some("Visual Studio Code"));
        assert_eq!(match_linux_ide("JetBrains Rider", "/opt/rider/bin/rider"), Some("JetBrains Rider"));
        assert_eq!(match_linux_ide("Sublime Text", "/usr/bin/subl"), Some("Sublime Text"));
    }

    #[test]
    fn test_match_linux_ide_no_match() {
        assert_eq!(match_linux_ide("Firefox", "/usr/bin/firefox"), None);
        assert_eq!(match_linux_ide("LibreOffice", "/usr/bin/libreoffice"), None);
    }

    #[test]
    fn test_scan_ides_runs() {
        // Just verify it doesn't panic
        let _ides = scan_ides();
    }
}
