using DevHub.Core.Interfaces;
using DevHub.Core.Models;
using DevHub.Core.ViewModels;
using System.Diagnostics;
using System.Windows.Input;

namespace DevHub.Modules.Projects.ViewModels
{
    public class ProjectCardViewModel : BaseViewModel
    {
        public string Name => Project.Name;
        public string Language => Project.Language;
        public string ProjPath => Project.Path;
        public string Description => Project.Description;
        public DateTime LastModified => Project.LastModified;
        
        public ProjectInfo Project { get; }
        public IEnumerable<IDEInfo> IDEs { get; }

        public ICommand OpenFolderCommand { get; }
        public ICommand OpenInIDECommand { get; }

        public ProjectSettings Settings { get; }
        public bool ShowName => Settings.ShowName;
        public bool ShowDescription => Settings.ShowDescription;
        public bool ShowLanguage => Settings.ShowLanguage;
        public bool ShowLastModified => Settings.ShowLastModified;
        public bool ShowIDEButtons => Settings.ShowIDEButtons;

        private readonly IIDEOpener _ideOpener;

        public ProjectCardViewModel(ProjectInfo project, IEnumerable<IDEInfo> ides, IIDEOpener ideOpener, ProjectSettings settings)
        {
            Project = project;
            IDEs = ides;
            _ideOpener = ideOpener;
            Settings = settings;

            OpenFolderCommand = new RelayCommand((_) =>
            {
                Process.Start("explorer.exe", ProjPath);
            });

            OpenInIDECommand = new RelayCommand((parameter) =>
            {
                if (parameter is IDEInfo ide)
                    _ideOpener.OpenProject(project.Path, ide);
            });

        }
    }
}
