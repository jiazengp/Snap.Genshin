using DGP.Genshin.Data.Weapon;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// WeaponMaterialPresenter.xaml 的交互逻辑
    /// </summary>
    public partial class WeaponMaterialPresenter : UserControl
    {
        public WeaponMaterialPresenter()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        public WeaponMaterial WeaponMaterial
        {
            get => (WeaponMaterial)this.GetValue(WeaponMaterialProperty);
            set => this.SetValue(WeaponMaterialProperty, value);
        }
        public static readonly DependencyProperty WeaponMaterialProperty =
            DependencyProperty.Register("WeaponMaterial", typeof(WeaponMaterial), typeof(WeaponMaterialPresenter), new PropertyMetadata(WeaponMaterial.Decarabians));
    }
}
