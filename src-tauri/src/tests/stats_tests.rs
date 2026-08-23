use super::*;
use crate::models::ProjectStats;

#[test]
fn test_walk_stats_separates_source_and_artifacts() {
    let root = std::env::temp_dir().join(format!("devhub_stats_{}", uuid::Uuid::new_v4()));
    std::fs::create_dir_all(root.join("src")).unwrap();
    std::fs::create_dir_all(root.join("node_modules").join("pkg")).unwrap();
    std::fs::create_dir_all(root.join(".git")).unwrap();

    std::fs::write(root.join("README.md"), "hello").unwrap();
    std::fs::write(root.join("src").join("main.rs"), "fn main() {}").unwrap();
    std::fs::write(
        root.join("node_modules").join("pkg").join("x.js"),
        "// heavy",
    )
    .unwrap();
    std::fs::write(root.join(".git").join("index"), "binary".as_bytes()).unwrap();

    let mut stats = ProjectStats::default();
    walk_stats(&root, &mut stats, 0, false);

    let source_len = "hello".len() as u64 + "fn main() {}".len() as u64;
    let artifact_len = "// heavy".len() as u64 + "binary".len() as u64;

    assert_eq!(stats.file_count, 2);
    assert_eq!(stats.dir_count, 1);
    assert_eq!(stats.source_size, source_len);

    assert_eq!(stats.total_size, source_len + artifact_len);
    assert!(stats.last_modified > 0);

    std::fs::remove_dir_all(&root).ok();
}

#[test]
fn test_walk_stats_missing_dir_is_noop() {
    let mut stats = ProjectStats::default();
    walk_stats(
        std::path::Path::new("Z:/definitely/not/here"),
        &mut stats,
        0,
        false,
    );
    assert_eq!(stats.file_count, 0);
    assert_eq!(stats.dir_count, 0);
    assert_eq!(stats.total_size, 0);
}
