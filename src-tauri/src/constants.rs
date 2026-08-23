/// Directories to skip during recursive directory traversal.
pub const EXCLUDED_DIRS: &[&str] = &[
    "node_modules", ".git", "bin", "obj", ".vs", ".vscode",
    "target", "dist", "build", ".next", "__pycache__",
];

/// Directories to skip during language detection (includes runtime dirs).
pub const EXCLUDED_LANG_DETECT: &[&str] = &[
    "node_modules", ".git", "target", "dist", "build", "__pycache__", "venv", ".venv",
];

/// Directories excluded from project stats (artifacts counted into disk size only).
pub const STATS_EXCLUDED_DIRS: &[&str] = &[
    ".git", "node_modules", "target", "dist", "build", "out",
    ".next", "__pycache__", ".venv", "venv",
];

/// Files whose presence marks a directory as a project root during scanning.
pub const PROJECT_MARKERS: &[&str] = &[
    "Cargo.toml", "package.json", "go.mod", "pom.xml", "build.gradle",
    "CMakeLists.txt", "requirements.txt", "setup.py", "pyproject.toml",
    "Gemfile", "composer.json",
];

/// Field/record separators for parsing `git log --pretty=format:` output.
pub const GIT_LOG_FIELD_SEPARATOR: char = '\u{1f}';
pub const GIT_LOG_RECORD_SEPARATOR: char = '\u{1e}';

/// Known IDE display names and detection tables.
pub mod ide {
    pub const VS_CODE: &str = "Visual Studio Code";
    pub const VISUAL_STUDIO: &str = "Visual Studio";
    pub const RIDER: &str = "JetBrains Rider";
    pub const INTELLIJ_IDEA: &str = "JetBrains IntelliJ IDEA";
    pub const WEBSTORM: &str = "JetBrains WebStorm";
    pub const PYCHARM: &str = "JetBrains PyCharm";
    pub const PYCHARM_COMMUNITY: &str = "JetBrains PyCharm Community";
    pub const CLION: &str = "JetBrains CLion";
    pub const GOLAND: &str = "JetBrains GoLand";
    pub const RUSTROVER: &str = "JetBrains RustRover";
    pub const NOTEPAD_PLUS_PLUS: &str = "Notepad++";
    pub const SUBLIME_TEXT: &str = "Sublime Text";

    /// Windows registry subkeys listing installed applications.
    pub const REGISTRY_UNINSTALL_SUBKEYS: &[&str] = &[
        r"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        r"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
    ];

    /// (display name substring, friendly name), checked in order.
    pub const WINDOWS_IDE_PATTERNS: &[(&str, &str)] = &[
        ("Visual Studio Code", VS_CODE),
        ("Visual Studio", VISUAL_STUDIO),
        ("Rider", RIDER),
        ("IntelliJ", INTELLIJ_IDEA),
        ("WebStorm", WEBSTORM),
        ("PyCharm", PYCHARM),
        ("CLion", CLION),
        ("GoLand", GOLAND),
        ("RustRover", RUSTROVER),
        ("Notepad++", NOTEPAD_PLUS_PLUS),
        ("Sublime Text", SUBLIME_TEXT),
    ];

    /// Same as above but with PyCharm Community before PyCharm.
    pub const DISPLAY_NAME_PATTERNS: &[(&str, &str)] = &[
        ("visual studio code", VS_CODE),
        ("visual studio", VISUAL_STUDIO),
        ("rider", RIDER),
        ("intellij", INTELLIJ_IDEA),
        ("webstorm", WEBSTORM),
        ("pycharm community", PYCHARM_COMMUNITY),
        ("pycharm", PYCHARM),
        ("clion", CLION),
        ("goland", GOLAND),
        ("rustrover", RUSTROVER),
        ("notepad++", NOTEPAD_PLUS_PLUS),
        ("sublime text", SUBLIME_TEXT),
    ];

    /// (name/exec substring, friendly name) for Linux .desktop entries.
    pub const LINUX_IDE_PATTERNS: &[(&str, &str)] = &[
        ("code", VS_CODE),
        ("rider", RIDER),
        ("idea", INTELLIJ_IDEA),
        ("webstorm", WEBSTORM),
        ("pycharm", PYCHARM),
        ("clion", CLION),
        ("goland", GOLAND),
        ("rustrover", RUSTROVER),
        ("sublime_text", SUBLIME_TEXT),
        ("subl", SUBLIME_TEXT),
    ];

    /// System-wide directories containing .desktop launchers.
    #[allow(dead_code)]
    pub const LINUX_DESKTOP_DIRS: &[&str] = &[
        "/usr/share/applications",
        "/usr/local/share/applications",
        "/var/lib/flatpak/exports/share/applications",
        "/var/lib/snapd/desktop/applications",
    ];

    /// Per-user desktop launcher directory relative to $HOME.
    #[allow(dead_code)]
    pub const LINUX_USER_DESKTOP_DIR: &str = ".local/share/applications";

    /// (.app bundle name, friendly name, executable path inside the bundle).
    #[allow(dead_code)]
    pub const MACOS_IDE_APPS: &[(&str, &str, &str)] = &[
        ("Visual Studio Code.app", VS_CODE, "Contents/Resources/app/bin/code"),
        ("Rider.app", RIDER, "Contents/MacOS/rider"),
        ("IntelliJ IDEA.app", INTELLIJ_IDEA, "Contents/MacOS/idea"),
        ("WebStorm.app", WEBSTORM, "Contents/MacOS/webstorm"),
        ("PyCharm.app", PYCHARM, "Contents/MacOS/pycharm"),
        ("PyCharm CE.app", PYCHARM_COMMUNITY, "Contents/MacOS/pycharm"),
        ("CLion.app", CLION, "Contents/MacOS/clion"),
        ("GoLand.app", GOLAND, "Contents/MacOS/goland"),
        ("RustRover.app", RUSTROVER, "Contents/MacOS/rustrover"),
        ("Sublime Text.app", SUBLIME_TEXT, "Contents/SharedSupport/bin/subl"),
    ];

    #[allow(dead_code)]
    pub const MACOS_SYSTEM_APPS_DIR: &str = "/Applications";
    #[allow(dead_code)]
    pub const MACOS_USER_APPS_DIR: &str = "Applications";
}
