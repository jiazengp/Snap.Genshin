using DGP.Genshin.DataViewer.Helper;
using DGP.Genshin.DataViewer.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DGP.Genshin.DataViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }
  
    }
}
