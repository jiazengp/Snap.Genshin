using DGP.Genshin.Data;
using DGP.Genshin.Data.Characters;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var a = DataManager.Instance.GetAllCharacters();
            Debug.WriteLine(a.Count());
            Characters = a;
        }


        #region propdp
        public IEnumerable<Character> Characters
        {
            get { return (IEnumerable<Character>)GetValue(CharactersProperty); }
            set { SetValue(CharactersProperty, value); }
        }
        public static readonly DependencyProperty CharactersProperty =
            DependencyProperty.Register("Characters", typeof(IEnumerable<Character>), typeof(HomePage), new PropertyMetadata(null));
        #endregion
    }

}
