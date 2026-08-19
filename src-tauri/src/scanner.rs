use crate::models::{Project, ProgrammingLanguage};
use crate::constants::{EXCLUDED_DIRS, EXCLUDED_LANG_DETECT};

pub struct ScanProgress {
    pub current_path: String,
    pub projects_found: usize,
}

fn build_extension_map() -> std::collections::HashMap<&'static str, ProgrammingLanguage> {
    [
        (".cs", ProgrammingLanguage::CSharp),
        (".py", ProgrammingLanguage::Python),
        (".rs", ProgrammingLanguage::Rust),
        (".js", ProgrammingLanguage::JavaScript),
        (".jsx", ProgrammingLanguage::JavaScript),
        (".ts", ProgrammingLanguage::TypeScript),
        (".tsx", ProgrammingLanguage::TypeScript),
        (".go", ProgrammingLanguage::Go),
        (".java", ProgrammingLanguage::Java),
        (".kt", ProgrammingLanguage::Java),
        (".cpp", ProgrammingLanguage::Cpp),
        (".c", ProgrammingLanguage::Cpp),
        (".h", ProgrammingLanguage::Cpp),
        (".hpp", ProgrammingLanguage::Cpp),
    ]
    .iter()
    .cloned()
    .collect()
}

pub fn detect_projects_with_progress(
    root_path: String,
    on_progress: &mut dyn FnMut(ScanProgress),
) -> Result<Vec<Project>, String> {
    detect_projects_inner(root_path, &mut Some(on_progress))
}

fn detect_projects_inner(
    root_path: String,
    on_progress: &mut Option<&mut dyn FnMut(ScanProgress)>,
) -> Result<Vec<Project>, String> {
    let mut projects = Vec::new();
    let extension_map = build_extension_map();

    let root = std::path::Path::new(&root_path);
    if !root.exists() {
        return Err(format!("Path does not exist: {}", root_path));
    }

    if let Some(cb) = on_progress.as_mut() {
        cb(ScanProgress {
            current_path: root_path.clone(),
            projects_found: 0,
        });
    }

    scan_dir(root, 0, 5, &extension_map, &mut projects, on_progress);

    Ok(projects)
}

fn scan_dir(
    path: &std::path::Path,
    depth: usize,
    max_depth: usize,
    extension_map: &std::collections::HashMap<&str, ProgrammingLanguage>,
    projects: &mut Vec<Project>,
    on_progress: &mut Option<&mut dyn FnMut(ScanProgress)>,
) {
    if depth > max_depth {
        return;
    }

    if let Ok(entries) = std::fs::read_dir(path) {
        for entry in entries.flatten() {
            let entry_path = entry.path();
            if entry_path.is_dir() {
                if entry_path.is_symlink() {
                    continue;
                }

                let dir_name = entry_path.file_name()
                    .unwrap_or_default()
                    .to_string_lossy();

                if EXCLUDED_DIRS.contains(&dir_name.as_ref()) {
                    continue;
                }

                let has_indicator = entry_path.join("Cargo.toml").exists()
                    || entry_path.join("package.json").exists()
                    || entry_path.join("go.mod").exists()
                    || entry_path.join("pom.xml").exists()
                    || entry_path.join("build.gradle").exists()
                    || entry_path.join("CMakeLists.txt").exists()
                    || entry_path.join("requirements.txt").exists()
                    || entry_path.join("setup.py").exists()
                    || entry_path.join("pyproject.toml").exists()
                    || entry_path.join("Gemfile").exists()
                    || entry_path.join("composer.json").exists()
                    || has_sln_file(&entry_path);

                if has_indicator {
                    let language = detect_language(&entry_path, extension_map);
                    if let Ok(project) = Project::new(
                        dir_name.to_string(),
                        entry_path.to_string_lossy().to_string(),
                    ) {
                        let mut p = project;
                        p.language = language;
                        projects.push(p);

                        if let Some(cb) = on_progress.as_mut() {
                            cb(ScanProgress {
                                current_path: entry_path.to_string_lossy().to_string(),
                                projects_found: projects.len(),
                            });
                        }
                    }
                } else {
                    scan_dir(
                        &entry_path,
                        depth + 1,
                        max_depth,
                        extension_map,
                        projects,
                        on_progress,
                    );
                }
            }
        }
    }
}

