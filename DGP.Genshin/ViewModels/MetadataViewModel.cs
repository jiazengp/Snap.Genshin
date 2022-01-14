using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Controls.GenshinElements;
using DGP.Genshin.DataModels;
using DGP.Genshin.DataModels.Characters;
using DGP.Genshin.DataModels.GachaStatistics;
using DGP.Genshin.DataModels.Materials.GemStones;
using DGP.Genshin.DataModels.Materials.Locals;
using DGP.Genshin.DataModels.Materials.Monsters;
using DGP.Genshin.DataModels.Materials.Talents;
using DGP.Genshin.DataModels.Materials.Weeklys;
using DGP.Genshin.DataModels.Weapons;
using DGP.Genshin.Helpers;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows.Data;
using WeaponMaterial = DGP.Genshin.DataModels.Materials.Weapons.Weapon;

namespace DGP.Genshin.ViewModels
{
    /// <summary>
    /// 元数据视图模型
    /// 存有各类共享物品数据
    /// </summary>
    [ViewModel(ViewModelType.Singleton)]
    public class MetadataViewModel : ObservableObject
    {
        #region Consts
        private const string BossesJson = "bosses.json";
        private const string CharactersJson = "characters.json";
        private const string CitiesJson = "cities.json";
        private const string DailyTalentsJson = "dailytalents.json";
        private const string DailyWeaponsJson = "dailyweapons.json";
        private const string ElementsJson = "elements.json";
        private const string ElitesJson = "elites.json";
        private const string GemStonesJson = "gemstones.json";
        private const string LocalsJson = "locals.json";
        private const string MonstersJson = "monsters.json";
        private const string StarsJson = "stars.json";
        private const string WeaponsJson = "weapons.json";
        private const string WeaponTypesJson = "weapontypes.json";
        private const string WeeklyTalentsJson = "weeklytalents.json";
        private const string GachaEventJson = "gachaevents.json";

        private const string folderPath = "Metadata\\";
        #endregion

        #region Collections
        [IntegrityAware]
        public ObservableCollection<Boss>? Bosses
        {
            get
            {
                bosses ??= Json.ToObject<ObservableCollection<Boss>>(Read(BossesJson));
                return bosses;
            }
            set => bosses = value;
        }
        private ObservableCollection<Boss>? bosses;

        [IntegrityAware(IsCharacter = true)]
        public ObservableCollection<Character>? Characters
        {
            get
            {
                characters ??= Json.ToObject<ObservableCollection<Character>>(Read(CharactersJson));
                return characters;
            }
            set => characters = value;
        }
        private ObservableCollection<Character>? characters;

        [IntegrityAware]
        public ObservableCollection<KeySource>? Cities
        {
            get
            {
                cities ??= Json.ToObject<ObservableCollection<KeySource>>(Read(CitiesJson));
                return cities;
            }
            set => cities = value;
        }
        private ObservableCollection<KeySource>? cities;

        [IntegrityAware]
        public ObservableCollection<Talent>? DailyTalents
        {
            get
            {
                dailyTalents ??= Json.ToObject<ObservableCollection<Talent>>(Read(DailyTalentsJson));
                return dailyTalents;
            }
            set => dailyTalents = value;
        }
        private ObservableCollection<Talent>? dailyTalents;

        [IntegrityAware]
        public ObservableCollection<WeaponMaterial>? DailyWeapons
        {
            get
            {
                dailyWeapons ??= Json.ToObject<ObservableCollection<WeaponMaterial>>(Read(DailyWeaponsJson));
                return dailyWeapons;
            }
            set => dailyWeapons = value;
        }
        private ObservableCollection<WeaponMaterial>? dailyWeapons;

        [IntegrityAware]
        public ObservableCollection<KeySource>? Elements
        {
            get
            {
                elements ??= Json.ToObject<ObservableCollection<KeySource>>(Read(ElementsJson));
                return elements;
            }
            set => elements = value;
        }
        private ObservableCollection<KeySource>? elements;

