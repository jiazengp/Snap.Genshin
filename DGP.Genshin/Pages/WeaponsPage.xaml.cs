using DGP.Genshin.Data.Weapons;
using DGP.Genshin.Services;
using DGP.Snap.Framework.Core.LifeCycling;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// WeaponsPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeaponsPage : Page
    {
        public WeaponsPage()
        {
            this.DataContext = LifeCycle.InstanceOf<DataService>();
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Weapon> a = LifeCycle.InstanceOf<DataService>().Weapons;
            this.Weapons = a;
        }
        #region propdp
        public ObservableCollection<Weapon> Weapons
        {
            get => (ObservableCollection<Weapon>)this.GetValue(WeaponsProperty);
            set => this.SetValue(WeaponsProperty, value);
        }
        public static readonly DependencyProperty WeaponsProperty =
            DependencyProperty.Register("Weapons", typeof(ObservableCollection<Weapon>), typeof(WeaponsPage), new PropertyMetadata(null));
        #endregion
    }
}
