using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Infrastructure.Configuration;

namespace DevHub.Infrastructure.Storage;

public class JsonProjectRepository(string filePath = null!) : JsonFileStore<Project>(filePath ?? AppPaths.ProjectsFile), IProjectRepository
{
    public async Task<List<Project>> GetAllAsync(CancellationToken ct = default)
        => await LoadAllAsync(ct);

    public async Task<Project?> GetByIdAsync(CancellationToken ct, Guid id)
    {
        var projects = await LoadAllAsync(ct);
        return projects.FirstOrDefault(p => p.Id == id);
    }

    public async Task AddAsync(Project project, CancellationToken ct = default)
    {
        var projects = await LoadAllAsync(ct);
        projects.Add(project);
        await SaveAllAsync(projects, ct);
    }

    public async Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        var projects = await LoadAllAsync(ct);
        var index = projects.FindIndex(p => p.Id == project.Id);
        if (index >= 0)
        {
            projects[index] = project;
            await SaveAllAsync(projects, ct);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var projects = await LoadAllAsync(ct);
        projects.RemoveAll(p => p.Id == id);
        await SaveAllAsync(projects, ct);
    }
}
