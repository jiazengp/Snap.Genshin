using DGP.Genshin.Data.Weapon;
using ModernWpf.Controls;
using System.Windows;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// WeaponDialog.xaml 的交互逻辑
    /// </summary>
    public partial class WeaponDialog : ContentDialog
    {
        public WeaponDialog()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }
        public WeaponDialog(Weapon w) : this()
        {
            this.Weapon = w;
        }
        public WeaponDialog(WeaponIcon wi) : this(wi.Weapon) { }

        public Weapon Weapon
        {
            get => (Weapon)this.GetValue(WeaponProperty);
            set => this.SetValue(WeaponProperty, value);
        }
        public static readonly DependencyProperty WeaponProperty =
            DependencyProperty.Register("Weapon", typeof(Weapon), typeof(WeaponDialog), new PropertyMetadata(null));
    }
}
