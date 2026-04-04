using DevHub.Domain.Models;

namespace DevHub.Application.Interfaces;

public interface ICaptureLinkUseCase
{
    Task<Link?> ExecuteAsync(Guid? projectId, CancellationToken ct = default);
}
