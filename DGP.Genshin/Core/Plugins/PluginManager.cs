using System.IO;

namespace DGP.Genshin.Core.Plugins
{
    /// <summary>
    /// 插件管理器
    /// 
    /// 秉承 插件即服务 的思想
    /// </summary>
    internal class PluginManager
    {
        private const string PluginPath = "Plugins";

        public void LoadAllPlugins()
        {
            Directory.CreateDirectory(PluginPath);
            Directory.EnumerateFiles(PluginPath, "*.dll", SearchOption.AllDirectories);
        }
    }
}
