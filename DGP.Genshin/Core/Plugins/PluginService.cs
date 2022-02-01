using Snap.Core.Logging;
using Snap.Extenion.Enumerable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DGP.Genshin.Core.Plugins
{
    /// <summary>
    /// 此服务会保证插件文件夹的存在
    /// </summary>
    internal class PluginService : IPluginService
    {
        private const string PluginFolder = "Plugins";
        private readonly IEnumerable<Assembly> pluginAssemblies;
        private readonly IEnumerable<IPlugin> plugins;

        public IEnumerable<Assembly> PluginAssemblies => pluginAssemblies;

        public IEnumerable<IPlugin> Plugins => plugins;

        public PluginService()
        {
            pluginAssemblies = LoadAllPluginDlls();
            plugins = PluginAssemblies.Select(p => InstantiatePlugin(p)).NotNull();
        }

        private IEnumerable<Assembly> LoadAllPluginDlls()
        {
            //fix autorun fail issue
            string pluginPath = Path.GetFullPath(PluginFolder, AppContext.BaseDirectory);
            Directory.CreateDirectory(pluginPath);
            IEnumerable<string>? pluginsPaths = Directory.EnumerateFiles(pluginPath, "*.dll", SearchOption.AllDirectories);
            List<Assembly> plugins = new();

            foreach (string? pluginLocation in pluginsPaths)
            {
                this.Log($"Loading plugin from: {pluginLocation}");
                PluginLoadContext loadContext = new(pluginLocation);
                try
                {
                    Assembly? assembly = loadContext.LoadFromAssemblyName(new(Path.GetFileNameWithoutExtension(pluginLocation)));
                    if (assembly.GetCustomAttribute<SnapGenshinPluginAttribute>() is not null)
                    {
                        plugins.Add(assembly);
                        this.Log($"plugin : {assembly.FullName} added to plugin collection");
                    }
                }
                catch(Exception e)
                {
                    this.Log(e);
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
