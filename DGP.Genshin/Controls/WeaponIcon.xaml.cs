using DGP.Genshin.Data.Weapon;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// WeaponIcon.xaml 的交互逻辑
    /// </summary>
    public partial class WeaponIcon : UserControl
    {
        public WeaponIcon()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        public Weapon Weapon
        {
            get => (Weapon)this.GetValue(WeaponProperty);
            set => this.SetValue(WeaponProperty, value);
        }
        public static readonly DependencyProperty WeaponProperty =
            DependencyProperty.Register("Weapon", typeof(Weapon), typeof(WeaponIcon), new PropertyMetadata(null));

        private void IconClick(object sender, RoutedEventArgs e) => new WeaponDialog(this.Weapon).ShowAsync();
    }
}
