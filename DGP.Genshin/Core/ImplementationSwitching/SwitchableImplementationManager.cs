using DGP.Genshin.Core.Background.Abstraction;
using DGP.Genshin.Service.Abstraction.Launching;
using Snap.Data.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Core.ImplementationSwitching
{
    /// <summary>
    /// 可切换服务管理器
    /// 添加可切换实现后需要同时更改
    /// <see cref="SnapGenshinServiceManager.RegisterSwitchableImplementation(Type, SwitchableImplementationAttribute)"/>
    /// </summary>
    internal class SwitchableImplementationManager
    {
        private const string ImplementationsFile = "implementations.json";

        /// <summary>
        /// 构造一个新的可切换服务管理器
        /// </summary>
        public SwitchableImplementationManager()
        {
            this.TypeData = Json.FromFileOrNew<ImplmentationTypeData>(PathContext.Locate(ImplementationsFile));
        }

        /// <summary>
        /// 背景图片提供器集合
        /// </summary>
        [SwitchableInterfaceType(typeof(IBackgroundProvider))]
        public List<SwitchableEntry<IBackgroundProvider>> BackgroundProviders { get; internal set; } = new();

        /// <summary>
        /// 当前的背景图片提供器
        /// </summary>
        public SwitchableEntry<IBackgroundProvider>? CurrentBackgroundProvider { get; set; }

        /// <summary>
        /// 启动服务合集
        /// </summary>
        [SwitchableInterfaceType(typeof(ILaunchService))]
        public List<SwitchableEntry<ILaunchService>> LaunchServices { get; internal set; } = new();

        /// <summary>
        /// 当前的启动服务
        /// </summary>
        public SwitchableEntry<ILaunchService>? CurrentLaunchService { get; set; }

        private ImplmentationTypeData TypeData { get; }

        /// <summary>
        /// 在程序集探测完成后将管理器内的实现切换到正确的实现
        /// </summary>
        public void SwitchToCorrectImplementations()
        {
            this.CurrentBackgroundProvider = this.BackgroundProviders.First(i => i.Name == this.TypeData.BackgroundProviderName);
            this.CurrentLaunchService = this.LaunchServices.First(i => i.Name == this.TypeData.LaunchServiceName);
        }

        /// <summary>
        /// 退出程序时调用以保存选项
        /// </summary>
        public void UnInitialize()
        {
            this.TypeData.BackgroundProviderName = this.CurrentBackgroundProvider!.Name;
            this.TypeData.LaunchServiceName = this.CurrentLaunchService!.Name;

            Json.ToFile(PathContext.Locate(ImplementationsFile), this.TypeData);
        }

        /// <summary>
        /// 可切换服务入口点
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        internal class SwitchableEntry<T>
        {
            public SwitchableEntry(SwitchableImplementationAttribute attribute, Lazy<T> factory)
            {
                this.Name = attribute.Name;
                this.Description = attribute.Description;
                this.Factory = factory;
            }

            public string Name { get; }
            public string Description { get; }
            public Lazy<T> Factory { get; }
        }

        private class ImplmentationTypeData
        {
            public string BackgroundProviderName { get; set; } = SwitchableImplementationAttribute.DefaultName;

            public string LaunchServiceName { get; set; } = SwitchableImplementationAttribute.DefaultName;
        }
    }
}