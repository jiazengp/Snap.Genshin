using System.Windows;

namespace DGP.Genshin.Control
{
    public sealed partial class WhatsNewWindow : Window
    {
        public WhatsNewWindow()
        {
            DataContext = this;
            InitializeComponent();
        }
        public string? ReleaseNote
        {
            get => (string)GetValue(ReleaseNoteProperty);

            set => SetValue(ReleaseNoteProperty, value);
        }
        public static readonly DependencyProperty ReleaseNoteProperty =
            DependencyProperty.Register(nameof(ReleaseNote), typeof(string), typeof(WhatsNewWindow));
    }
}
