using DevHub.Domain.Enums;
using DevHub.Domain.Models;

namespace DevHub.Domain.Models;

public record AppSettings
{
    public List<IdeEntry> Ides { get; set; } = [];
    public int DefaultIdeIndex { get; set; }
    public bool AutostartEnabled { get; set; }
    public CloseAction CloseAction { get; set; } = CloseAction.Ask;
    public bool IsDarkTheme { get; set; } = true;
}
