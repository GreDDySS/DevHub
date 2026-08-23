// Known issue: On Windows, WebView2 logs a harmless error on shutdown:
// [ERROR:ui\gfx\win\window_impl.cc:172] Failed to unregister class Chrome_WidgetWin_0. Error = 1412
// This is a Chromium internal issue — the class is still in use when the app exits.
// No fix available; affects most Tauri/WebView2 apps on Windows. Safe to ignore.

mod models;
mod storage;
mod commands;
mod scanner;
mod constants;
mod ide_detection;

use tauri::Manager;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_global_shortcut::Builder::new().build())
        .plugin(tauri_plugin_clipboard_manager::init())
        .plugin(tauri_plugin_autostart::init(
            tauri_plugin_autostart::MacosLauncher::LaunchAgent,
            Some(vec!["--minimized"]),
        ))
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_log::Builder::new().build())
        .plugin(tauri_plugin_notification::init())
        .plugin(tauri_plugin_dialog::init())
        .setup(|app| {
            let _ = storage::init_storage();

            #[cfg(desktop)]
            {
                use tauri::tray::{TrayIconBuilder, MouseButton, MouseButtonState, TrayIconEvent};

                let _tray = TrayIconBuilder::new()
                    .tooltip("DevHub")
                    .icon(app.default_window_icon().unwrap().clone())
                    .on_tray_icon_event(|tray_icon, event| {
                        if let TrayIconEvent::Click {
                            button: MouseButton::Left,
                            button_state: MouseButtonState::Up,
                            ..
                        } = event
                        {
                            let app = tray_icon.app_handle();
                            if let Some(window) = app.get_webview_window("main") {
                                let _ = window.show();
                                let _ = window.set_focus();
                            }
                        }
                    })
                    .on_menu_event(|app, event| {
                        match event.id.as_ref() {
                            "show" => {
                                if let Some(window) = app.get_webview_window("main") {
                                    let _ = window.show();
                                    let _ = window.set_focus();
                                }
                            }
                            "quit" => {
                                app.exit(0);
                            }
                            _ => {}
                        }
                    })
                    .build(app)?;

                // Setup menu
                {
                    use tauri::menu::{MenuBuilder, MenuItemBuilder};

                    let show = MenuItemBuilder::with_id("show", "Show DevHub")
                        .build(app)?;
                    let quit = MenuItemBuilder::with_id("quit", "Exit")
                        .build(app)?;

                    let menu = MenuBuilder::new(app)
                        .item(&show)
                        .separator()
                        .item(&quit)
                        .build()?;

                    _tray.set_menu(Some(menu))?;
                }
            }

            // Setup global shortcut (Ctrl+Shift+Y) — save link from clipboard
            #[cfg(desktop)]
            {
                use tauri_plugin_global_shortcut::{
                    Code, GlobalShortcutExt, Modifiers, Shortcut,
                };
                use tauri_plugin_clipboard_manager::ClipboardExt;

                let shortcut = Shortcut::new(Some(Modifiers::CONTROL | Modifiers::SHIFT), Code::KeyY);
                let app_handle = app.handle().clone();

                app.global_shortcut()
                    .on_shortcut(shortcut, move |_app, _shortcut, event| {
                        if event.state == tauri_plugin_global_shortcut::ShortcutState::Pressed {
                            if let Ok(text) = app_handle.clipboard().read_text() {
                                let url = text.to_string();
                                if url.starts_with("http://") || url.starts_with("https://") {
                                    if let Ok(link) = crate::models::Link::new(url) {
                                        let _ = storage::add_link(link);
                                    }
                                }
                            }
                        }
                    })?;
            }

            // Handle close request
            #[cfg(desktop)]
            {
                use tauri::Emitter;

                let main_window = app.get_webview_window("main").unwrap();
                let main_window_clone = main_window.clone();

                main_window.on_window_event(move |event| {
                    if let tauri::WindowEvent::CloseRequested { api, .. } = event {
                        let settings = storage::get_settings();
                        match &settings.close_action {
                            models::CloseAction::Exit => {}
                            models::CloseAction::MinimizeToTray => {
                                api.prevent_close();
                                let _ = main_window_clone.hide();
                            }
                            models::CloseAction::Ask => {
                                api.prevent_close();
                                let _ = main_window_clone.show();
                                let _ = main_window_clone.set_focus();
                                let _ = main_window_clone.emit("show-close-dialog", ());
                            }
                        }
                    }
                });
            }

            // Show window after setup is complete
            if let Some(window) = app.get_webview_window("main") {
                let _ = window.show();
            }

            Ok(())
        })
        .invoke_handler(tauri::generate_handler![
            commands::get_projects,
            commands::add_project,
            commands::update_project,
            commands::delete_project,
            commands::toggle_favorite,
            commands::toggle_hidden,
            commands::get_links,
            commands::capture_link,
            commands::add_link,
            commands::delete_link,
            commands::get_todos,
            commands::add_todo,
            commands::update_todo,
            commands::toggle_todo,
            commands::delete_todo,
            commands::clear_completed_todos,
            commands::get_settings,
            commands::save_settings,
            commands::scan_ides,
            commands::open_in_explorer,
            commands::open_in_ide,
            commands::open_in_console,
            commands::open_in_browser,
            commands::get_git_activity,
            commands::get_project_stats,
            commands::detect_projects,
            commands::refresh_projects,
            commands::force_exit,
            commands::get_data_dir,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
