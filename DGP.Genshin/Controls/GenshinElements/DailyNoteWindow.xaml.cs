using DGP.Genshin.ViewModels.TitleBarButtons;
using System.Windows;

namespace DGP.Genshin.Controls.GenshinElements
{
    public partial class DailyNoteWindow : Window
    {
        public DailyNoteWindow()
        {
            DataContext = App.GetViewModel<DailyNoteViewModel>();
            InitializeComponent();
        }
    }
}
