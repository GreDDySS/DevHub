use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use uuid::Uuid;

pub const CURRENT_SETTINGS_VERSION: u32 = 1;

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[derive(Default)]
pub enum ProjectStatus {
    #[default]
    Active,
    Inactive,
}


#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[derive(Default)]
pub enum ProgrammingLanguage {
    CSharp,
    Python,
    Rust,
    JavaScript,
    TypeScript,
    Go,
    Java,
    Cpp,
    #[default]
    Other,
}


#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[derive(Default)]
pub enum CloseAction {
    Exit,
    #[default]
    MinimizeToTray,
    Ask,
}


#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Project {
    pub id: String,
    pub name: String,
    pub path: String,
    pub description: String,
    pub language: ProgrammingLanguage,
    pub status: ProjectStatus,
    pub tags: Vec<String>,
    pub preferred_ide: Option<String>,
    pub is_favorite: bool,
    pub is_hidden: bool,
    pub created_at: DateTime<Utc>,
    pub updated_at: DateTime<Utc>,
}

impl Project {
    pub fn new(name: String, path: String) -> Result<Self, String> {
        if name.trim().is_empty() {
            return Err("Project name cannot be empty".to_string());
        }
        if name.len() > 200 {
            return Err("Project name cannot exceed 200 characters".to_string());
        }
        if path.trim().is_empty() {
            return Err("Project path cannot be empty".to_string());
        }

        let now = Utc::now();
        Ok(Self {
            id: Uuid::new_v4().to_string(),
            name: name.trim().to_string(),
            path: path.trim().to_string(),
            description: String::new(),
            language: ProgrammingLanguage::Other,
            status: ProjectStatus::Active,
            tags: Vec::new(),
            preferred_ide: None,
            is_favorite: false,
            is_hidden: false,
            created_at: now,
            updated_at: now,
        })
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Link {
    pub id: String,
    pub url: String,
    pub title: String,
    pub project_id: Option<String>,
    pub tags: Vec<String>,
    pub notes: String,
    pub captured_at: DateTime<Utc>,
    pub created_at: DateTime<Utc>,
    pub updated_at: DateTime<Utc>,
}

impl Link {
    pub fn new(url: String) -> Result<Self, String> {
        let url = url.trim().to_string();
        if url.is_empty() {
            return Err("URL cannot be empty".to_string());
        }
        if !url.starts_with("http://") && !url.starts_with("https://") {
            return Err("URL must start with http:// or https://".to_string());
        }

        let now = Utc::now();

        Ok(Self {
            id: Uuid::new_v4().to_string(),
            title: String::new(),
            url,
            project_id: None,
            tags: Vec::new(),
            notes: String::new(),
            captured_at: now,
            created_at: now,
            updated_at: now,
        })
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[derive(Default)]
pub enum TodoPriority {
    Low,
    #[default]
    Normal,
    High,
}


#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Todo {
    pub id: String,
    pub project_id: Option<String>,
    pub title: String,
    pub priority: TodoPriority,
    pub is_completed: bool,
    pub created_at: DateTime<Utc>,
    pub completed_at: Option<DateTime<Utc>>,
    pub updated_at: DateTime<Utc>,
}

impl Todo {
    pub fn new(title: String, project_id: Option<String>) -> Result<Self, String> {
        let title = title.trim().to_string();
        if title.is_empty() {
            return Err("Todo title cannot be empty".to_string());
        }
        if title.len() > 500 {
            return Err("Todo title cannot exceed 500 characters".to_string());
        }

        let now = Utc::now();
        Ok(Self {
            id: Uuid::new_v4().to_string(),
            project_id,
            title,
            priority: TodoPriority::default(),
            is_completed: false,
            created_at: now,
            completed_at: None,
            updated_at: now,
        })
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GitCommit {
    pub hash: String,
    pub short_hash: String,
    pub author: String,
    pub message: String,
    pub timestamp: i64,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GitActivity {
    pub branch: String,
    pub total_commits: u64,
    pub commits: Vec<GitCommit>,
    pub web_url: Option<String>,
}

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
pub struct ProjectStats {
    /// Files outside build-artifact/VCS dirs
    pub file_count: u64,
    pub dir_count: u64,
    /// Full size on disk, including artifacts (.git, node_modules, ...)
    pub total_size: u64,
    /// Size excluding artifacts
    pub source_size: u64,
    pub last_modified: i64,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct IdeEntry {
    pub name: String,
    pub path: String,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AppSettings {
    #[serde(default)]
    pub version: u32,
    pub ides: Vec<IdeEntry>,
    pub default_ide_index: usize,
    pub autostart_enabled: bool,
    pub close_action: CloseAction,
    pub is_dark_theme: bool,
    #[serde(default = "default_inactive_days")]
    pub inactive_days: u32,
    #[serde(default = "default_true")]
    pub statuses_enabled: bool,
}

fn default_inactive_days() -> u32 {
    30
}

fn default_true() -> bool {
    true
}

impl Default for AppSettings {
    fn default() -> Self {
        Self {
            version: CURRENT_SETTINGS_VERSION,
            ides: Vec::new(),
            default_ide_index: 0,
            autostart_enabled: false,
            close_action: CloseAction::MinimizeToTray,
            is_dark_theme: false,
            inactive_days: 30,
            statuses_enabled: true,
        }
    }
}

pub fn calculate_status(updated_at: DateTime<Utc>, inactive_days: u32) -> ProjectStatus {
    let elapsed = Utc::now() - updated_at;
    if elapsed.num_days() >= inactive_days as i64 {
        ProjectStatus::Inactive
    } else {
        ProjectStatus::Active
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ProjectFilter {
    pub search_query: Option<String>,
    pub status: Option<ProjectStatus>,
    pub languages: Option<Vec<ProgrammingLanguage>>,
    pub sort_by: Option<String>,
    pub tags: Option<Vec<String>>,
    pub show_hidden: Option<bool>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct CreateProjectRequest {
    pub name: String,
    pub path: String,
    pub description: Option<String>,
    pub language: Option<ProgrammingLanguage>,
}

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
pub struct UpdateProjectRequest {
    pub name: Option<String>,
    pub path: Option<String>,
    pub description: Option<String>,
    pub language: Option<ProgrammingLanguage>,
    pub status: Option<ProjectStatus>,
    pub tags: Option<Vec<String>>,
    pub preferred_ide: Option<Option<String>>,
    pub is_favorite: Option<bool>,
    pub is_hidden: Option<bool>,
}

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
pub struct UpdateTodoRequest {
    pub title: Option<String>,
    pub priority: Option<TodoPriority>,
    pub is_completed: Option<bool>,
}

#[cfg(test)]
#[path = "tests/models_tests.rs"]
mod tests;
