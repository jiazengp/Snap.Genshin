using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Core.Plugins;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
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
            Plugins = App.Current.ServiceManager.PluginService.Plugins;

            OpenPluginFolderCommand = new RelayCommand(OpenPluginsFolder);
        }

        private void OpenPluginsFolder()
        {
            Process.Start("explorer.exe", Path.GetFullPath(PluginFolder, AppContext.BaseDirectory));
        }
    }
}
