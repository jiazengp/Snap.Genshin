using DGP.Genshin.Data.Character;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// WeeklyMaterialPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeeklyMaterialPage : Page
    {
        public WeeklyMaterialPage()
        {
            DataContext = this;
            this.InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Characters = CharacterManager.Instance.Characters;
        }
        public CharacterCollection Characters
        {
            get => (CharacterCollection)this.GetValue(CharactersProperty);
            set => this.SetValue(CharactersProperty, value);
        }
        public static readonly DependencyProperty CharactersProperty =
            DependencyProperty.Register("Characters", typeof(CharacterCollection), typeof(WeeklyMaterialPage), new PropertyMetadata(null));


    }
}
