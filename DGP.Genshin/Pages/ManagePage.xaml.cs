using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.DataModel.Characters;
using DGP.Genshin.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// we didn't use ICommand for some historic reason
    /// </summary>
    public partial class ManagePage : Page
    {
        public ManagePage()
        {
            DataContext = MetadataViewModel.Instance;
            InitializeComponent();
            this.Log("unitialized");
        }

        private async void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            switch (((TabItem)TabHost.SelectedItem).Header)
            {
                case "Boss材料":
                    CharacterEditDialog.DataContext = MetadataViewModel.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "城市":
                    CharacterEditDialog.DataContext = MetadataViewModel.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "角色":
                    CharacterEditDialog.DataContext = MetadataViewModel.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "日常天赋":
                    CharacterEditDialog.DataContext = MetadataViewModel.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "武器材料":
                    CharacterEditDialog.DataContext = MetadataViewModel.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "精英怪物":
                    CharacterEditDialog.DataContext = MetadataViewModel.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "区域材料":
                    CharacterEditDialog.DataContext = MetadataViewModel.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "普通怪物":
                    CharacterEditDialog.DataContext = MetadataViewModel.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "武器":
                    WeaponEditDialog.DataContext = MetadataViewModel.Instance;
                    await WeaponEditDialog.ShowAsync();
                    break;
                case "周常材料":
                    CharacterEditDialog.DataContext = MetadataViewModel.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                default:
                    break;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MetadataViewModel data = MetadataViewModel.Instance;
            switch (((TabItem)TabHost.SelectedItem).Header)
            {
                case "角色":
                    data.Characters?.Remove(data.SelectedCharacter!);
                    data.SelectedCharacter = null;
                    break;
                case "武器":
                    data.Weapons?.Remove(data.SelectedWeapon!);
                    data.SelectedWeapon = null;
                    break;
                default:
                    break;
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            MetadataViewModel data = MetadataViewModel.Instance;
            switch (((TabItem)TabHost.SelectedItem).Header)
            {
                case "角色":
                    Character character = new()
                    {
                        Talent = data.DailyTalents?.First(),
                        Boss = data.Bosses?.First(),
                        GemStone = data.GemStones?.First(),
                        Local = data.Locals?.First(),
                        Monster = data.Monsters?.First(),
                        Weekly = data.WeeklyTalents?.First(),
                        Element = data.Elements?.First().Source
                    };
                    data.Characters?.Add(character);
                    data.SelectedCharacter = character;
                    CharacterEditDialog.DataContext = MetadataViewModel.Instance;
                    await CharacterEditDialog.ShowAsync();
                    break;
                case "武器":
                    DataModel.Weapons.Weapon weapon = new()
                    {
                        Ascension = data.DailyWeapons?.First(),
                        Elite = data.Elites?.First(),
                        Monster = data.Monsters?.First()
                    };
                    data.Weapons?.Add(weapon);
                    data.SelectedWeapon = weapon;
                    WeaponEditDialog.DataContext = MetadataViewModel.Instance;
                    await WeaponEditDialog.ShowAsync();
                    break;
                default:
                    break;
            }
        }
    }
}
