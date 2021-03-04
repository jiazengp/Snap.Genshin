using DGP.Genshin.Controls;
using DGP.Genshin.Data.Character;
using DGP.Genshin.Data.Weapon;
using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Service;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Windows.System;

namespace DGP.Genshin.Pages
{
    public partial class GachaStatisticPage : Page, INotifyPropertyChanged
    {
        

        public GachaStatisticPage()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            IDictionary<string, IEnumerable<GachaLogItem>> dict = GachaStatisticService.Instance.Data.GachaLogs;
            NormalPoolSource = dict["200"];
            NovicePoolSource = dict["100"];
            CharacterPoolSource = dict["301"];
            WeaponPoolSource = dict["302"];

            NormalPool = ConvertToIcon(dict["200"]);
            NovicePool = ConvertToIcon(dict["100"]);
            CharacterPool = ConvertToIcon(dict["301"]);
            WeaponPool = ConvertToIcon(dict["302"]);
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
        public IEnumerable<UserControl> NormalPool { get => normalPool; set => Set(ref normalPool, value); }
        #endregion

        #region NormalPoolSource
        private IEnumerable<GachaLogItem> normalPoolSource;
        public IEnumerable<GachaLogItem> NormalPoolSource { get => normalPoolSource; set => Set(ref normalPoolSource, value); }
        #endregion

        #region NovicePool
        private IEnumerable<UserControl> novicePool;
        public IEnumerable<UserControl> NovicePool
        {
            get => novicePool;
            set => Set(ref novicePool, value);
        }
        #endregion

        #region NovicePoolSource
        private IEnumerable<GachaLogItem> novicePoolSource;
        public IEnumerable<GachaLogItem> NovicePoolSource { get => novicePoolSource; set => Set(ref novicePoolSource, value); }
        #endregion

        #region CharacterPool
        private IEnumerable<UserControl> characterPool;
        public IEnumerable<UserControl> CharacterPool
        {
            get { return characterPool; }
            set { Set(ref characterPool, value); }
        }
        #endregion

        #region NormalPoolSource
        private IEnumerable<GachaLogItem> characterPoolSource;
        public IEnumerable<GachaLogItem> CharacterPoolSource { get => characterPoolSource; set => Set(ref characterPoolSource, value); }
        #endregion

        #region WeaponPool
        private IEnumerable<UserControl> weaponPool;

        public IEnumerable<UserControl> WeaponPool
        {
            get { return weaponPool; }
            set { Set(ref weaponPool, value); }
        }
        #endregion

        #region NovicePoolSource
        private IEnumerable<GachaLogItem> weaponPoolSource;
        public IEnumerable<GachaLogItem> WeaponPoolSource { get => weaponPoolSource; set => Set(ref weaponPoolSource, value); }
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
