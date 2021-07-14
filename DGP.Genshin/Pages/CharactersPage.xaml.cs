using DGP.Genshin.Data;
using DGP.Genshin.Data.Characters;
using DGP.Genshin.Services;
using DGP.Snap.Framework.Core.LifeCycling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// CharactersPage.xaml 的交互逻辑
    /// </summary>
    public partial class CharactersPage : Page
    {
        public CharactersPage()
        {
            this.DataContext = this;
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Characters = LifeCycle.InstanceOf<DataService>().Characters;
        }
        #region propdp
        public ObservableCollection<Character> Characters
        {
            get { return (ObservableCollection<Character>)GetValue(CharactersProperty); }
            set { SetValue(CharactersProperty, value); }
        }
        public static readonly DependencyProperty CharactersProperty =
            DependencyProperty.Register("Characters", typeof(ObservableCollection<Character>), typeof(CharactersPage), new PropertyMetadata(null));
        #endregion
    }
}
