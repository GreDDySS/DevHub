use crate::models::*;
use crate::storage;

#[tauri::command]
pub fn get_links() -> Vec<Link> {
    storage::get_links()
}

#[tauri::command]
pub fn capture_link(url: String) -> Result<Link, String> {
    let mut link = Link::new(url)?;
    link.title = link.url.clone();
    storage::add_link(link.clone())?;
    Ok(link)
}

#[tauri::command]
pub fn add_link(
    url: String,
    title: Option<String>,
    project_id: Option<String>,
) -> Result<Link, String> {
    let mut link = Link::new(url)?;
    if let Some(t) = title {
        link.title = t;
    } else {
        link.title = link.url.clone();
    }
    link.project_id = project_id;
    storage::add_link(link.clone())?;
    Ok(link)
}

#[tauri::command]
pub fn delete_link(id: String) -> Result<(), String> {
    storage::delete_link(&id)
}
