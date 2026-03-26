using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Infrastructure.Configuration;

namespace DevHub.Infrastructure.Storage;

public class JsonProjectRepository : JsonFileStore<Project>, IProjectRepository
{
    public JsonProjectRepository() : base(AppPaths.ProjectsFile)
    {
    }

    public async Task<List<Project>> GetAllAsync()
        => await LoadAllAsync();

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        var projects = await LoadAllAsync();
        return projects.FirstOrDefault(p => p.Id == id);
    }

    public async Task AddAsync(Project project)
    {
        var projects = await LoadAllAsync();
        projects.Add(project);
        await SaveAllAsync(projects);
    }

    public async Task UpdateAsync(Project project)
    {
        var projects = await LoadAllAsync();
        var index = projects.FindIndex(p => p.Id == project.Id);
        if (index >= 0)
        {
            projects[index] = project;
            await SaveAllAsync(projects);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var projects = await LoadAllAsync();
        projects.RemoveAll(p => p.Id == id);
        await SaveAllAsync(projects);
    }
}
