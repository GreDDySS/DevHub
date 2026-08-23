use super::*;
use std::fs;
use tempfile::TempDir;

#[test]
fn test_empty_directory() {
    let temp = TempDir::new().unwrap();
    let projects =
        detect_projects_with_progress(temp.path().to_string_lossy().to_string(), &mut |_| {})
            .unwrap();
    assert_eq!(projects.len(), 0);
}

#[test]
fn test_single_rust_project() {
    let temp = TempDir::new().unwrap();
    let dir = temp.path().join("my_rust_app");
    fs::create_dir(&dir).unwrap();
    fs::write(dir.join("Cargo.toml"), "[package]\nname = \"test\"").unwrap();
    fs::write(dir.join("main.rs"), "fn main() {}").unwrap();

    let projects =
        detect_projects_with_progress(temp.path().to_string_lossy().to_string(), &mut |_| {})
            .unwrap();
    assert_eq!(projects.len(), 1);
    assert_eq!(projects[0].name, "my_rust_app");
    assert_eq!(projects[0].language, ProgrammingLanguage::Rust);
}

#[test]
fn test_single_js_project() {
    let temp = TempDir::new().unwrap();
    let dir = temp.path().join("web_app");
    fs::create_dir(&dir).unwrap();
    fs::write(dir.join("package.json"), "{}").unwrap();

    let projects =
        detect_projects_with_progress(temp.path().to_string_lossy().to_string(), &mut |_| {})
            .unwrap();
    assert_eq!(projects.len(), 1);
    assert_eq!(projects[0].name, "web_app");
}

#[test]
fn test_nested_projects() {
    let temp = TempDir::new().unwrap();
    let parent = temp.path().join("workspace");
    let child = parent.join("sub_project");
    fs::create_dir_all(&child).unwrap();
    fs::write(child.join("Cargo.toml"), "[package]\nname = \"sub\"").unwrap();

    let projects =
        detect_projects_with_progress(temp.path().to_string_lossy().to_string(), &mut |_| {})
            .unwrap();
    assert_eq!(projects.len(), 1);
    assert_eq!(projects[0].name, "sub_project");
}

#[test]
fn test_excluded_node_modules_skipped() {
    let temp = TempDir::new().unwrap();
    let nm = temp.path().join("node_modules").join("some_package");
    fs::create_dir_all(&nm).unwrap();
    fs::write(nm.join("package.json"), "{}").unwrap();

    let projects =
        detect_projects_with_progress(temp.path().to_string_lossy().to_string(), &mut |_| {})
            .unwrap();
    assert_eq!(projects.len(), 0);
}

#[test]
fn test_excluded_git_dir_skipped() {
    let temp = TempDir::new().unwrap();
    let git = temp.path().join(".git").join("objects");
    fs::create_dir_all(&git).unwrap();
    fs::write(git.join("Cargo.toml"), "[package]").unwrap();

    let projects =
        detect_projects_with_progress(temp.path().to_string_lossy().to_string(), &mut |_| {})
            .unwrap();
    assert_eq!(projects.len(), 0);
}

#[test]
fn test_excluded_target_dir_skipped() {
    let temp = TempDir::new().unwrap();
    let target = temp.path().join("target").join("debug");
    fs::create_dir_all(&target).unwrap();
    fs::write(target.join("Cargo.toml"), "[package]").unwrap();

    let projects =
        detect_projects_with_progress(temp.path().to_string_lossy().to_string(), &mut |_| {})
            .unwrap();
    assert_eq!(projects.len(), 0);
}

#[test]
fn test_nonexistent_path() {
    let result = detect_projects_with_progress(
        "/nonexistent/path/that/does/not/exist".to_string(),
        &mut |_| {},
    );
    assert!(result.is_err());
}

#[test]
fn test_progress_callback_invoked() {
    let temp = TempDir::new().unwrap();
    let dir = temp.path().join("proj");
    fs::create_dir(&dir).unwrap();
    fs::write(dir.join("Cargo.toml"), "[package]").unwrap();

    let mut call_count = 0;
    detect_projects_with_progress(temp.path().to_string_lossy().to_string(), &mut |_| {
        call_count += 1;
    })
    .unwrap();
    assert!(call_count > 0);
}

