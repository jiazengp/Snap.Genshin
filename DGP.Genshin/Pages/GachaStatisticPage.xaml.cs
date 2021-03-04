using DGP.Genshin.Controls;
using DGP.Genshin.Data.Character;
using DGP.Genshin.Data.Weapon;
using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Service;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    public partial class GachaStatisticPage : Page, INotifyPropertyChanged
    {
        public GachaStatisticPage()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                IDictionary<string, IEnumerable<GachaLogItem>> dict = GachaStatisticService.Instance.Data.GachaLogs;
                this.Dispatcher.Invoke(() =>
                {
                    this.NormalPoolSource = dict["200"];
                    this.NovicePoolSource = dict["100"];
                    this.CharacterPoolSource = dict["301"];
                    this.WeaponPoolSource = dict["302"];

                    this.NormalPool = this.ConvertToIcon(dict["200"]);
                    this.NovicePool = this.ConvertToIcon(dict["100"]);
                    this.CharacterPool = this.ConvertToIcon(dict["301"]);
                    this.WeaponPool = this.ConvertToIcon(dict["302"]);
                });
            });
        }

        public IEnumerable<UserControl> ConvertToIcon(IEnumerable<GachaLogItem> items)
        {
            foreach (var item in items)
            {
                switch (item.ItemType)
                {
                    case "武器":
                        yield return new WeaponIcon() { Weapon = WeaponManager.Instance.Weapons.First(i => i.WeaponName == item.Name) };
                        break;
                    case "角色":
                        yield return new CharacterIcon() { Character = CharacterManager.Instance.Characters.First(i => i.CharacterName == item.Name) };
                        break;
                }
            }
        }

        #region Property

        #region NormalPool
        private IEnumerable<UserControl> normalPool;
        public IEnumerable<UserControl> NormalPool { get => this.normalPool; set => this.Set(ref this.normalPool, value); }
        #endregion

        #region NormalPoolSource
        private IEnumerable<GachaLogItem> normalPoolSource;
        public IEnumerable<GachaLogItem> NormalPoolSource { get => this.normalPoolSource; set => this.Set(ref this.normalPoolSource, value); }
        #endregion

        #region NovicePool
        private IEnumerable<UserControl> novicePool;
        public IEnumerable<UserControl> NovicePool
        {
            get => this.novicePool;
            set => this.Set(ref this.novicePool, value);
        }
        #endregion

        #region NovicePoolSource
        private IEnumerable<GachaLogItem> novicePoolSource;
        public IEnumerable<GachaLogItem> NovicePoolSource { get => this.novicePoolSource; set => this.Set(ref this.novicePoolSource, value); }
        #endregion

        #region CharacterPool
        private IEnumerable<UserControl> characterPool;
        public IEnumerable<UserControl> CharacterPool
        {
            get => this.characterPool;
            set => this.Set(ref this.characterPool, value);
        }
        #endregion

        #region NormalPoolSource
        private IEnumerable<GachaLogItem> characterPoolSource;
        public IEnumerable<GachaLogItem> CharacterPoolSource { get => this.characterPoolSource; set => this.Set(ref this.characterPoolSource, value); }
        #endregion

        #region WeaponPool
        private IEnumerable<UserControl> weaponPool;

        public IEnumerable<UserControl> WeaponPool
        {
            get => this.weaponPool;
            set => this.Set(ref this.weaponPool, value);
        }
        #endregion

        #region NovicePoolSource
        private IEnumerable<GachaLogItem> weaponPoolSource;
        public IEnumerable<GachaLogItem> WeaponPoolSource { get => this.weaponPoolSource; set => this.Set(ref this.weaponPoolSource, value); }
        #endregion

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
