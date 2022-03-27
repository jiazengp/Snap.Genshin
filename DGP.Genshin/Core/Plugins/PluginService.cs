using DGP.Genshin.Helper;
using Snap.Core.Logging;
using Snap.Extenion.Enumerable;
using Snap.Reflection;
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

        public IEnumerable<Assembly> PluginAssemblies
        {
            get => this.pluginAssemblies;
        }

        public IEnumerable<IPlugin> Plugins
        {
            get => this.plugins;
        }

        public PluginService()
        {
            this.pluginAssemblies = this.LoadAllPluginDlls();
            this.plugins = this.PluginAssemblies.Select(p => this.InstantiatePlugin(p)).NotNull();
        }

        private IEnumerable<Assembly> LoadAllPluginDlls()
        {
            //fix autorun fail issue
            string pluginPath = PathContext.Locate(PluginFolder);
            Directory.CreateDirectory(pluginPath);
            IEnumerable<string> pluginsPaths = Directory.EnumerateFiles(pluginPath, "*.dll", SearchOption.AllDirectories);
            List<Assembly> plugins = new();

            foreach (string pluginLocation in pluginsPaths)
            {
                this.Log($"Loading plugin from: {pluginLocation}");
                PluginLoadContext loadContext = new(pluginLocation);
                try
                {
                    AssemblyName pluginAssemblyName = new(Path.GetFileNameWithoutExtension(pluginLocation));
                    Assembly assembly = loadContext.LoadFromAssemblyName(pluginAssemblyName);
                    if (assembly.HasAttribute<SnapGenshinPluginAttribute>())
                    {
                        plugins.Add(assembly);
                        this.Log($"plugin : {assembly.FullName} added to plugin collection");
                    }
                }
                catch (Exception e)
                {
                    if (loadContext.IsCollectible)
                    {
                        loadContext.Unload();
                    }
                    this.Log(e);
                    this.Log($"Failed to load plugin from: {pluginLocation}");
                }
            }
            return plugins;
        }

        public IPlugin? InstantiatePlugin(Assembly assembly)
        {
            Type? type = assembly.GetTypes().FirstOrDefault(type => type.Implement<IPlugin>());
            return type is null ? null : Activator.CreateInstance(type) as IPlugin;
        }
    }
}
