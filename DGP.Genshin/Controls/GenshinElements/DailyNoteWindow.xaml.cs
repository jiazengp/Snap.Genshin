using System.Windows;

namespace DGP.Genshin.Controls.GenshinElements
{
    public partial class DailyNoteWindow : Window
    {
        public DailyNoteWindow(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