        [IntegrityAware]
        public ObservableCollection<Elite>? Elites
        {
            get
            {
                elites ??= Json.ToObject<ObservableCollection<Elite>>(Read(ElitesJson));
                return elites;
            }
            set => elites = value;
        }
        private ObservableCollection<Elite>? elites;

        [IntegrityAware]
        public ObservableCollection<GemStone>? GemStones
        {
            get
            {
                gemstones ??= Json.ToObject<ObservableCollection<GemStone>>(Read(GemStonesJson));
                return gemstones;
            }
            set => gemstones = value;
        }
        private ObservableCollection<GemStone>? gemstones;

        [IntegrityAware]
        public ObservableCollection<Local>? Locals
        {
            get
            {
                locals ??= Json.ToObject<ObservableCollection<Local>>(Read(LocalsJson));
                return locals;
            }
            set => locals = value;
        }
        private ObservableCollection<Local>? locals;

        [IntegrityAware]
        public ObservableCollection<Monster>? Monsters
        {
            get
            {
                monsters ??= Json.ToObject<ObservableCollection<Monster>>(Read(MonstersJson));
                return monsters;
            }
            set => monsters = value;
        }
        private ObservableCollection<Monster>? monsters;

        [IntegrityAware]
        public ObservableCollection<KeySource>? Stars
        {
            get
            {
                stars ??= Json.ToObject<ObservableCollection<KeySource>>(Read(StarsJson));
                return stars;
            }
            set => stars = value;
        }
        private ObservableCollection<KeySource>? stars;

        [IntegrityAware]
        public ObservableCollection<Weapon>? Weapons
        {
            get
            {
                weapons ??= Json.ToObject<ObservableCollection<Weapon>>(Read(WeaponsJson));
                return weapons;
            }
            set => weapons = value;
        }
        private ObservableCollection<Weapon>? weapons;

        [IntegrityAware]
        public ObservableCollection<KeySource>? WeaponTypes
        {
            get
            {
                weaponTypes ??= Json.ToObject<ObservableCollection<KeySource>>(Read(WeaponTypesJson));
                return weaponTypes;
            }
            set => weaponTypes = value;
        }
        private ObservableCollection<KeySource>? weaponTypes;

        [IntegrityAware]
        public ObservableCollection<Weekly>? WeeklyTalents
        {
            get
            {
                weeklyTalents ??= Json.ToObject<ObservableCollection<Weekly>>(Read(WeeklyTalentsJson));
                return weeklyTalents;
            }
            set => weeklyTalents = value;
        }
        private ObservableCollection<Weekly>? weeklyTalents;
        #endregion

        #region GachaEvents
        private List<SpecificBanner>? specificBanners;
        public List<SpecificBanner>? SpecificBanners
        {
            get
            {
                if (specificBanners == null)
                {
                    specificBanners = Json.ToObject<List<SpecificBanner>>(Read(GachaEventJson));
                }
                return specificBanners;
            }
            set => specificBanners = value;
        }
        #endregion

        #region Selected Bindable
        private Boss? selectedBoss;
        public Boss? SelectedBoss { get => selectedBoss; set => SetProperty(ref selectedBoss, value); }

        private KeySource? selectedCity;
        public KeySource? SelectedCity { get => selectedCity; set => SetProperty(ref selectedCity, value); }

        private Character? selectedCharacter;
        public Character? SelectedCharacter { get => selectedCharacter; set => SetProperty(ref selectedCharacter, value); }

        private Talent? selectedDailyTalent;
        public Talent? SelectedDailyTalent { get => selectedDailyTalent; set => SetProperty(ref selectedDailyTalent, value); }

        private WeaponMaterial? selectedDailyWeapon;
        public WeaponMaterial? SelectedDailyWeapon { get => selectedDailyWeapon; set => SetProperty(ref selectedDailyWeapon, value); }

