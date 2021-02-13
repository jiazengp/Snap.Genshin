using DGP.Genshin.Data.Character;
using DGP.Genshin.Data.Weapon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// CalculationPage.xaml 的交互逻辑
    /// </summary>
    public partial class CalculationPage : Page, INotifyPropertyChanged
    {
        public CalculationPage()
        {
            this.DataContext = this;
            this.InitializeComponent();
            this.SelectedCharTypeChanged += this.OnCharacterChanged;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetCharacters();
            this.SelectedChar = this.Characters.First();
            this.SetWeapons();
            this.SelectedWeapon = this.Weapons.First();
        }
        private void SetCharacters() => this.Characters = CharacterManager.Instance.Characters
                .Where(c => c.CharacterName != "旅行者(风)" && c.CharacterName != "旅行者(岩)")
                .Where(c => CharacterManager.UnreleasedPolicyFilter(c))
                .OrderByDescending(c => c.Star);
        private void SetWeapons() => this.Weapons = WeaponManager.Instance.Weapons
                .Where(item => WeaponManager.UnreleasedPolicyFilter(item))
                .Where(w => w.Type == this.SelectedChar.WeaponType)
                .OrderByDescending(w => w.Star);

        private void OnCharacterChanged()
        {
            this.SetWeapons();
            this.SelectedWeapon = this.Weapons.First();
        }

        public IEnumerable<Character> Characters
        {
            get => (IEnumerable<Character>)this.GetValue(CharactersProperty);
            set => this.SetValue(CharactersProperty, value);
        }
        public static readonly DependencyProperty CharactersProperty =
            DependencyProperty.Register("Characters", typeof(IEnumerable<Character>), typeof(CalculationPage), new PropertyMetadata(null));

        public IEnumerable<Weapon> Weapons
        {
            get => (IEnumerable<Weapon>)this.GetValue(WeaponsProperty);
            set => this.SetValue(WeaponsProperty, value);
        }
        public static readonly DependencyProperty WeaponsProperty =
            DependencyProperty.Register("Weapons", typeof(IEnumerable<Weapon>), typeof(CalculationPage), new PropertyMetadata(null));

        //we need to notify char weapon type changed.
        private readonly Action SelectedCharTypeChanged;

        private Character selectedChar;
        public Character SelectedChar
        {
            get => this.selectedChar; set
            {
                WeaponType? p = this.selectedChar?.WeaponType;
                this.Set(ref this.selectedChar, value);
                if (p != value.WeaponType)
                {
                    this.SelectedCharTypeChanged?.Invoke();
                }
            }
        }

        private Weapon selectedWeapon;
        public Weapon SelectedWeapon { get => this.selectedWeapon; set => this.Set(ref this.selectedWeapon, value); }

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
