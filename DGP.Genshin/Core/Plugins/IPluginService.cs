using System.Collections.Generic;
using System.Reflection;

namespace DGP.Genshin.Core.Plugins
{
    /// <summary>
    /// 插件管理器,
    /// 秉承 插件即服务 的思想
    /// </summary>
    internal interface IPluginService
    {
        /// <summary>
        /// 搜索到的所有插件程序集
        /// </summary>
        IEnumerable<Assembly> PluginAssemblies { get; }

        /// <summary>
        /// 从程序集实例化插件,
        /// 只会实例化找到的首个实现了 <see cref="IPlugin"/> 的类
        /// </summary>
        /// <param name="assembly">要查找的程序集</param>
        /// <returns></returns>
        IPlugin? InstantiatePlugin(Assembly assembly);
    }
}
