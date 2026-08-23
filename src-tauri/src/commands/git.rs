use crate::constants::{GIT_LOG_FIELD_SEPARATOR, GIT_LOG_RECORD_SEPARATOR};
use crate::models::{GitActivity, GitCommit};

fn is_git_repo(project_path: &str) -> bool {
    std::path::Path::new(project_path).join(".git").exists()
}

fn run_git(project_path: &str, args: &[&str]) -> Result<String, String> {
    let output = std::process::Command::new("git")
        .arg("-C")
        .arg(project_path)
        .args(args)
        .output()
        .map_err(|e| format!("Failed to run git (is it installed?): {}", e))?;

    if !output.status.success() {
        return Err(String::from_utf8_lossy(&output.stderr).trim().to_string());
    }
    Ok(String::from_utf8_lossy(&output.stdout).to_string())
}

fn normalize_remote_url(url: &str) -> Option<String> {
    let url = url.trim();
    if url.is_empty() {
        return None;
    }

    let url = url.strip_suffix(".git").unwrap_or(url);

    if let Some(rest) = url.strip_prefix("git@") {
        let (host, path) = rest.split_once(':')?;
        return Some(format!("https://{}/{}", host, path));
    }

    if let Some(rest) = url.strip_prefix("ssh://") {
        let rest = rest.strip_prefix("git@").unwrap_or(rest);
        return Some(format!("https://{}", rest));
    }

    if url.starts_with("http://") || url.starts_with("https://") {
        return Some(url.to_string());
    }

    None
}

#[tauri::command]
pub fn get_git_activity(
    project_path: String,
    limit: Option<usize>,
) -> Result<Option<GitActivity>, String> {
    if !is_git_repo(&project_path) {
        return Ok(None);
    }

    let limit = limit.unwrap_or(10).min(50);

    let branch = run_git(&project_path, &["rev-parse", "--abbrev-ref", "HEAD"])
        .map(|s| s.trim().to_string())
        .unwrap_or_else(|_| "unknown".to_string());

    let total_commits = run_git(&project_path, &["rev-list", "--count", "HEAD"])
        .ok()
        .and_then(|s| s.trim().parse::<u64>().ok())
        .unwrap_or(0);

    let web_url = run_git(&project_path, &["config", "--get", "remote.origin.url"])
        .ok()
        .and_then(|url| normalize_remote_url(&url));

    let format = format!(
        "%H{sep}%h{sep}%an{sep}%at{sep}%s{rec}",
        sep = GIT_LOG_FIELD_SEPARATOR,
        rec = GIT_LOG_RECORD_SEPARATOR
    );
    let max_count = format!("--max-count={}", limit);
    let out = run_git(
        &project_path,
        &[
            "log",
            "--date=unix",
            &format!("--pretty=format:{}", format),
            &max_count,
        ],
    ).unwrap_or_default();

    let commits: Vec<GitCommit> = out
        .split(GIT_LOG_RECORD_SEPARATOR)
        .filter(|record| !record.trim().is_empty())
        .filter_map(|record| {
            let fields: Vec<&str> = record.split(GIT_LOG_FIELD_SEPARATOR).collect();
            if fields.len() < 5 {
                return None;
            }
            let timestamp = fields[3].parse::<i64>().ok()?;
            Some(GitCommit {
                hash: fields[0].to_string(),
                short_hash: fields[1].to_string(),
                author: fields[2].to_string(),
                message: fields[4].to_string(),
                timestamp,
            })
        })
        .collect();

    if commits.is_empty() && total_commits == 0 {
        return Err("Failed to read git history".to_string());
    }

    Ok(Some(GitActivity {
        branch,
        total_commits,
        commits,
        web_url,
    }))
}

#[cfg(test)]
#[path = "../tests/git_tests.rs"]
mod tests;