fn has_sln_file(path: &std::path::Path) -> bool {
    if let Ok(entries) = std::fs::read_dir(path) {
        for entry in entries.flatten() {
            if let Some(ext) = entry.path().extension() {
                if ext == "sln" {
                    return true;
                }
            }
        }
    }
    false
}

fn detect_language(
    path: &std::path::Path,
    extension_map: &std::collections::HashMap<&str, ProgrammingLanguage>,
) -> ProgrammingLanguage {
    fn scan_recursive(
        dir: &std::path::Path,
        depth: usize,
        max_depth: usize,
        extension_map: &std::collections::HashMap<&str, ProgrammingLanguage>,
    ) -> Option<ProgrammingLanguage> {
        if depth > max_depth {
            return None;
        }
        if let Ok(entries) = std::fs::read_dir(dir) {
            let mut dirs = Vec::new();

            for entry in entries.flatten() {
                let entry_path = entry.path();
                let is_dir = entry_path.is_dir();

                if is_dir {
                    if entry_path.is_symlink() {
                        continue;
                    }
                    let dir_name = entry_path.file_name().unwrap_or_default().to_string_lossy();
                    if !EXCLUDED_LANG_DETECT.contains(&dir_name.as_ref()) {
                        dirs.push(entry_path);
                    }
                } else {
                    let ext_str = entry_path.extension()
                        .map(|e| format!(".{}", e.to_string_lossy().to_lowercase()))
                        .unwrap_or_default();
                    if let Some(lang) = extension_map.get(ext_str.as_str()) {
                        return Some(lang.clone());
                    }
                }
            }

            for dir_path in dirs {
                if let Some(lang) = scan_recursive(&dir_path, depth + 1, max_depth, extension_map) {
                    return Some(lang);
                }
            }
        }
        None
    }

    scan_recursive(path, 0, 3, extension_map)
        .unwrap_or(ProgrammingLanguage::Other)
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::fs;
    use tempfile::TempDir;

    #[test]
    fn test_empty_directory() {
        let temp = TempDir::new().unwrap();
        let projects = detect_projects_with_progress(
            temp.path().to_string_lossy().to_string(),
            &mut |_| {},
        )
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

        let projects = detect_projects_with_progress(
            temp.path().to_string_lossy().to_string(),
            &mut |_| {},
        )
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

        let projects = detect_projects_with_progress(
            temp.path().to_string_lossy().to_string(),
            &mut |_| {},
        )
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

        let projects = detect_projects_with_progress(
            temp.path().to_string_lossy().to_string(),
            &mut |_| {},
        )
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

        let projects = detect_projects_with_progress(
            temp.path().to_string_lossy().to_string(),
            &mut |_| {},
        )
        .unwrap();
        assert_eq!(projects.len(), 0);
    }

    #[test]
    fn test_excluded_git_dir_skipped() {
        let temp = TempDir::new().unwrap();
        let git = temp.path().join(".git").join("objects");
        fs::create_dir_all(&git).unwrap();
        fs::write(git.join("Cargo.toml"), "[package]").unwrap();

        let projects = detect_projects_with_progress(
            temp.path().to_string_lossy().to_string(),
            &mut |_| {},
        )
        .unwrap();
        assert_eq!(projects.len(), 0);
    }

    #[test]
    fn test_excluded_target_dir_skipped() {
        let temp = TempDir::new().unwrap();
        let target = temp.path().join("target").join("debug");
        fs::create_dir_all(&target).unwrap();
        fs::write(target.join("Cargo.toml"), "[package]").unwrap();

        let projects = detect_projects_with_progress(
            temp.path().to_string_lossy().to_string(),
            &mut |_| {},
        )
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
        detect_projects_with_progress(
            temp.path().to_string_lossy().to_string(),
            &mut |_| {
                call_count += 1;
            },
        )
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

        let projects = detect_projects_with_progress(
            temp.path().to_string_lossy().to_string(),
            &mut |_| {},
        )
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

        let projects = detect_projects_with_progress(
            temp.path().to_string_lossy().to_string(),
            &mut |_| {},
        )
        .unwrap();
        assert_eq!(projects.len(), 1);
        assert_eq!(projects[0].name, "real_project");
    }
}
