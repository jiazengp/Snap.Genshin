using System.Collections.Generic;
using System.Reflection;

namespace DGP.Genshin.Core.Plugins
{
    internal interface IPluginService
    {
        /// <summary>
        /// 此处的程序集可能包括了不含插件实现的 污染程序集
        /// </summary>
        IEnumerable<Assembly> PluginAssemblies { get; }
        IEnumerable<IPlugin> Plugins { get; }

        IPlugin? InstantiatePlugin(Assembly assembly);
    }
}
