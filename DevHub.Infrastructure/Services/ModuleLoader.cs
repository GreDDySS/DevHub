using DevHub.Core.Attributes;
using DevHub.Core.Interfaces;
using DevHub.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace DevHub.Infrastructure.Services
{
    public class ModuleLoader
    {
        private readonly string _modulesPath;
        private readonly string _configPath;
        private ModulesConfiguration _configuration;

        public ModuleLoader(string baseDir)
        {
            _modulesPath = Path.Combine(baseDir, "Modules");
            _configPath = Path.Combine(baseDir, "modules.config.json");

            EnsureConfig();
        }

        private void EnsureConfig()
        {
            if (!File.Exists(_configPath))
            {
                _configuration = new ModulesConfiguration();
                SaveConfig();
            }
            else
            {
                var json = File.ReadAllText(_configPath);
                _configuration = JsonSerializer.Deserialize<ModulesConfiguration>(json) ?? new ModulesConfiguration();
            }
        }

        private void SaveConfig()
        {
            var json = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }

        public void LoadModules(IServiceCollection services)
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in loadedAssemblies)
            {
                TryLoadModuleFromAssembly(assembly, services);
            }

            if (Directory.Exists(_modulesPath))
            {
                var dllFiles = Directory.GetFiles(_modulesPath, "DevHub.Modules.*.dll");
                foreach (var dllPath in dllFiles)
                {
                    try 
                    {
                        var assembly = Assembly.LoadFrom(dllPath);
                        TryLoadModuleFromAssembly(assembly, services);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading module from {dllPath}: {ex.Message}");
                    }
                }
            }
        }

        private void TryLoadModuleFromAssembly(Assembly assembly, IServiceCollection services)
        {
            var moduleType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (moduleType == null) return;

            if (services.Any(s => s.ServiceType == typeof(IModule) && s.ImplementationInstance?.GetType() == moduleType))
            {
                return;
            }

            var attr = moduleType.GetCustomAttribute<ModuleAttribute>();
            var moduleName = attr?.Name ?? moduleType.Name;
            var isSystem = attr?.IsSystem ?? false;

            if (!_configuration.ModulesState.ContainsKey(moduleName))
            {
                _configuration.ModulesState[moduleName] = true;
                SaveConfig();
            }

            if (!isSystem && !_configuration.ModulesState[moduleName])
            {
                return;
            }

            var module = (IModule)Activator.CreateInstance(moduleType);
            module.RegisterTypes(services);

            services.AddSingleton(typeof(IModule), module);
        }
    }
}
