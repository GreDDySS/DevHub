use crate::models::IdeEntry;

pub fn scan_ides() -> Vec<IdeEntry> {
    let mut ides = Vec::new();

    #[cfg(target_os = "windows")]
    {
        use winreg::enums::*;
        use winreg::RegKey;

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
                                                        let is_match = match_exe_name(friendly_name, &name);
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

        let mut all_desktop_dirs: Vec<std::path::PathBuf> = vec![
            std::path::PathBuf::from("/usr/share/applications"),
            std::path::PathBuf::from("/usr/local/share/applications"),
            std::path::PathBuf::from("/var/lib/flatpak/exports/share/applications"),
            std::path::PathBuf::from("/var/lib/snapd/desktop/applications"),
        ];
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
                            let (name_value, exec_value) = parse_desktop_file(&content);

                            if name_value.is_empty() || exec_value.is_empty() {
                                continue;
                            }

                            if let Some(friendly_name) = match_linux_ide(&name_value, &exec_value) {
                                let exe_path = if exec_value.starts_with('/') {
                                    exec_value.clone()
                                } else {
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
                            }
                        }
                    }
                }
            }
        }
    }

    #[cfg(target_os = "macos")]
    {
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

pub fn validate_ide_path(path: &str) -> Result<(), String> {
    let p = std::path::Path::new(path);
    if !p.exists() {
        return Err(format!("IDE not found: {}", path));
    }
    if !p.is_file() {
        return Err(format!("IDE path is not a file: {}", path));
    }
    Ok(())
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
    fn test_validate_ide_path_nonexistent() {
        assert!(validate_ide_path("C:\\nonexistent.exe").is_err());
    }

    #[test]
    fn test_validate_ide_path_directory() {
        let result = validate_ide_path("C:\\Windows");
        assert!(result.is_err());
    }

    #[test]
    fn test_scan_ides_runs() {
        let _ides = scan_ides();
    }
}
