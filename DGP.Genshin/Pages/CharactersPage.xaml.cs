using DGP.Genshin.Services;
using DGP.Snap.Framework.Core.LifeCycling;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// CharactersPage.xaml 的交互逻辑
    /// </summary>
    public partial class CharactersPage : Page
    {
        public CharactersPage()
        {
            this.DataContext = LifeCycle.InstanceOf<DataService>();
            this.InitializeComponent();
        }
    }
}
