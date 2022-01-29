using DGP.Genshin.ViewModel.Title;
using ModernWpf.Controls.Primitives;

namespace DGP.Genshin.Control.Title
{
    public partial class DailyNoteTitleBarButton : TitleBarButton
    {
        public DailyNoteTitleBarButton()
        {
            DataContext = App.AutoWired<DailyNoteViewModel>();
            InitializeComponent();
        }
    }
}
