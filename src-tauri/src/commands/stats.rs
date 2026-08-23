use crate::constants::STATS_EXCLUDED_DIRS;
use crate::models::ProjectStats;

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
pub async fn get_project_stats(project_path: String) -> Result<Option<ProjectStats>, String> {
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

#[cfg(test)]
#[path = "../tests/stats_tests.rs"]
mod tests;
