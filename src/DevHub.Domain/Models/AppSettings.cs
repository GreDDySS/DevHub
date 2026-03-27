using DevHub.Domain.Enums;

namespace DevHub.Domain.Models;

public class AppSettings
{
    public List<IdeEntry> Ides { get; set; } = [];
    public int DefaultIdeIndex { get; set; }
    public bool AutostartEnabled { get; set; }
    public CloseAction CloseAction { get; set; } = CloseAction.Ask;
}
