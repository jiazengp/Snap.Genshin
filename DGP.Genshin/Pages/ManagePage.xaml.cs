using DGP.Genshin.DataModel.Characters;
using DGP.Genshin.Services;
using DGP.Snap.Framework.Extensions.System;
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
            DataContext = MetaDataService.Instance;
            InitializeComponent();
            this.Log("unitialized");
        }

        private async void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            switch (((TabItem)TabHost.SelectedItem).Header)
            {
                case "角色":
                    CharacterEditDialog.DataContext = MetaDataService.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "武器":
                    WeaponEditDialog.DataContext = MetaDataService.Instance;
                    await WeaponEditDialog.ShowAsync();
                    break;
                default:
                    break;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MetaDataService data = MetaDataService.Instance;
            switch (((TabItem)TabHost.SelectedItem).Header)
            {
                case "角色":
                    if (data.SelectedCharacter is not null)
                    {
                        data.Characters.Remove(data.SelectedCharacter);
                        data.SelectedCharacter = null;
                    }
                    break;
                case "武器":
                    if (data.SelectedWeapon is not null)
                    {
                        data.Weapons.Remove(data.SelectedWeapon);
                        data.SelectedWeapon = null;
                    }
                    break;
                default:
                    break;
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            MetaDataService data = MetaDataService.Instance;
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
                        Weekly = data.WeeklyTalents.First(),
                        Element = data.Elements.First().Source
                    };
                    data.Characters.Add(character);
                    data.SelectedCharacter = character;
                    CharacterEditDialog.DataContext = MetaDataService.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "武器":
                    DataModel.Weapons.Weapon weapon = new DataModel.Weapons.Weapon()
                    {
                        Ascension = data.DailyWeapons.First(),
                        Elite = data.Elites.First(),
                        Monster = data.Monsters.First()
                    };
                    data.Weapons.Add(weapon);
                    data.SelectedWeapon = weapon;
                    WeaponEditDialog.DataContext = MetaDataService.Instance;
                    await WeaponEditDialog.ShowAsync();
                    break;
                default:
                    break;
            }
        }
    }
}
