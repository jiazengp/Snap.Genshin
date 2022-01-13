using System.Windows;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// WhatsNewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WhatsNewWindow : Window
    {
        public WhatsNewWindow()
        {
            DataContext = this;
            InitializeComponent();
        }
        public string? ReleaseNote
        {
            get { return (string)GetValue(ReleaseNoteProperty); }
            set { SetValue(ReleaseNoteProperty, value); }
        }
        public static readonly DependencyProperty ReleaseNoteProperty =
            DependencyProperty.Register("ReleaseNote", typeof(string), typeof(WhatsNewWindow), new PropertyMetadata(null));
    }
}
