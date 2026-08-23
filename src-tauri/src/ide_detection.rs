use crate::constants::ide;
use crate::models::IdeEntry;

pub fn scan_ides() -> Vec<IdeEntry> {
    let mut ides = Vec::new();

    #[cfg(target_os = "windows")]
    {
        use winreg::enums::*;
        use winreg::RegKey;

        let hives = [HKEY_LOCAL_MACHINE, HKEY_CURRENT_USER];

        for hive in &hives {
            for subkey_path in ide::REGISTRY_UNINSTALL_SUBKEYS {
                let key = RegKey::predef(*hive).open_subkey(subkey_path);
                if let Ok(key) = key {
                    for entry_name in key.enum_keys().filter_map(|k| k.ok()) {
                        if let Ok(subkey) = key.open_subkey(&entry_name) {
                            let display_name: String =
                                subkey.get_value("DisplayName").unwrap_or_default();
                            let install_location: String =
                                subkey.get_value("InstallLocation").unwrap_or_default();

                            if display_name.is_empty() || install_location.is_empty() {
                                continue;
                            }

                            let display_lower = display_name.to_lowercase();

                            for (pattern, friendly_name) in ide::WINDOWS_IDE_PATTERNS {
                                if display_lower.contains(&pattern.to_lowercase()) {
                                    let install_path = std::path::Path::new(&install_location);
                                    if install_path.exists() {
                                        let search_dirs =
                                            [install_path.join("bin"), install_path.to_path_buf()];
                                        for dir in &search_dirs {
                                            if let Ok(entries) = std::fs::read_dir(dir) {
                                                for entry in entries.flatten() {
                                                    let p = entry.path();
                                                    if p.extension().is_some_and(|e| e == "exe") {
                                                        let name = p
                                                            .file_stem()
                                                            .unwrap_or_default()
                                                            .to_string_lossy();
                                                        let is_match =
                                                            match_exe_name(friendly_name, &name);
                                                        if is_match {
                                                            ides.push(IdeEntry {
                                                                name: friendly_name.to_string(),
                                                                path: p
                                                                    .to_string_lossy()
                                                                    .to_string(),
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
        let mut all_desktop_dirs: Vec<std::path::PathBuf> = ide::LINUX_DESKTOP_DIRS
            .iter()
            .map(std::path::PathBuf::from)
            .collect();
        if let Some(home) = dirs::home_dir() {
            all_desktop_dirs.push(home.join(ide::LINUX_USER_DESKTOP_DIR));
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
                                                String::from_utf8(o.stdout)
                                                    .ok()
                                                    .map(|s| s.trim().to_string())
                                            } else {
                                                None
                                            }
                                        })
                                        .unwrap_or_default()
                                };

                                if !exe_path.is_empty() && std::path::Path::new(&exe_path).exists()
                                {
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
        let mut app_dirs = vec![std::path::PathBuf::from(ide::MACOS_SYSTEM_APPS_DIR)];
        if let Some(home) = dirs::home_dir() {
            app_dirs.push(home.join(ide::MACOS_USER_APPS_DIR));
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

                        for (app_pattern, friendly_name, exe_suffix) in ide::MACOS_IDE_APPS {
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

    for (pattern, friendly_name) in ide::DISPLAY_NAME_PATTERNS {
        if lower.contains(pattern) {
            return Some(friendly_name);
        }
    }
    None
}

pub fn match_exe_name(friendly_name: &str, exe_stem: &str) -> bool {
    match friendly_name {
        ide::VS_CODE => exe_stem == "Code",
        ide::VISUAL_STUDIO => exe_stem == "devenv",
        ide::RIDER => exe_stem == "rider64",
        ide::INTELLIJ_IDEA => exe_stem == "idea64",
        ide::WEBSTORM => exe_stem == "webstorm64",
        ide::PYCHARM | ide::PYCHARM_COMMUNITY => exe_stem == "pycharm64",
        ide::CLION => exe_stem == "clion64",
        ide::GOLAND => exe_stem == "goland64",
        ide::RUSTROVER => exe_stem == "rustrover64",
        ide::NOTEPAD_PLUS_PLUS => exe_stem == "notepad++",
        ide::SUBLIME_TEXT => exe_stem == "sublime_text",
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

    for (pattern, friendly_name) in ide::LINUX_IDE_PATTERNS {
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
#[path = "tests/ide_detection_tests.rs"]
mod tests;
