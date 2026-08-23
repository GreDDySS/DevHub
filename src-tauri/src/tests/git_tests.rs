use super::*;

#[test]
fn test_normalize_remote_url_https() {
    assert_eq!(
        normalize_remote_url("https://github.com/user/repo.git"),
        Some("https://github.com/user/repo".to_string())
    );
}

#[test]
fn test_normalize_remote_url_https_no_suffix() {
    assert_eq!(
        normalize_remote_url("https://github.com/user/repo"),
        Some("https://github.com/user/repo".to_string())
    );
}

#[test]
fn test_normalize_remote_url_ssh() {
    assert_eq!(
        normalize_remote_url("git@github.com:user/repo.git"),
        Some("https://github.com/user/repo".to_string())
    );
}

#[test]
fn test_normalize_remote_url_ssh_gitlab_nested_group() {
    assert_eq!(
        normalize_remote_url("git@gitlab.com:group/project.git"),
        Some("https://gitlab.com/group/project".to_string())
    );
}

#[test]
fn test_normalize_remote_url_ssh_protocol() {
    assert_eq!(
        normalize_remote_url("ssh://git@github.com/user/repo.git"),
        Some("https://github.com/user/repo".to_string())
    );
}

#[test]
fn test_normalize_remote_url_invalid() {
    assert_eq!(normalize_remote_url(""), None);
    assert_eq!(normalize_remote_url("/local/path/repo"), None);
}
