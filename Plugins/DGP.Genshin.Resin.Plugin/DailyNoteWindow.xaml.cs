using DGP.Genshin.Resin.Plugin.ViewModel;
using System;
using System.Windows;

namespace DGP.Genshin.Resin.Plugin
{
    public partial class DailyNoteWindow : Window
    {
        
        public DailyNoteWindow(DailyNoteResinViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
