use serde::{Deserialize, Serialize};
use chrono::{DateTime, Utc};
use uuid::Uuid;

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub enum ProjectStatus {
    Active,
    Completed,
    Paused,
    Archived,
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
pub enum LinkType {
    YouTube,
    Article,
    Repository,
    Documentation,
    Other,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "PascalCase")]
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
    #[serde(rename = "type")]
    pub link_type: LinkType,
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
        
        let link_type = Self::detect_type(&url);
        let now = Utc::now();
        
        Ok(Self {
            id: Uuid::new_v4().to_string(),
            title: String::new(),
            url,
            link_type,
            project_id: None,
            tags: Vec::new(),
            notes: String::new(),
            captured_at: now,
            created_at: now,
            updated_at: now,
        })
    }
    
    fn detect_type(url: &str) -> LinkType {
        if url.contains("youtube.com") || url.contains("youtu.be") {
            LinkType::YouTube
        } else if url.contains("github.com") || url.contains("gitlab.com") || url.contains("bitbucket.org") {
            LinkType::Repository
        } else if url.contains("docs.") || url.contains("/docs/") || url.contains("/documentation/") {
            LinkType::Documentation
        } else {
            LinkType::Article
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct IdeEntry {
    pub name: String,
    pub path: String,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AppSettings {
    pub ides: Vec<IdeEntry>,
    pub default_ide_index: usize,
    pub autostart_enabled: bool,
    pub close_action: CloseAction,
    pub is_dark_theme: bool,
}

impl Default for AppSettings {
    fn default() -> Self {
        Self {
            ides: Vec::new(),
            default_ide_index: 0,
            autostart_enabled: false,
            close_action: CloseAction::MinimizeToTray,
            is_dark_theme: false,
        }
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
        assert_eq!(l.link_type, LinkType::Repository);
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
    fn test_link_detect_youtube() {
        let link = Link::new("https://youtube.com/watch?v=123".to_string()).unwrap();
        assert_eq!(link.link_type, LinkType::YouTube);
    }

    #[test]
    fn test_link_detect_github() {
        let link = Link::new("https://github.com/user/repo".to_string()).unwrap();
        assert_eq!(link.link_type, LinkType::Repository);
    }

    #[test]
    fn test_link_detect_docs() {
        let link = Link::new("https://docs.example.com/guide".to_string()).unwrap();
        assert_eq!(link.link_type, LinkType::Documentation);
    }

    #[test]
    fn test_link_detect_article() {
        let link = Link::new("https://blog.example.com/post".to_string()).unwrap();
        assert_eq!(link.link_type, LinkType::Article);
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
}
