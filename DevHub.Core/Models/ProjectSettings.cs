namespace DevHub.Core.Models
{
    public class ProjectSettings
    {
        public List<string> ScanPath { get; set; } = new();
        public List<string> FileExtension { get; set; } = new();
        public List<string> IgnoredDirectories { get; set; } = new();
        public int RefreshIntervalMinutes { get; set; }

        public bool ShowName { get; set; } = true;
        public bool ShowDescription { get; set; } = true;
        public bool ShowLanguage { get; set; } = true;
        public bool ShowLastModified { get; set; } = true;
        public bool ShowIDEButtons { get; set; } = true;

        public List<IDEInfo> IDEs { get; set; } = new();
     }
}
