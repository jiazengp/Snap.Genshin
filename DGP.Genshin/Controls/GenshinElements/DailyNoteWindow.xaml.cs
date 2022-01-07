using DGP.Genshin.ViewModels;
using System.Windows;

namespace DGP.Genshin.Controls.GenshinElements
{
    public partial class DailyNoteWindow : Window
    {
        public DailyNoteWindow(DailyNoteResinViewModel dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
