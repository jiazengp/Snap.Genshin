﻿using DGP.Genshin.Services;
using DGP.Snap.Framework.Extensions.System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// WeeklyPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeeklyPage : Page
    {
        public WeeklyPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await Task.Delay(1000);
            this.DataContext = WeeklyViewService.Instance;
            this.Log("initialized");
        }
    }
}
