namespace DevHub.Core.Models
{
    public class ProjectInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = "Нет описание...";
        public string Language { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
    }
}
