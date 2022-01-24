using DGP.Genshin.ViewModel;
using System.Windows;

namespace DGP.Genshin.Control.GenshinElement
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
