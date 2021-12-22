using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Extensions.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DGP.Genshin.Core.Plugins
{
    [Service(typeof(IPluginService),ServiceType.Singleton)]
    internal class PluginService : IPluginService
    {
        private const string PluginFolder = "Plugins";
        private IEnumerable<Assembly>? pluginAssemblies;

        public IEnumerable<Assembly> PluginAssemblies
        {
            get
            {
                pluginAssemblies ??= LoadAllPlugins();
                return pluginAssemblies;
            }
        }

        private IEnumerable<Assembly> LoadAllPlugins()
        {
            string pluginPath = Path.GetFullPath(PluginFolder, App.BaseDirectory);
            Directory.CreateDirectory(PluginFolder);
            IEnumerable<string>? pluginsPaths = Directory.EnumerateFiles(PluginFolder, "*.Plugin.dll", SearchOption.AllDirectories);
            List<Assembly> plugins = new();

            foreach (string? pluginLocation in pluginsPaths)
            {
                this.Log($"Loading plugin from: {pluginLocation}");
                PluginLoadContext loadContext = new(pluginLocation);
                try
                {
                    Assembly? assembly = loadContext.LoadFromAssemblyName(new(Path.GetFileNameWithoutExtension(pluginLocation)));
                    plugins.Add(assembly);
                }
                catch
                {
                    this.Log($"Failed to load plugin from: {pluginLocation}");
                }
            }
            return plugins;
        }

        public IPlugin? InstantiatePlugin(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type))
                {
                    return Activator.CreateInstance(type) as IPlugin;
                }
            }
            return null;
        }
    }
}
