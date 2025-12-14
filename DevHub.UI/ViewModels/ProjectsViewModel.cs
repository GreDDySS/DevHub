using DevHub.Core.Interfaces;
using DevHub.Core.Models;
using DevHub.UI.Base;
using System.Collections.ObjectModel;
using DevHub.UI.ViewModels;
using DevHub.Core.Services;

namespace DevHub.UI.ViewModels
{
    public class ProjectsViewModel : BaseViewModel
    {
        public ObservableCollection<ProjectCardViewModel> Projects { get; }
        public ProjectSettings Settings { get; private set; }

        public ProjectsViewModel(IProjectsService service, ProjectSettings settings)
        {
            Settings = settings;
            service.Refresh();

            var ideOpener = new IDEOpener();

            Projects = new ObservableCollection<ProjectCardViewModel>(
                service.Projects.Select(p => new ProjectCardViewModel(p, service.IDEs, ideOpener, settings)));
        }

        public void UpdateSettings(IProjectsService service, ProjectSettings newSettings)
        {
            Settings = newSettings;
            service.Refresh();
            Projects.Clear();
            var ideOpener = new IDEOpener();
            foreach (var p in service.Projects)
            {
                Projects.Add(new ProjectCardViewModel(p, service.IDEs, ideOpener, newSettings));
            }
        }
    }
}