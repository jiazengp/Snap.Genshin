using DGP.Genshin.Core.Background.Abstraction;
using DGP.Genshin.Helper;
using Snap.Data.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Core.ImplementationSwitching
{
    internal class SwitchableImplementationManager
    {
        private const string ImplementationsFile = "implementations.json";
        private class ImplmentationTypeData
        {
            public string BackgroundProviderName { get; set; } = SwitchableImplementationAttribute.DefaultName;
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
        public List<SwitchableEntry<IBackgroundProvider>> BackgroundProviders { get; internal set; } = new();
        public SwitchableEntry<IBackgroundProvider>? CurrentBackgroundProvider { get; set; }
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
        }

        public void UnInitialize()
        {
            Json.ToFile(PathContext.Locate(ImplementationsFile), TypeData);
        }
    }
}
