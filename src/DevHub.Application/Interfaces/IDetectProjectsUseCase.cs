using DevHub.Domain.Models;

namespace DevHub.Application.Interfaces;

public interface IDetectProjectsUseCase
{
    List<Project> Execute(string rootPath);
}
