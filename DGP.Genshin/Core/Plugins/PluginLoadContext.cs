using Snap.Core.Logging;
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
        private readonly AssemblyName appAssemblyName;

        public PluginLoadContext(string pluginPath) : base(true)
        {
            internalResolver = new AssemblyDependencyResolver(pluginPath);
            appAssemblyName = Assembly.GetExecutingAssembly().GetName();
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            //replace DGP.Genshin ref version to current release version
            if (assemblyName.Name == appAssemblyName.Name)
            {
                assemblyName.Version = appAssemblyName.Version;
            }

            if (internalResolver.ResolveAssemblyToPath(assemblyName) is string assemblyPath)
            {
                this.Log($"Load {assemblyName} from {assemblyPath}");
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}
