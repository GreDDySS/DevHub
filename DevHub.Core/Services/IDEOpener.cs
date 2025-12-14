using DevHub.Core.Interfaces;
using DevHub.Core.Models;
using System.Diagnostics;

namespace DevHub.Core.Services
{
    public class IDEOpener : IIDEOpener
    {
        public void OpenProject(string projectPath, IDEInfo ide)
        {
            string targetPath = projectPath;

            if (ide.Name.Contains("Visual Studio", StringComparison.OrdinalIgnoreCase) || ide.ExecutablePath.Contains("dotenv.exe", StringComparison.OrdinalIgnoreCase))
            {
                var slnFiles = Directory.GetFiles(projectPath, "*.sln", SearchOption.TopDirectoryOnly);
                if (slnFiles.Length > 0)
                {
                    targetPath = slnFiles[0];
                }
            }

            var args = string.Format(ide.Arguments, targetPath);

            Process.Start(new ProcessStartInfo
            {
                FileName = ide.ExecutablePath,
                Arguments = args,
                UseShellExecute = true
            });
        }
    }
}
