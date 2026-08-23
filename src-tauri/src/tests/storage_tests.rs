use super::*;
use crate::models::{Link, Project, Todo, UpdateProjectRequest};

fn setup_test_dir() -> PathBuf {
    let dir = std::env::temp_dir().join(format!("devhub_test_{}", uuid::Uuid::new_v4()));
    fs::create_dir_all(&dir).unwrap();
    std::env::set_var("DEVHUB_TEST_DIR", dir.to_str().unwrap());
    dir
}

fn cleanup_test_dir(dir: &PathBuf) {
    std::env::remove_var("DEVHUB_TEST_DIR");
    let _ = fs::remove_dir_all(dir);
}

fn reset_storage() {
    *PROJECTS.lock().unwrap() = Vec::new();
    *LINKS.lock().unwrap() = Vec::new();
    *TODOS.lock().unwrap() = Vec::new();
    *SETTINGS.lock().unwrap() = AppSettings::default();
}

#[test]
fn test_migrate_settings_from_old_version() {
    let mut settings = AppSettings::default();
    settings.version = 0;
    let migrated = migrate_settings(settings);
    assert_eq!(migrated.version, CURRENT_SETTINGS_VERSION);
}

#[test]
fn test_migrate_settings_already_current() {
    let mut settings = AppSettings::default();
    settings.version = CURRENT_SETTINGS_VERSION;
    let migrated = migrate_settings(settings);
    assert_eq!(migrated.version, CURRENT_SETTINGS_VERSION);
}

#[test]
fn test_init_storage_missing_files() {
    let dir = setup_test_dir();
    reset_storage();
    let result = init_storage();
    assert!(result.is_ok());
    let projects = get_projects();
    assert_eq!(projects.len(), 0);
    let settings = get_settings();
    assert_eq!(settings.version, CURRENT_SETTINGS_VERSION);
    cleanup_test_dir(&dir);
}

#[test]
fn test_corrupted_settings_json() {
    let dir = setup_test_dir();
    reset_storage();
    let data_dir = get_data_dir();
    fs::create_dir_all(&data_dir).unwrap();
    fs::write(data_dir.join("settings.json"), "not valid json {{{").unwrap();
    let result = init_storage();
    assert!(result.is_err());
    cleanup_test_dir(&dir);
}

#[test]
fn test_corrupted_projects_json() {
    let dir = setup_test_dir();
    reset_storage();
    let data_dir = get_data_dir();
    fs::create_dir_all(&data_dir).unwrap();
    fs::write(data_dir.join("projects.json"), "definitely not json").unwrap();
    let result = init_storage();
    assert!(result.is_err());
    cleanup_test_dir(&dir);
}

#[test]
fn test_corrupted_links_json() {
    let dir = setup_test_dir();
    reset_storage();
    let data_dir = get_data_dir();
    fs::create_dir_all(&data_dir).unwrap();
    fs::write(data_dir.join("links.json"), "{bad json").unwrap();
    let result = init_storage();
    assert!(result.is_err());
    cleanup_test_dir(&dir);
}

#[test]
fn test_add_and_get_project() {
    let dir = setup_test_dir();
    reset_storage();
    let project = Project::new("Test".to_string(), "/test/path".to_string()).unwrap();
    add_project(project).unwrap();
    let projects = get_projects();
    assert_eq!(projects.len(), 1);
    assert_eq!(projects[0].name, "Test");
    assert_eq!(projects[0].path, "/test/path");
    cleanup_test_dir(&dir);
}

#[test]
fn test_add_multiple_projects() {
    let dir = setup_test_dir();
    reset_storage();
    for i in 0..5 {
        let p = Project::new(format!("Project {}", i), format!("/path/{}", i)).unwrap();
        add_project(p).unwrap();
    }
    let projects = get_projects();
    assert_eq!(projects.len(), 5);
    cleanup_test_dir(&dir);
}

#[test]
fn test_update_project_single_field() {
    let dir = setup_test_dir();
    reset_storage();
    let project = Project::new("Original".to_string(), "/path".to_string()).unwrap();
    let id = project.id.clone();
    add_project(project).unwrap();

    let update = UpdateProjectRequest {
        name: Some("Renamed".to_string()),
        ..Default::default()
    };
    let updated = update_project(&id, update).unwrap();
    assert_eq!(updated.name, "Renamed");
    assert_eq!(updated.path, "/path");
    cleanup_test_dir(&dir);
}

#[test]
fn test_update_project_multiple_fields() {
    let dir = setup_test_dir();
    reset_storage();
    let project = Project::new("Original".to_string(), "/path".to_string()).unwrap();
    let id = project.id.clone();
    add_project(project).unwrap();

    let update = UpdateProjectRequest {
        name: Some("New Name".to_string()),
        description: Some("A description".to_string()),
        is_favorite: Some(true),
        ..Default::default()
    };
    let updated = update_project(&id, update).unwrap();
    assert_eq!(updated.name, "New Name");
    assert_eq!(updated.description, "A description");
    assert!(updated.is_favorite);
    cleanup_test_dir(&dir);
}

