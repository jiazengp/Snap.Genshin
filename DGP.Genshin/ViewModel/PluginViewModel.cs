using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Helper;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    public class PluginViewModel : ObservableObject
    {
        private const string PluginFolder = "Plugins";
        private const string pluginsLink = "https://github.com/DGP-Studio/Snap.Genshin/tree/main/Plugins";
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

            OpenPluginFolderCommand = new RelayCommand(OpenPluginsFolder);
            OpenPluginListLinkCommand = new RelayCommand(OpenPluginListLink);
        }

        private void OpenPluginsFolder()
        {
            Process.Start("explorer.exe", PathContext.Locate(PluginFolder));
        }
        private void OpenPluginListLink()
        {
            Process.Start(new ProcessStartInfo() { FileName = pluginsLink, UseShellExecute = true });
        }
    }
}
