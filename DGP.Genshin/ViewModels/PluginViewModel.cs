using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Helpers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(InjectAs.Transient)]
    public class PluginViewModel : ObservableObject
    {
        private const string PluginFolder = "Plugins";

        private IEnumerable<IPlugin> plugins;

        public IEnumerable<IPlugin> Plugins
        {
            get => plugins;
            [MemberNotNull(nameof(plugins))]
            set => SetProperty(ref plugins, value);
        }

        public ICommand OpenPluginFolderCommand { get; }

        public PluginViewModel()
        {
            Plugins = App.Current.PluginService.Plugins;

            OpenPluginFolderCommand = new RelayCommand(OpenPluginsFolder);
        }

        private void OpenPluginsFolder()
        {
            Process.Start("explorer.exe", PathContext.Locate(PluginFolder));
        }
    }
}
