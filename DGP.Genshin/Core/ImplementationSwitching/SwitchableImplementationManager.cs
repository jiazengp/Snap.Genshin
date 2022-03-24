using DGP.Genshin.Core.Background.Abstraction;
using DGP.Genshin.Helper;
using DGP.Genshin.Service.Abstraction.Launching;
using Snap.Data.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Core.ImplementationSwitching
{
    /// <summary>
    /// 添加可切换实现后需要同时更改
    /// <see cref="SnapGenshinServiceManager.RegisterSwitchableImplementation(Type, SwitchableImplementationAttribute)"/>
    /// </summary>
    internal class SwitchableImplementationManager
    {
        private const string ImplementationsFile = "implementations.json";
        private class ImplmentationTypeData
        {
            public string BackgroundProviderName { get; set; } = SwitchableImplementationAttribute.DefaultName;
            public string LaunchServiceName { get; set; } = SwitchableImplementationAttribute.DefaultName;
        }
        private ImplmentationTypeData TypeData { get; }
        internal class SwitchableEntry<T>
        {
            public SwitchableEntry(SwitchableImplementationAttribute attribute, Lazy<T> factory)
            {
                Name = attribute.Name;
                Description = attribute.Description;
                Factory = factory;
            }

            public string Name { get; }
            public string Description { get; }
            public Lazy<T> Factory { get; }
        }

        #region Switchable
        [SwitchableInterfaceType(typeof(IBackgroundProvider))]
        public List<SwitchableEntry<IBackgroundProvider>> BackgroundProviders { get; internal set; } = new();
        public SwitchableEntry<IBackgroundProvider>? CurrentBackgroundProvider { get; set; }

        [SwitchableInterfaceType(typeof(ILaunchService))]
        public List<SwitchableEntry<ILaunchService>> LaunchServices { get; internal set; } = new();
        public SwitchableEntry<ILaunchService>? CurrentLaunchService { get; set; }
        #endregion

        public SwitchableImplementationManager()
        {
            TypeData = Json.FromFileOrNew<ImplmentationTypeData>(PathContext.Locate(ImplementationsFile));
        }

        /// <summary>
        /// 在程序集探测完成后将管理器内的实现切换到正确的实现
        /// </summary>
        public void SwitchToCorrectImplementations()
        {
            CurrentBackgroundProvider = BackgroundProviders.First(i => i.Name == TypeData.BackgroundProviderName);
            CurrentLaunchService = LaunchServices.First(i => i.Name == TypeData.LaunchServiceName);
        }

        public void UnInitialize()
        {
            TypeData.BackgroundProviderName = CurrentBackgroundProvider!.Name;
            TypeData.LaunchServiceName = CurrentLaunchService!.Name;

            Json.ToFile(PathContext.Locate(ImplementationsFile), TypeData);
        }
    }
}
