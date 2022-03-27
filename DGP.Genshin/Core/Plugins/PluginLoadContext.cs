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
            this.internalResolver = new AssemblyDependencyResolver(pluginPath);
            this.appAssemblyName = Assembly.GetExecutingAssembly().GetName();
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            //replace DGP.Genshin ref version to current release version
            //seems useless, but just keep this there
            if (assemblyName.Name == this.appAssemblyName.Name)
            {
                assemblyName.Version = this.appAssemblyName.Version;
            }

            if (this.internalResolver.ResolveAssemblyToPath(assemblyName) is string assemblyPath)
            {
                this.Log($"Load {assemblyName} from {assemblyPath}");
                return this.LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}
