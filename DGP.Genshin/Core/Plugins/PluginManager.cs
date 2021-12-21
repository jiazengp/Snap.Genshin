using DGP.Genshin.Common.Extensions.System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DGP.Genshin.Core.Plugins
{
    /// <summary>
    /// 插件管理器,
    /// 秉承 插件即服务 的思想
    /// </summary>
    internal class PluginManager
    {
        private const string PluginPath = "Plugins";

        public void LoadAllPlugins()
        {
            Directory.CreateDirectory(PluginPath);
            IEnumerable<string>? pluginsPaths = Directory.EnumerateFiles(PluginPath, "*.Plugin.dll", SearchOption.AllDirectories);

            foreach (string? pluginLocation in pluginsPaths)
            {
                this.Log($"Loading plugin from: {pluginLocation}");
                PluginLoadContext loadContext = new(pluginLocation);
                Assembly? pluginAssembly = loadContext.LoadFromAssemblyName(new(Path.GetFileNameWithoutExtension(pluginLocation)));
            }

        }
    }
}
