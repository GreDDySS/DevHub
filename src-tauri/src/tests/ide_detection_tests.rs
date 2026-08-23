use super::*;

#[test]
fn test_match_ide_pattern_vscode() {
    assert_eq!(
        match_ide_pattern("Visual Studio Code"),
        Some("Visual Studio Code")
    );
    assert_eq!(
        match_ide_pattern("Microsoft Visual Studio Code"),
        Some("Visual Studio Code")
    );
}

#[test]
fn test_match_ide_pattern_visual_studio() {
    assert_eq!(
        match_ide_pattern("Visual Studio 2022 Community"),
        Some("Visual Studio")
    );
    assert_eq!(
        match_ide_pattern("Microsoft Visual Studio Enterprise"),
        Some("Visual Studio")
    );
}

#[test]
fn test_match_ide_pattern_jetbrains() {
    assert_eq!(
        match_ide_pattern("JetBrains Rider 2026.1"),
        Some("JetBrains Rider")
    );
    assert_eq!(
        match_ide_pattern("JetBrains IntelliJ IDEA 2025.1"),
        Some("JetBrains IntelliJ IDEA")
    );
    assert_eq!(
        match_ide_pattern("JetBrains WebStorm 2025.1"),
        Some("JetBrains WebStorm")
    );
    assert_eq!(
        match_ide_pattern("JetBrains PyCharm 2025.1"),
        Some("JetBrains PyCharm")
    );
    assert_eq!(
        match_ide_pattern("JetBrains PyCharm Community Edition"),
        Some("JetBrains PyCharm Community")
    );
    assert_eq!(
        match_ide_pattern("JetBrains CLion 2025.1"),
        Some("JetBrains CLion")
    );
    assert_eq!(
        match_ide_pattern("JetBrains GoLand 2025.1"),
        Some("JetBrains GoLand")
    );
    assert_eq!(
        match_ide_pattern("JetBrains RustRover 2026.1"),
        Some("JetBrains RustRover")
    );
}

#[test]
fn test_match_ide_pattern_other() {
    assert_eq!(match_ide_pattern("Notepad++"), Some("Notepad++"));
    assert_eq!(
        match_ide_pattern("Sublime Text Build 4180"),
        Some("Sublime Text")
    );
}

#[test]
fn test_match_ide_pattern_no_match() {
    assert_eq!(match_ide_pattern("Google Chrome"), None);
    assert_eq!(match_ide_pattern("Mozilla Firefox"), None);
    assert_eq!(match_ide_pattern(""), None);
}

#[test]
fn test_match_ide_pattern_case_insensitive() {
    assert_eq!(
        match_ide_pattern("VISUAL STUDIO CODE"),
        Some("Visual Studio Code")
    );
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
    assert_eq!(
        match_linux_ide("Visual Studio Code", "/usr/bin/code"),
        Some("Visual Studio Code")
    );
    assert_eq!(
        match_linux_ide("code-oss", "/usr/bin/code-oss"),
        Some("Visual Studio Code")
    );
    assert_eq!(
        match_linux_ide("JetBrains Rider", "/opt/rider/bin/rider"),
        Some("JetBrains Rider")
    );
    assert_eq!(
        match_linux_ide("Sublime Text", "/usr/bin/subl"),
        Some("Sublime Text")
    );
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