#[test]
fn test_update_project_not_found() {
    let dir = setup_test_dir();
    reset_storage();
    let result = update_project(
        "nonexistent-id",
        UpdateProjectRequest {
            name: Some("X".to_string()),
            ..Default::default()
        },
    );
    assert!(result.is_err());
    assert!(result.unwrap_err().contains("not found"));
    cleanup_test_dir(&dir);
}

#[test]
fn test_delete_project() {
    let dir = setup_test_dir();
    reset_storage();
    let project = Project::new("ToDelete".to_string(), "/path".to_string()).unwrap();
    let id = project.id.clone();
    add_project(project).unwrap();
    assert_eq!(get_projects().len(), 1);

    delete_project(&id).unwrap();
    assert_eq!(get_projects().len(), 0);
    cleanup_test_dir(&dir);
}

#[test]
fn test_delete_project_not_found() {
    let dir = setup_test_dir();
    reset_storage();
    let result = delete_project("nonexistent-id");
    assert!(result.is_err());
    assert!(result.unwrap_err().contains("not found"));
    cleanup_test_dir(&dir);
}

#[test]
fn test_add_and_get_link() {
    let dir = setup_test_dir();
    reset_storage();
    let link = Link::new("https://example.com".to_string()).unwrap();
    add_link(link).unwrap();
    let links = get_links();
    assert_eq!(links.len(), 1);
    assert_eq!(links[0].url, "https://example.com");
    cleanup_test_dir(&dir);
}

#[test]
fn test_delete_link() {
    let dir = setup_test_dir();
    reset_storage();
    let link = Link::new("https://example.com".to_string()).unwrap();
    let id = link.id.clone();
    add_link(link).unwrap();
    assert_eq!(get_links().len(), 1);

    delete_link(&id).unwrap();
    assert_eq!(get_links().len(), 0);
    cleanup_test_dir(&dir);
}

#[test]
fn test_delete_link_not_found() {
    let dir = setup_test_dir();
    reset_storage();
    let result = delete_link("nonexistent-id");
    assert!(result.is_err());
    cleanup_test_dir(&dir);
}

#[test]
fn test_delete_project_detaches_links() {
    let dir = setup_test_dir();
    reset_storage();
    let project = Project::new("WithLinks".to_string(), "/path/withlinks".to_string()).unwrap();
    let pid = project.id.clone();
    add_project(project).unwrap();

    let mut attached = Link::new("https://example.com/a".to_string()).unwrap();
    attached.project_id = Some(pid.clone());
    add_link(attached).unwrap();

    delete_project(&pid).unwrap();

    let links = get_links();
    assert_eq!(links.len(), 1);
    assert_eq!(links[0].project_id, None);
    cleanup_test_dir(&dir);
}

#[test]
fn test_add_and_get_todo() {
    let dir = setup_test_dir();
    reset_storage();
    let todo = Todo::new("Task A".to_string(), None).unwrap();
    add_todo(todo).unwrap();
    let todos = get_todos();
    assert_eq!(todos.len(), 1);
    assert_eq!(todos[0].title, "Task A");
    cleanup_test_dir(&dir);
}

#[test]
fn test_get_todos_sorts_incomplete_first_by_priority() {
    let dir = setup_test_dir();
    reset_storage();

    let mut low = Todo::new("Low task".to_string(), None).unwrap();
    low.priority = crate::models::TodoPriority::Low;
    let mut high = Todo::new("High task".to_string(), None).unwrap();
    high.priority = crate::models::TodoPriority::High;
    let normal = Todo::new("Normal task".to_string(), None).unwrap();

    add_todo(low.clone()).unwrap();
    add_todo(high.clone()).unwrap();
    add_todo(normal.clone()).unwrap();

    update_todo(
        &high.id,
        crate::models::UpdateTodoRequest {
            is_completed: Some(true),
            ..Default::default()
        },
    )
    .unwrap();

    let todos = get_todos();
    assert_eq!(todos[0].title, "Normal task");
    assert_eq!(todos[1].title, "Low task");
    assert_eq!(todos[2].title, "High task");
    assert!(todos[2].is_completed);
    cleanup_test_dir(&dir);
}

#[test]
fn test_update_todo_title_and_priority() {
    let dir = setup_test_dir();
    reset_storage();
    let todo = Todo::new("Before".to_string(), None).unwrap();
    let id = todo.id.clone();
    add_todo(todo).unwrap();

    let updated = update_todo(
        &id,
        crate::models::UpdateTodoRequest {
            title: Some("After".to_string()),
            priority: Some(crate::models::TodoPriority::High),
            ..Default::default()
        },
    )
    .unwrap();
    assert_eq!(updated.title, "After");
    assert_eq!(updated.priority, crate::models::TodoPriority::High);
    assert!(!updated.is_completed);
    cleanup_test_dir(&dir);
}

