using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Helpers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(InjectAs.Transient)]
    public class PluginViewModel : ObservableObject
    {
        private const string PluginFolder = "Plugins";

        private IEnumerable<IPlugin> plugins;
        private IRelayCommand openPluginFolderCommand;

        public IEnumerable<IPlugin> Plugins
        {
            get => plugins;
            [MemberNotNull(nameof(plugins))]
            set => SetProperty(ref plugins, value);
        }
        public IRelayCommand OpenPluginFolderCommand
        {
            get => openPluginFolderCommand;
            [MemberNotNull(nameof(openPluginFolderCommand))]
            set => openPluginFolderCommand = value;
        }

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
