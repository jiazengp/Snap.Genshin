using DGP.Genshin.Core.Plugins;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.ViewModels
{
    public class PluginViewModel : ObservableObject
    {
        public IEnumerable<IPlugin> Plugins { get; set; }

        public PluginViewModel()
        {

        }
    }
}
