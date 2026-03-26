using DevHub.Domain.Enums;

namespace DevHub.Application.DTOs;

public record ProjectFilter(
    string? SearchQuery = null,
    ProjectStatus? Status = null,
    List<string>? Tags = null);
