using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DGP.Genshin.Core
{
    /// <summary>
    /// 服务管理器
    /// 依赖注入的核心管理类
    /// </summary>
    internal class ServiceManager
    {
        private readonly PluginService pluginService;

        /// <summary>
        /// 插件服务，
        /// 由于不能置于容器中，所以直接存放在此处
        /// </summary>
        internal PluginService PluginService => pluginService;

        /// <summary>
        /// 实例化一个新的服务管理器
        /// </summary>
        public ServiceManager()
        {
            pluginService = new PluginService();
            Services = ConfigureServices();
        }

        /// <summary>
        /// 获取 <see cref="IServiceProvider"/> 的实例
        /// 存放类
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// 配置服务
        /// </summary>
        private IServiceProvider ConfigureServices()
        {
            ServiceCollection services = new();
            services.AddSingleton<IMessenger>(App.Messenger);
            //register default services
            RegisterServices(services, typeof(App));
            //insert plugins services here
            //currently we don't allow dlls only contains services to inject
            RegisterPluginsServices(services, PluginService.Plugins);
            return services.BuildServiceProvider();
        }

        /// <summary>
        /// 向容器注册插件内的服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="plugins"></param>
        private static void RegisterPluginsServices(ServiceCollection services, IEnumerable<IPlugin> plugins)
        {
            foreach (IPlugin plugin in plugins)
            {
                RegisterServices(services, plugin.GetType());
            }
        }

        /// <summary>
        /// 向容器注册服务
        /// </summary>
        /// <param name="services">容器</param>
        /// <param name="entryType">入口类型，该类型所在的程序集均会被扫描</param>
        /// <exception cref="SnapGenshinInternalException">注册的类型为非已知类型</exception>
        private static void RegisterServices(ServiceCollection services, Type entryType)
        {
            foreach (Type type in entryType.Assembly.GetTypes())
            {
                RegisterService(services, type);
            }
        }
        /// <summary>
        /// 向容器注册服务
        /// </summary>
        /// <param name="services">容器</param>
        /// <param name="type">待检测的类型</param>
        /// <exception cref="SnapGenshinInternalException">未知的注册类型</exception>
        private static void RegisterService(ServiceCollection services, Type type)
        {
            //注册服务类型
            if (type.GetCustomAttribute<ServiceAttribute>() is ServiceAttribute serviceAttr)
            {
                _ = serviceAttr.ServiceType switch
                {
                    ServiceType.Singleton => services.AddSingleton(serviceAttr.InterfaceType, type),
                    ServiceType.Transient => services.AddTransient(serviceAttr.InterfaceType, type),
                    _ => throw new SnapGenshinInternalException($"未知的服务类型 {type}"),
                };
            }
            //注册视图模型
            if (type.GetCustomAttribute<ViewModelAttribute>() is ViewModelAttribute viewModelAttr)
            {
                _ = viewModelAttr.ViewModelType switch
                {
                    ViewModelType.Singleton => services.AddSingleton(type),
                    ViewModelType.Transient => services.AddTransient(type),
                    _ => throw new SnapGenshinInternalException($"未知的视图模型类型 {type}"),
                };
            }
        }
    }
}
