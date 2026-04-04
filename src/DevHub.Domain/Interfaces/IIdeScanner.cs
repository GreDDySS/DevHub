using DevHub.Domain.Models;

namespace DevHub.Domain.Interfaces;

public interface IIdeScanner
{
    List<IdeEntry> Scan();
}
