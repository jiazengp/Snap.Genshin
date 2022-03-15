using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Helper;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using Snap.Data.Utility;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class PluginViewModel : ObservableObject
    {
        private const string PluginFolder = "Plugins";
        private const string pluginsLink = "https://www.snapgenshin.com/documents/extensions/";
        private IEnumerable<IPlugin> plugins;

        public IEnumerable<IPlugin> Plugins
        {
            get => plugins;

            [MemberNotNull(nameof(plugins))]
            set => SetProperty(ref plugins, value);
        }

        public ICommand OpenPluginFolderCommand { get; }
        public ICommand OpenPluginListLinkCommand { get; }

        public PluginViewModel()
        {
            Plugins = App.Current.PluginService.Plugins;

            OpenPluginFolderCommand = new RelayCommand(() => FileExplorer.Open(PathContext.Locate(PluginFolder)));
            OpenPluginListLinkCommand = new RelayCommand(() => Browser.Open(pluginsLink));
        }
    }
}
