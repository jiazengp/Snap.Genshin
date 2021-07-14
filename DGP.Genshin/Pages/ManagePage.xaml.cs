using DGP.Genshin.Data.Characters;
using DGP.Genshin.Services;
using DGP.Snap.Framework.Core.LifeCycling;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// ManagePage.xaml 的交互逻辑
    /// </summary>
    public partial class ManagePage : Page
    {
        public ManagePage()
        {
            this.DataContext = LifeCycle.InstanceOf<DataService>();
            InitializeComponent();
        }

        private async void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            switch (((TabItem)TabHost.SelectedItem).Header)
            {
                case "角色":
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "武器":
                    await WeaponEditDialog.ShowAsync();
                    break;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DataService data = LifeCycle.InstanceOf<DataService>();
            switch (((TabItem)TabHost.SelectedItem).Header)
            {
                case "角色":
                    data.Characters.Remove(data.SelectedCharacter);
                    data.SelectedCharacter = null;
                    break;
                case "武器":
                    data.Weapons.Remove(data.SelectedWeapon);
                    data.SelectedWeapon = null;
                    break;
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            DataService data = LifeCycle.InstanceOf<DataService>();
            switch (((TabItem)TabHost.SelectedItem).Header)
            {
                case "角色":
                    Character character = new Character()
                    {
                        Talent = data.DailyTalents.First(),
                        Boss = data.Bosses.First(),
                        GemStone = data.GemStones.First(),
                        Local = data.Locals.First(),
                        Monster = data.Monsters.First(),
                        Weekly = data.WeeklyTalents.First()
                    };
                    data.Characters.Add(character);
                    data.SelectedCharacter = character;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "武器":
                    Data.Weapons.Weapon weapon = new Data.Weapons.Weapon()
                    {
                        Ascension = data.DailyWeapons.First(),
                        Elite = data.Elites.First(),
                        Monster = data.Monsters.First()
                    };
                    data.Weapons.Add(weapon);
                    data.SelectedWeapon = weapon;
                    await WeaponEditDialog.ShowAsync();
                    break;
            }
        }
    }
    
}
