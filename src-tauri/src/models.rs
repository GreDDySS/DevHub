use serde::{Deserialize, Serialize};
use chrono::{DateTime, Utc};
use uuid::Uuid;

pub const CURRENT_SETTINGS_VERSION: u32 = 1;

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub enum ProjectStatus {
    Active,
    Inactive,
}

impl Default for ProjectStatus {
    fn default() -> Self {
        Self::Active
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub enum ProgrammingLanguage {
    CSharp,
    Python,
    Rust,
    JavaScript,
    TypeScript,
    Go,
    Java,
    Cpp,
    Other,
}

impl Default for ProgrammingLanguage {
    fn default() -> Self {
        Self::Other
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub enum CloseAction {
    Exit,
    MinimizeToTray,
    Ask,
}

impl Default for CloseAction {
    fn default() -> Self {
        Self::MinimizeToTray
    }
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
pub enum TodoPriority {
    Low,
    Normal,
    High,
}

impl Default for TodoPriority {
    fn default() -> Self {
        Self::Normal
    }
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

#[derive(Debug, Clone, Serialize, Deserialize)]
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
mod tests {
    use super::*;

    #[test]
    fn test_project_new() {
        let project = Project::new("Test Project".to_string(), "/path/to/project".to_string());
        assert!(project.is_ok());
        
        let p = project.unwrap();
        assert_eq!(p.name, "Test Project");
        assert_eq!(p.path, "/path/to/project");
        assert_eq!(p.status, ProjectStatus::Active);
        assert_eq!(p.language, ProgrammingLanguage::Other);
        assert!(!p.is_favorite);
        assert!(!p.is_hidden);
    }

    #[test]
    fn test_project_new_empty_name() {
        let project = Project::new("".to_string(), "/path/to/project".to_string());
        assert!(project.is_err());
        assert!(project.unwrap_err().contains("empty"));
    }

    #[test]
    fn test_project_new_empty_path() {
        let project = Project::new("Test".to_string(), "".to_string());
        assert!(project.is_err());
        assert!(project.unwrap_err().contains("empty"));
    }

    #[test]
    fn test_project_new_long_name() {
        let long_name = "a".repeat(201);
        let project = Project::new(long_name, "/path".to_string());
        assert!(project.is_err());
        assert!(project.unwrap_err().contains("200 characters"));
    }

    #[test]
    fn test_project_new_trims_whitespace() {
        let project = Project::new("  Test  ".to_string(), "  /path  ".to_string()).unwrap();
        assert_eq!(project.name, "Test");
        assert_eq!(project.path, "/path");
    }

    #[test]
    fn test_link_new() {
        let link = Link::new("https://github.com/test/repo".to_string());
        assert!(link.is_ok());
        
        let l = link.unwrap();
        assert_eq!(l.url, "https://github.com/test/repo");
    }

    #[test]
    fn test_link_new_empty() {
        let link = Link::new("".to_string());
        assert!(link.is_err());
    }

    #[test]
    fn test_link_new_no_protocol() {
        let link = Link::new("github.com/test".to_string());
        assert!(link.is_err());
        assert!(link.unwrap_err().contains("http"));
    }

    #[test]
    fn test_close_action_serialization() {
        let exit = CloseAction::Exit;
        let json = serde_json::to_string(&exit).unwrap();
        assert_eq!(json, "\"Exit\"");
        
        let minimize = CloseAction::MinimizeToTray;
        let json = serde_json::to_string(&minimize).unwrap();
        assert_eq!(json, "\"MinimizeToTray\"");
        
        let ask = CloseAction::Ask;
        let json = serde_json::to_string(&ask).unwrap();
        assert_eq!(json, "\"Ask\"");
    }

    #[test]
    fn test_close_action_deserialization() {
        let exit: CloseAction = serde_json::from_str("\"Exit\"").unwrap();
        assert_eq!(exit, CloseAction::Exit);
        
        let minimize: CloseAction = serde_json::from_str("\"MinimizeToTray\"").unwrap();
        assert_eq!(minimize, CloseAction::MinimizeToTray);
        
        let ask: CloseAction = serde_json::from_str("\"Ask\"").unwrap();
        assert_eq!(ask, CloseAction::Ask);
    }

    #[test]
    fn test_settings_default() {
        let settings = AppSettings::default();
        assert!(settings.ides.is_empty());
        assert_eq!(settings.default_ide_index, 0);
        assert!(!settings.autostart_enabled);
        assert_eq!(settings.close_action, CloseAction::MinimizeToTray);
        assert!(!settings.is_dark_theme);
    }

    #[test]
    fn test_settings_serialization_roundtrip() {
        let settings = AppSettings::default();
        let json = serde_json::to_string(&settings).unwrap();
        let deserialized: AppSettings = serde_json::from_str(&json).unwrap();
        assert_eq!(deserialized.close_action, CloseAction::MinimizeToTray);
        assert_eq!(deserialized.autostart_enabled, false);
    }

    #[test]
    fn test_update_project_request_default() {
        let req = UpdateProjectRequest::default();
        assert!(req.name.is_none());
        assert!(req.path.is_none());
        assert!(req.description.is_none());
        assert!(req.is_favorite.is_none());
        assert!(req.is_hidden.is_none());
    }

    #[test]
    fn test_todo_new() {
        let todo = Todo::new("Write tests".to_string(), Some("p1".to_string()));
        assert!(todo.is_ok());

        let t = todo.unwrap();
        assert_eq!(t.title, "Write tests");
        assert_eq!(t.project_id, Some("p1".to_string()));
        assert!(!t.is_completed);
        assert_eq!(t.priority, TodoPriority::Normal);
        assert!(t.completed_at.is_none());
    }

    #[test]
    fn test_todo_new_empty_title() {
        let todo = Todo::new("   ".to_string(), None);
        assert!(todo.is_err());
        assert!(todo.unwrap_err().contains("empty"));
    }

    #[test]
    fn test_todo_new_trims_title() {
        let todo = Todo::new("  Task  ".to_string(), None).unwrap();
        assert_eq!(todo.title, "Task");
    }

    #[test]
    fn test_todo_roundtrip_serialization() {
        let todo = Todo::new("Serialize me".to_string(), None).unwrap();
        let json = serde_json::to_string(&todo).unwrap();
        let deserialized: Todo = serde_json::from_str(&json).unwrap();
        assert_eq!(deserialized.title, todo.title);
        assert_eq!(deserialized.id, todo.id);
        assert_eq!(deserialized.priority, todo.priority);
    }

    #[test]
    fn test_calculate_status_active() {
        let recent = Utc::now() - chrono::Duration::days(10);
        assert_eq!(calculate_status(recent, 30), ProjectStatus::Active);
    }

    #[test]
    fn test_calculate_status_inactive() {
        let old = Utc::now() - chrono::Duration::days(60);
        assert_eq!(calculate_status(old, 30), ProjectStatus::Inactive);
    }

    #[test]
    fn test_calculate_status_boundary() {
        let exactly = Utc::now() - chrono::Duration::days(30);
        assert_eq!(calculate_status(exactly, 30), ProjectStatus::Inactive);
    }
}
