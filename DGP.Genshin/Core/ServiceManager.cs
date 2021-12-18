using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace DGP.Genshin.Core
{
    /// <summary>
    /// 服务管理器
    /// </summary>
    public class ServiceManager
    {
        /// <summary>
        /// 实例化一个新的服务管理器
        /// </summary>
        public ServiceManager()
        {
            Services = ConfigureServices();
        }
        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            ServiceCollection services = new();
            RegisterServices(services, typeof(App));
            return services.BuildServiceProvider();
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
                //注册服务类型
                if (type.GetCustomAttribute<ServiceAttribute>() is ServiceAttribute serviceAttr)
                {
                    _ = serviceAttr.ServiceType switch
                    {
                        ServiceType.Singleton => services.AddSingleton(serviceAttr.InterfaceType, type),
                        ServiceType.Transient => services.AddTransient(serviceAttr.InterfaceType, type),
                        _ => throw new SnapGenshinInternalException($"未知的服务类型{type}"),
                    };
                }
                //注册视图模型类
                if (type.GetCustomAttribute<ViewModelAttribute>() is ViewModelAttribute viewModelAttr)
                {
                    _ = viewModelAttr.ViewModelType switch
                    {
                        ViewModelType.Singleton => services.AddSingleton(type),
                        ViewModelType.Transient => services.AddTransient(type),
                        _ => throw new SnapGenshinInternalException($"未知的视图模型类型{type}"),
                    };
                }
            }
        }
    }
}
