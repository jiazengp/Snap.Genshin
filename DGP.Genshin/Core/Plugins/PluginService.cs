using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Common.Extensions.System.Collections.Generic;
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
    internal class PluginService
    {
        private const string PluginFolder = "Plugins";
        private readonly IEnumerable<Assembly> pluginAssemblies;
        private readonly IEnumerable<IPlugin> plugins;

        /// <summary>
        /// 此处的程序集可能包括了不含插件实现的 污染程序集
        /// </summary>
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
            IEnumerable<string>? pluginsPaths = Directory.EnumerateFiles(pluginPath, "*.Plugin.dll", SearchOption.AllDirectories);
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
