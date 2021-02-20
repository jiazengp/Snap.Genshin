using DGP.Genshin.Controls;
using DGP.Genshin.Data.Character;
using DGP.Genshin.Data.Talent;
using DGP.Genshin.Data.Weapon;
using System;
using System.Collections.Generic;
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
            this.InitializeCharacters();
            this.InitializeWeapons();

            this.SetVisibility();
        }
        private void InitializeWeapons()
        {
            IEnumerable<Weapon> weapons = WeaponManager.Instance.Weapons;
            this.MondstadtWeapons = weapons
                .Where(item => WeaponHelper.IsTodaysMondstadtWeapon(item.Material))
                .Where(item => WeaponManager.UnreleasedPolicyFilter(item))
                .OrderByDescending(item => item.Star)
                .Select(item => new WeaponIcon() { Weapon = item });
            this.LiyueWeapons = weapons
                .Where(item => WeaponHelper.IsTodaysLiyueWeapon(item.Material))
                .Where(item => WeaponManager.UnreleasedPolicyFilter(item))
                .OrderByDescending(item => item.Star)
                .Select(item => new WeaponIcon() { Weapon = item });
        }
        private void InitializeCharacters()
        {
            IEnumerable<Character> chars = CharacterManager.Instance.Characters;
            this.MondstadtCharacters = chars
                .Where(item => TalentHelper.IsTodaysMondstadtMaterial(item.TalentMaterial))
                .Where(item => CharacterManager.UnreleasedPolicyFilter(item))
                .OrderByDescending(item => item.Star)
                .Select(item => new CharacterIcon() { Character = item });
            this.LiyueCharacters = chars
                .Where(item => TalentHelper.IsTodaysLiyueMaterial(item.TalentMaterial))
                .Where(item => CharacterManager.UnreleasedPolicyFilter(item))
                .OrderByDescending(item => item.Star)
                .Select(item => new CharacterIcon() { Character = item });
        }
        private void SetVisibility()
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            this.Visibility1 = today == DayOfWeek.Sunday || today == DayOfWeek.Monday || today == DayOfWeek.Thursday ?
                Visibility.Visible : Visibility.Collapsed;
            this.Visibility2 = today == DayOfWeek.Sunday || today == DayOfWeek.Tuesday || today == DayOfWeek.Friday ?
                Visibility.Visible : Visibility.Collapsed;
            this.Visibility3 = today == DayOfWeek.Sunday || today == DayOfWeek.Wednesday || today == DayOfWeek.Saturday ?
                Visibility.Visible : Visibility.Collapsed;
        }

        private void OnCharacterClicked(object sender, EventArgs e) =>
            //this.CharacterDetailDialog.Character = ((CharacterIcon)sender).Character;
            //this.CharacterDetailDialog.ShowAsync();
            new CharacterDialog((CharacterIcon)sender).ShowAsync();
        private void OnWeaponClicked(object sender, EventArgs e) =>
            //this.WeaponDetailDialog.Weapon = ((WeaponIcon)sender).Weapon;
            //this.WeaponDetailDialog.ShowAsync();
            new WeaponDialog((WeaponIcon)sender).ShowAsync();
        #region propdp

        #region Characters
        public IEnumerable<CharacterIcon> MondstadtCharacters
        {
            get => (IEnumerable<CharacterIcon>)this.GetValue(MondstadtCharactersProperty);
            set => this.SetValue(MondstadtCharactersProperty, value);
        }
        public static readonly DependencyProperty MondstadtCharactersProperty =
            DependencyProperty.Register("MondstadtCharacters", typeof(IEnumerable<CharacterIcon>), typeof(HomePage), new PropertyMetadata(null));

        public IEnumerable<CharacterIcon> LiyueCharacters
        {
            get => (IEnumerable<CharacterIcon>)this.GetValue(LiyueCharactersProperty);
            set => this.SetValue(LiyueCharactersProperty, value);
        }
        public static readonly DependencyProperty LiyueCharactersProperty =
            DependencyProperty.Register("LiyueCharacters", typeof(IEnumerable<CharacterIcon>), typeof(HomePage), new PropertyMetadata(null));
        #endregion

        #region Weapon
        public IEnumerable<WeaponIcon> MondstadtWeapons
        {
            get => (IEnumerable<WeaponIcon>)this.GetValue(MondstadtWeaponsProperty);
            set => this.SetValue(MondstadtWeaponsProperty, value);
        }
        public static readonly DependencyProperty MondstadtWeaponsProperty =
            DependencyProperty.Register("MondstadtWeapons", typeof(IEnumerable<WeaponIcon>), typeof(HomePage), new PropertyMetadata(null));

        public IEnumerable<WeaponIcon> LiyueWeapons
        {
            get => (IEnumerable<WeaponIcon>)this.GetValue(LiyueWeaponsProperty);
            set => this.SetValue(LiyueWeaponsProperty, value);
        }
        public static readonly DependencyProperty LiyueWeaponsProperty =
            DependencyProperty.Register("LiyueWeapons", typeof(IEnumerable<WeaponIcon>), typeof(HomePage), new PropertyMetadata(null));
        #endregion

        #region Visibility
        public Visibility Visibility1
        {
            get => (Visibility)this.GetValue(Visibility1Property);
            set => this.SetValue(Visibility1Property, value);
        }
        public static readonly DependencyProperty Visibility1Property =
            DependencyProperty.Register("Visibility1", typeof(Visibility), typeof(HomePage), new PropertyMetadata(Visibility.Collapsed));
        public Visibility Visibility2
        {
            get => (Visibility)this.GetValue(Visibility2Property);
            set => this.SetValue(Visibility2Property, value);
        }
        public static readonly DependencyProperty Visibility2Property =
            DependencyProperty.Register("Visibility2", typeof(Visibility), typeof(HomePage), new PropertyMetadata(Visibility.Collapsed));
        public Visibility Visibility3
        {
            get => (Visibility)this.GetValue(Visibility3Property);
            set => this.SetValue(Visibility3Property, value);
        }
        public static readonly DependencyProperty Visibility3Property =
            DependencyProperty.Register("Visibility3", typeof(Visibility), typeof(HomePage), new PropertyMetadata(Visibility.Collapsed));
        #endregion

        #endregion
    }

}
