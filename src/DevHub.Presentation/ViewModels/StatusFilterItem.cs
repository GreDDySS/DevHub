using DevHub.Domain.Enums;

namespace DevHub.Presentation.ViewModels;

public record StatusFilterItem(string Name, ProjectStatus? Value)
{
    public override string ToString() => Name;
}
