using DevHub.Domain.Models;

namespace DevHub.Domain.Interfaces;

public interface IIdeScanner
{
    Task<List<IdeEntry>> ScanAsync(CancellationToken ct = default);
}