#[test]
fn test_toggle_todo_sets_completed_at() {
    let dir = setup_test_dir();
    reset_storage();
    let todo = Todo::new("Toggle me".to_string(), None).unwrap();
    let id = todo.id.clone();
    add_todo(todo).unwrap();

    let completed = update_todo(
        &id,
        crate::models::UpdateTodoRequest {
            is_completed: Some(true),
            ..Default::default()
        },
    )
    .unwrap();
    assert!(completed.is_completed);
    assert!(completed.completed_at.is_some());

    let uncompleted = update_todo(
        &id,
        crate::models::UpdateTodoRequest {
            is_completed: Some(false),
            ..Default::default()
        },
    )
    .unwrap();
    assert!(!uncompleted.is_completed);
    assert!(uncompleted.completed_at.is_none());
    cleanup_test_dir(&dir);
}

#[test]
fn test_update_todo_empty_title_rejected() {
    let dir = setup_test_dir();
    reset_storage();
    let todo = Todo::new("Keep me".to_string(), None).unwrap();
    let id = todo.id.clone();
    add_todo(todo).unwrap();

    let result = update_todo(
        &id,
        crate::models::UpdateTodoRequest {
            title: Some("   ".to_string()),
            ..Default::default()
        },
    );
    assert!(result.is_err());
    cleanup_test_dir(&dir);
}

#[test]
fn test_delete_todo() {
    let dir = setup_test_dir();
    reset_storage();
    let todo = Todo::new("Delete me".to_string(), None).unwrap();
    let id = todo.id.clone();
    add_todo(todo).unwrap();
    delete_todo(&id).unwrap();
    assert_eq!(get_todos().len(), 0);
    cleanup_test_dir(&dir);
}

#[test]
fn test_delete_todo_not_found() {
    let dir = setup_test_dir();
    reset_storage();
    let result = delete_todo("nonexistent-id");
    assert!(result.is_err());
    cleanup_test_dir(&dir);
}

#[test]
fn test_clear_completed_todos() {
    let dir = setup_test_dir();
    reset_storage();
    for i in 0..3 {
        let t = Todo::new(format!("Task {}", i), None).unwrap();
        add_todo(t).unwrap();
    }
    let todos = get_todos();
    for t in todos.iter().take(2) {
        update_todo(
            &t.id,
            crate::models::UpdateTodoRequest {
                is_completed: Some(true),
                ..Default::default()
            },
        )
        .unwrap();
    }

    let removed = clear_completed_todos().unwrap();
    assert_eq!(removed, 2);
    assert_eq!(get_todos().len(), 1);

    let removed_again = clear_completed_todos().unwrap();
    assert_eq!(removed_again, 0);
    cleanup_test_dir(&dir);
}

#[test]
fn test_todos_persist_to_disk() {
    let dir = setup_test_dir();
    reset_storage();
    let todo = Todo::new("Persisted".to_string(), Some("proj-1".to_string())).unwrap();
    add_todo(todo).unwrap();

    let data_dir = get_data_dir();
    let content = fs::read_to_string(data_dir.join("todos.json")).unwrap();
    assert!(content.contains("Persisted"));
    assert!(content.contains("proj-1"));
    cleanup_test_dir(&dir);
}

#[test]
fn test_update_settings_roundtrip() {
    let dir = setup_test_dir();
    reset_storage();
    let mut settings = AppSettings::default();
    settings.is_dark_theme = true;
    settings.autostart_enabled = true;
    update_settings(settings.clone()).unwrap();

    let loaded = get_settings();
    assert!(loaded.is_dark_theme);
    assert!(loaded.autostart_enabled);
    cleanup_test_dir(&dir);
}

#[test]
fn test_concurrent_add_projects() {
    let dir = setup_test_dir();
    reset_storage();
    let mut handles = vec![];
    for i in 0..10 {
        handles.push(std::thread::spawn(move || {
            let p = Project::new(format!("Project {}", i), format!("/path/{}", i)).unwrap();
            add_project(p).unwrap();
        }));
    }
    for h in handles {
        h.join().unwrap();
    }
    let projects = get_projects();
    assert_eq!(projects.len(), 10);
    cleanup_test_dir(&dir);
}

#[test]
fn test_project_roundtrip_serialization() {
    let project = Project::new("Roundtrip".to_string(), "/test".to_string()).unwrap();
    let json = serde_json::to_string(&project).unwrap();
    let deserialized: Project = serde_json::from_str(&json).unwrap();
    assert_eq!(deserialized.name, project.name);
    assert_eq!(deserialized.path, project.path);
    assert_eq!(deserialized.id, project.id);
}

#[test]
fn test_link_roundtrip_serialization() {
    let link = Link::new("https://github.com/test".to_string()).unwrap();
    let json = serde_json::to_string(&link).unwrap();
    let deserialized: Link = serde_json::from_str(&json).unwrap();
    assert_eq!(deserialized.url, link.url);
    assert_eq!(deserialized.id, link.id);
}
