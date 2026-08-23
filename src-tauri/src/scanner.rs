use crate::constants::{EXCLUDED_DIRS, EXCLUDED_LANG_DETECT, PROJECT_MARKERS};
use crate::models::{ProgrammingLanguage, Project};

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

                let dir_name = entry_path.file_name().unwrap_or_default().to_string_lossy();

                if EXCLUDED_DIRS.contains(&dir_name.as_ref()) {
                    continue;
                }

                let has_indicator = PROJECT_MARKERS
                    .iter()
                    .any(|marker| entry_path.join(marker).exists())
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
                    let ext_str = entry_path
                        .extension()
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

    scan_recursive(path, 0, 3, extension_map).unwrap_or(ProgrammingLanguage::Other)
}

#[cfg(test)]
#[path = "tests/scanner_tests.rs"]
mod tests;
