use crate::ide_detection;
use crate::models::AppSettings;
use crate::storage;

#[tauri::command]
pub fn force_exit(app: tauri::AppHandle) {
    app.exit(0);
}

#[tauri::command]
pub fn get_data_dir() -> String {
    storage::get_data_dir_path()
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
pub fn scan_ides() -> Vec<crate::models::IdeEntry> {
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
