using DGP.Genshin.ViewModels;
using ModernWpf.Controls.Primitives;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// DailyNoteTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class DailyNoteTitleBarButton : TitleBarButton
    {
        public DailyNoteTitleBarButton()
        {
            DataContext = App.GetViewModel<DailyNoteViewModel>();
            InitializeComponent();
        }
    }
}
