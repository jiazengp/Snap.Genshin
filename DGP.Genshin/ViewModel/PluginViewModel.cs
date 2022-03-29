using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DGP.Genshin.Core.Plugins;
using Snap.Core.DependencyInjection;
using Snap.Data.Utility;
using System.Collections.Generic;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 插件视图模型
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    internal class PluginViewModel : ObservableObject
    {
        private const string PluginFolder = "Plugins";
        private const string PluginsLink = "https://www.snapgenshin.com/documents/extensions/";

        private IEnumerable<IPlugin> plugins;

        /// <summary>
        /// 构造一个新的插件视图模型
        /// </summary>
        public PluginViewModel()
        {
            this.Plugins = App.Current.PluginService.Plugins;

            this.OpenPluginFolderCommand = new RelayCommand(() => FileExplorer.Open(PathContext.Locate(PluginFolder)));
            this.OpenPluginListLinkCommand = new RelayCommand(() => Browser.Open(PluginsLink));
        }

        /// <summary>
        /// 插件集合
        /// </summary>
        public IEnumerable<IPlugin> Plugins
        {
            get => this.plugins;

            [MemberNotNull(nameof(plugins))]
            set => this.SetProperty(ref this.plugins, value);
        }

        /// <summary>
        /// 打开插件文件夹命令
        /// </summary>
        public ICommand OpenPluginFolderCommand { get; }

        /// <summary>
        /// 打开插件列表链接命令
        /// </summary>
        public ICommand OpenPluginListLinkCommand { get; }
    }
}