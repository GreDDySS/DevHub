use crate::models::*;
use crate::storage;

#[tauri::command]
pub fn get_todos(project_id: Option<String>) -> Vec<Todo> {
    storage::get_todos()
        .into_iter()
        .filter(|t| t.project_id == project_id)
        .collect()
}

#[tauri::command]
pub fn add_todo(title: String, project_id: Option<String>) -> Result<Todo, String> {
    let todo = Todo::new(title, project_id)?;
    storage::add_todo(todo.clone())?;
    Ok(todo)
}

#[tauri::command]
pub fn update_todo(id: String, request: UpdateTodoRequest) -> Result<Todo, String> {
    storage::update_todo(&id, request)
}

#[tauri::command]
pub fn toggle_todo(id: String) -> Result<Todo, String> {
    let todos = storage::get_todos();
    let todo = todos
        .iter()
        .find(|t| t.id == id)
        .ok_or_else(|| format!("Todo not found: {}", id))?;

    let new_value = !todo.is_completed;
    storage::update_todo(
        &id,
        UpdateTodoRequest {
            is_completed: Some(new_value),
            ..Default::default()
        },
    )
}

#[tauri::command]
pub fn delete_todo(id: String) -> Result<(), String> {
    storage::delete_todo(&id)
}

#[tauri::command]
pub fn clear_completed_todos() -> Result<usize, String> {
    storage::clear_completed_todos()
}
