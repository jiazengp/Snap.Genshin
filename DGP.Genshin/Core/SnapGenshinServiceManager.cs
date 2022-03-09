using DGP.Genshin.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.Threading;
using Snap.Extenion.Enumerable;
using System.Collections.Generic;

namespace DGP.Genshin.Core
{
    /// <summary>
    /// 实现了注入插件的服务管理类
    /// </summary>
    internal class SnapGenshinServiceManager : ServiceManagerBase
    {
        /// <summary>
        /// 重载探测程序集方法
        /// 注入插件
        /// </summary>
        /// <param name="services"></param>
        protected override void OnProbingServices(ServiceCollection services)
        {
            //default messager
            services.AddSingleton<IMessenger>(App.Messenger);
            //JoinableTaskContext
            JoinableTaskContext context = new();
            services.AddSingleton(new JoinableTaskContext());
            services.AddSingleton(new JoinableTaskFactory(context));

            base.OnProbingServices(services);
            //insert plugins services here
            RegisterPluginsServices(services, App.Current.PluginService.Plugins);
        }

        /// <summary>
        /// 向容器注册插件内的服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="plugins"></param>
        private void RegisterPluginsServices(ServiceCollection services, IEnumerable<IPlugin> plugins)
        {
            plugins.ForEach(plugin => RegisterServices(services, plugin.GetType()));
        }
    }
}
