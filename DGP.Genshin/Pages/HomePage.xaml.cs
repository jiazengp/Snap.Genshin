using DGP.Genshin.Data;
using DGP.Genshin.Data.Characters;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

    }

}
