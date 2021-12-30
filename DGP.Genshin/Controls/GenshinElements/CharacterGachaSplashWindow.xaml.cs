using System.Windows;

namespace DGP.Genshin.Controls.GenshinElements
{
    /// <summary>
    /// CharacterGachaSplashWindwo.xaml 的交互逻辑
    /// </summary>
    public partial class CharacterGachaSplashWindow : Window
    {
        public CharacterGachaSplashWindow()
        {
            DataContext = this;
            InitializeComponent();
        }
        public string? Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(CharacterGachaSplashWindow));

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