#[test]
fn test_multiple_project_types() {
    let temp = TempDir::new().unwrap();

    let rust = temp.path().join("rust_app");
    fs::create_dir(&rust).unwrap();
    fs::write(rust.join("Cargo.toml"), "[package]").unwrap();

    let py = temp.path().join("py_app");
    fs::create_dir(&py).unwrap();
    fs::write(py.join("requirements.txt"), "flask").unwrap();

    let go = temp.path().join("go_app");
    fs::create_dir(&go).unwrap();
    fs::write(go.join("go.mod"), "module go_app").unwrap();

    let projects =
        detect_projects_with_progress(temp.path().to_string_lossy().to_string(), &mut |_| {})
            .unwrap();
    assert_eq!(projects.len(), 3);
}

#[test]
fn test_language_detection_rust() {
    let temp = TempDir::new().unwrap();
    let dir = temp.path().join("proj");
    fs::create_dir(&dir).unwrap();
    fs::write(dir.join("main.rs"), "fn main() {}").unwrap();

    let ext_map = build_extension_map();
    let lang = detect_language(&dir, &ext_map);
    assert_eq!(lang, ProgrammingLanguage::Rust);
}

#[test]
fn test_language_detection_python() {
    let temp = TempDir::new().unwrap();
    let dir = temp.path().join("proj");
    fs::create_dir(&dir).unwrap();
    fs::write(dir.join("app.py"), "print('hello')").unwrap();

    let ext_map = build_extension_map();
    let lang = detect_language(&dir, &ext_map);
    assert_eq!(lang, ProgrammingLanguage::Python);
}

#[test]
fn test_language_detection_typescript() {
    let temp = TempDir::new().unwrap();
    let dir = temp.path().join("proj");
    fs::create_dir(&dir).unwrap();
    fs::write(dir.join("index.ts"), "export {}").unwrap();

    let ext_map = build_extension_map();
    let lang = detect_language(&dir, &ext_map);
    assert_eq!(lang, ProgrammingLanguage::TypeScript);
}

#[test]
fn test_language_detection_case_insensitive() {
    let temp = TempDir::new().unwrap();
    let dir = temp.path().join("proj");
    fs::create_dir(&dir).unwrap();
    fs::write(dir.join("Main.JS"), "console.log()").unwrap();

    let ext_map = build_extension_map();
    let lang = detect_language(&dir, &ext_map);
    assert_eq!(lang, ProgrammingLanguage::JavaScript);
}

#[test]
fn test_language_detection_no_files() {
    let temp = TempDir::new().unwrap();
    let dir = temp.path().join("proj");
    fs::create_dir(&dir).unwrap();

    let ext_map = build_extension_map();
    let lang = detect_language(&dir, &ext_map);
    assert_eq!(lang, ProgrammingLanguage::Other);
}

#[test]
fn test_has_sln_file() {
    let temp = TempDir::new().unwrap();
    fs::write(temp.path().join("solution.sln"), "").unwrap();
    assert!(has_sln_file(temp.path()));
}

#[test]
fn test_has_sln_file_false() {
    let temp = TempDir::new().unwrap();
    fs::write(temp.path().join("readme.md"), "").unwrap();
    assert!(!has_sln_file(temp.path()));
}

#[cfg(unix)]
#[test]
fn test_symlink_not_followed() {
    use std::os::unix::fs::symlink;

    let temp = TempDir::new().unwrap();
    let real = temp.path().join("real_project");
    fs::create_dir(&real).unwrap();
    fs::write(real.join("Cargo.toml"), "[package]").unwrap();

    let link = temp.path().join("linked_project");
    symlink(&real, &link).unwrap();

    let projects =
        detect_projects_with_progress(temp.path().to_string_lossy().to_string(), &mut |_| {})
            .unwrap();
    assert_eq!(projects.len(), 1);
    assert_eq!(projects[0].name, "real_project");
}