        private Elite? selectedElite;
        public Elite? SelectedElite { get => selectedElite; set => SetProperty(ref selectedElite, value); }

        private Local? selectedLocal;
        public Local? SelectedLocal { get => selectedLocal; set => SetProperty(ref selectedLocal, value); }

        private Monster? selectedMonster;
        public Monster? SelectedMonster { get => selectedMonster; set => SetProperty(ref selectedMonster, value); }

        private Weapon? selectedWeapon;
        public Weapon? SelectedWeapon { get => selectedWeapon; set => SetProperty(ref selectedWeapon, value); }

        private Weekly? selectedWeeklyTalent;
        public Weekly? SelectedWeeklyTalent { get => selectedWeeklyTalent; set => SetProperty(ref selectedWeeklyTalent, value); }
        #endregion

        #region Command
        private IRelayCommand characterInitializeCommand;
        private IRelayCommand filterCharacterCommand;
        private IRelayCommand gachaSplashCommand;
        private IRelayCommand weaponInitializeCommand;

        public IRelayCommand CharacterInitializeCommand
        {
            get => characterInitializeCommand;
            [MemberNotNull(nameof(characterInitializeCommand))]
            set => SetProperty(ref characterInitializeCommand, value);
        }
        public IRelayCommand WeaponInitializeCommand
        {
            get => weaponInitializeCommand;
            set => SetProperty(ref weaponInitializeCommand, value);
        }
        public IRelayCommand FilterCharacterCommand
        {
            get => filterCharacterCommand;
            [MemberNotNull(nameof(filterCharacterCommand))]
            set => SetProperty(ref filterCharacterCommand, value);
        }
        public IRelayCommand GachaSplashCommand
        {
            get => gachaSplashCommand;
            [MemberNotNull(nameof(gachaSplashCommand))]
            set => SetProperty(ref gachaSplashCommand, value);
        }
        #endregion

        public MetadataViewModel()
        {
            CharacterInitializeCommand = new RelayCommand(() => { SelectedCharacter ??= Characters?.First(); });
            weaponInitializeCommand = new RelayCommand(() => { SelectedWeapon ??= Weapons?.First(); });
            FilterCharacterCommand = new RelayCommand(FilterCharacterAndWeapon);
            GachaSplashCommand = new RelayCommand(() =>
            {
                new CharacterGachaSplashWindow()
                {
                    Source = SelectedCharacter?.GachaSplash,
                    Owner = App.Current.MainWindow
                }.ShowDialog();
            });
        }

        #region Helper
        /// <summary>
        /// 根据名称查找合适的url
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string? FindSourceByName(string? name)
        {
            if (name is null)
            {
                return null;
            }
            Primitive? p = (Primitive?)characters?.FirstOrDefault(c => c.Name == name) ??
                weapons?.FirstOrDefault(w => w.Name == name) ?? null;
            return p?.Source;
        }

        /// <summary>
        /// 对角色与武器的视图应用当前筛选条件
        /// </summary>
        public void FilterCharacterAndWeapon()
        {
            ICollectionView? cview = CollectionViewSource.GetDefaultView(Characters);
            cview.Filter = c =>
            {
                Character? ch = c as Character;
                return WeaponTypes!.Any(w => w.IsSelected && ch?.Weapon == w.Source) && Elements!.Any(e => e.IsSelected && ch?.Element == e.Source);
            };
            cview.MoveCurrentToFirst();
            cview.Refresh();

            ICollectionView? wview = CollectionViewSource.GetDefaultView(Weapons);
            wview.Filter = w =>
            {
                Weapon? we = w as Weapon;
                return WeaponTypes!.Any(w => w.IsSelected && we?.Type == w.Source);
            };
            wview.MoveCurrentToFirst();
            wview.Refresh();
        }

        #endregion

        #region read write
        private string Read(string fileName)
        {
            string path = PathContext.Locate(folderPath, fileName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                this.Log($"{fileName} loaded.");
                return json;
            }
            return string.Empty;
        }
        #endregion
    }
}