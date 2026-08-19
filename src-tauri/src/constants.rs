/// Directories to skip during recursive directory traversal.
pub const EXCLUDED_DIRS: &[&str] = &[
    "node_modules", ".git", "bin", "obj", ".vs", ".vscode",
    "target", "dist", "build", ".next", "__pycache__",
];

/// Directories to skip during language detection (includes runtime dirs).
pub const EXCLUDED_LANG_DETECT: &[&str] = &[
    "node_modules", ".git", "target", "dist", "build", "__pycache__", "venv", ".venv",
];
