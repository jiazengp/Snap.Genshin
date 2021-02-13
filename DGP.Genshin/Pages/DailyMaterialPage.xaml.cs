using DGP.Genshin.Data.Character;
using DGP.Genshin.Data.Weapon;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// DialyMaterialPage.xaml 的交互逻辑
    /// </summary>
    public partial class DailyMaterialPage : Page
    {
        public DailyMaterialPage()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Characters = CharacterManager.Instance.Characters;
            this.Weapons = WeaponManager.Instance.Weapons;
        }
        public CharacterCollection Characters
        {
            get => (CharacterCollection)this.GetValue(CharactersProperty);
            set => this.SetValue(CharactersProperty, value);
        }
        public static readonly DependencyProperty CharactersProperty =
            DependencyProperty.Register("Characters", typeof(CharacterCollection), typeof(DailyMaterialPage), new PropertyMetadata(null));

        public WeaponCollection Weapons
        {
            get => (WeaponCollection)this.GetValue(WeaponsProperty);
            set => this.SetValue(WeaponsProperty, value);
        }
        public static readonly DependencyProperty WeaponsProperty =
            DependencyProperty.Register("Weapons", typeof(WeaponCollection), typeof(DailyMaterialPage), new PropertyMetadata(null));


    }
}
