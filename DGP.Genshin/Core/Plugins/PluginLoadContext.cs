using System.Reflection;
using System.Runtime.Loader;

namespace DGP.Genshin.Core.Plugins
{
    /// <summary>
    /// 重写默认的程序集加载行为
    /// </summary>
    internal class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver internalResolver;

        public PluginLoadContext(string pluginPath)
        {
            internalResolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? assemblyPath = internalResolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }
    }
}
