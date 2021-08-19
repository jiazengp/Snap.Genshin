using DGP.Genshin.Controls.CachedImage;
using DGP.Genshin.Data;
using DGP.Genshin.Data.Characters;
using DGP.Genshin.Data.Materials.GemStones;
using DGP.Genshin.Data.Materials.Locals;
using DGP.Genshin.Data.Materials.Monsters;
using DGP.Genshin.Data.Materials.Talents;
using DGP.Genshin.Data.Materials.Weeklys;
using DGP.Genshin.Data.Weapons;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.Extensions.System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DGP.Genshin.Services
{
    internal class DataService : DependencyObject
    {
        private static readonly string folderPath = @"Metadata\";

        #region Collections
        public ObservableCollection<Boss> Bosses
        {
            get
            {
                if (this.bosses == null)
                {
                    this.bosses = Json.ToObject<ObservableCollection<Boss>>(Read("bosses.json"));
                }
                return this.bosses;
            }
            set => this.bosses = value;
        }
        private ObservableCollection<Boss> bosses;

        #region Characters
        public ObservableCollection<Character> Characters
        {
            get
            {
                if (this.characters == null)
                {
                    this.characters = Json.ToObject<ObservableCollection<Character>>(Read("characters.json"));
                }
                return this.characters;
            }
            set => this.characters = value;
        }
        private ObservableCollection<Character> characters;

        private ICollectionView charactersView;
        public ICollectionView CharactersView
        {
            get
            {
                if (this.charactersView == null)
                {
                    this.charactersView = CollectionViewSource.GetDefaultView(this.Characters);
                }
                return this.charactersView;
            }
        }

        public async void UpdateCharacterView(bool _)
        {
            await Task.Run(() =>
            {
                //run the select func asynchronously to make the ui more responsible
                IEnumerable<string> selectedElementsFilter = this.FilterElements.ToSelected().Select(e => e.Source);
                this.Dispatcher.Invoke(() =>
                {
                    this.CharactersView.Filter = (i) =>
                    {
                        Character c = (Character)i;
                        return selectedElementsFilter.Contains(c.Element);
                    };
                });
            });
        }
        private void Set<T>(ref T storage, T value)
        {
            if (Equals(storage, value))
            {
                return;
            }
            storage = value;
        }

        private ObservableCollection<Selectable<KeySource>> filterElements;
        public ObservableCollection<Selectable<KeySource>> FilterElements
        {
            get
            {
                if (this.filterElements == null)
                {
                    this.filterElements = this.Elements.ToObservableSelectable(true, UpdateCharacterView);
                }
                return this.filterElements;
            }

            set => this.filterElements = value;
        }

        #endregion
        public ObservableCollection<KeySource> Cities
        {
            get
            {
                if (this.cities == null)
                {
                    this.cities = Json.ToObject<ObservableCollection<KeySource>>(Read("cities.json"));
                }
                return this.cities;
            }
            set => this.cities = value;
        }
        private ObservableCollection<KeySource> cities;

        public ObservableCollection<Talent> DailyTalents
        {
            get
            {
                if (this.dailyTalents == null)
                {
                    this.dailyTalents = Json.ToObject<ObservableCollection<Talent>>(Read("dailytalents.json"));
                }
                return this.dailyTalents;
            }
            set => this.dailyTalents = value;
        }
        private ObservableCollection<Talent> dailyTalents;

        public ObservableCollection<Data.Materials.Weapons.Weapon> DailyWeapons
        {
            get
            {
                if (this.dailyWeapons == null)
                {
                    this.dailyWeapons = Json.ToObject<ObservableCollection<Data.Materials.Weapons.Weapon>>(Read("dailyweapons.json"));
                }
                return this.dailyWeapons;
            }
            set => this.dailyWeapons = value;
        }
        private ObservableCollection<Data.Materials.Weapons.Weapon> dailyWeapons;

        public ObservableCollection<KeySource> Elements
        {
            get
            {
                if (this.elements == null)
                {
                    this.elements = Json.ToObject<ObservableCollection<KeySource>>(Read("elements.json"));
                }
                return this.elements;
            }
            set => this.elements = value;
        }
        private ObservableCollection<KeySource> elements;

        public ObservableCollection<Elite> Elites
        {
            get
            {
                if (this.elites == null)
                {
                    this.elites = Json.ToObject<ObservableCollection<Elite>>(Read("elites.json"));
                }
                return this.elites;
            }
            set => this.elites = value;
        }
        private ObservableCollection<Elite> elites;

        public ObservableCollection<GemStone> GemStones
        {
            get
            {
                if (this.gemstones == null)
                {
                    this.gemstones = Json.ToObject<ObservableCollection<GemStone>>(Read("gemstones.json"));
                }
                return this.gemstones;
            }
            set => this.gemstones = value;
        }
        private ObservableCollection<GemStone> gemstones;

        public ObservableCollection<Local> Locals
        {
            get
            {
                if (this.locals == null)
                {
                    this.locals = Json.ToObject<ObservableCollection<Local>>(Read("locals.json"));
                }
                return this.locals;
            }
            set => this.locals = value;
        }
        private ObservableCollection<Local> locals;

        public ObservableCollection<Monster> Monsters
        {
            get
            {
                if (this.monsters == null)
                {
                    this.monsters = Json.ToObject<ObservableCollection<Monster>>(Read("monsters.json"));
                }
                return this.monsters;
            }
            set => this.monsters = value;
        }
        private ObservableCollection<Monster> monsters;

        public ObservableCollection<KeySource> Stars
        {
            get
            {
                if (this.stars == null)
                {
                    this.stars = Json.ToObject<ObservableCollection<KeySource>>(Read("stars.json"));
                }
                return this.stars;
            }
            set => this.stars = value;
        }
        private ObservableCollection<KeySource> stars;

        public ObservableCollection<Weapon> Weapons
        {
            get
            {
                if (this.weapons == null)
                {
                    this.weapons = Json.ToObject<ObservableCollection<Weapon>>(Read("weapons.json"));
                }
                return this.weapons;
            }
            set => this.weapons = value;
        }
        private ObservableCollection<Weapon> weapons;

        public ObservableCollection<KeySource> WeaponTypes
        {
            get
            {
                if (this.weaponTypes == null)
                {
                    this.weaponTypes = Json.ToObject<ObservableCollection<KeySource>>(Read("weapontypes.json"));
                }
                return this.weaponTypes;
            }
            set => this.weaponTypes = value;
        }
        private ObservableCollection<KeySource> weaponTypes;

        public ObservableCollection<Weekly> WeeklyTalents
        {
            get
            {
                if (this.weeklyTalents == null)
                {
                    this.weeklyTalents = Json.ToObject<ObservableCollection<Weekly>>(Read("weeklytalents.json"));
                }
                return this.weeklyTalents;
            }
            set => this.weeklyTalents = value;
        }
        private ObservableCollection<Weekly> weeklyTalents;

        #endregion

        #region Bindable
        public Boss SelectedBoss
        {
            get => (Boss)GetValue(SelectedBossProperty);
            set => SetValue(SelectedBossProperty, value);
        }
        public static readonly DependencyProperty SelectedBossProperty =
            DependencyProperty.Register("SelectedBoss", typeof(Boss), typeof(DataService), new PropertyMetadata(null));

        public Character SelectedCharacter
        {
            get => (Character)GetValue(SelectedCharacterProperty);
            set => SetValue(SelectedCharacterProperty, value);
        }
        public static readonly DependencyProperty SelectedCharacterProperty =
            DependencyProperty.Register("SelectedCharacter", typeof(Character), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedCity
        {
            get => (KeySource)GetValue(SelectedCityProperty);
            set => SetValue(SelectedCityProperty, value);
        }
        public static readonly DependencyProperty SelectedCityProperty =
            DependencyProperty.Register("SelectedCity", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Talent SelectedDailyTalent
        {
            get => (Talent)GetValue(SelectedDailyTalentProperty);
            set => SetValue(SelectedDailyTalentProperty, value);
        }
        public static readonly DependencyProperty SelectedDailyTalentProperty =
            DependencyProperty.Register("SelectedDailyTalent", typeof(Talent), typeof(DataService), new PropertyMetadata(null));

        public Data.Materials.Weapons.Weapon SelectedDailyWeapon
        {
            get => (Data.Materials.Weapons.Weapon)GetValue(SelectedDailyWeaponProperty);
            set => SetValue(SelectedDailyWeaponProperty, value);
        }
        public static readonly DependencyProperty SelectedDailyWeaponProperty =
            DependencyProperty.Register("SelectedDailyWeapon", typeof(Data.Materials.Weapons.Weapon), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedElement
        {
            get => (KeySource)GetValue(SelectedElementProperty);
            set => SetValue(SelectedElementProperty, value);
        }
        public static readonly DependencyProperty SelectedElementProperty =
            DependencyProperty.Register("SelectedElement", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Elite SelectedElite
        {
            get => (Elite)GetValue(SelectedEliteProperty);
            set => SetValue(SelectedEliteProperty, value);
        }
        public static readonly DependencyProperty SelectedEliteProperty =
            DependencyProperty.Register("SelectedElite", typeof(Elite), typeof(DataService), new PropertyMetadata(null));

        public GemStone SelectedGemStone
        {
            get => (GemStone)GetValue(SelectedGemStoneProperty);
            set => SetValue(SelectedGemStoneProperty, value);
        }
        public static readonly DependencyProperty SelectedGemStoneProperty =
            DependencyProperty.Register("SelectedGemStone", typeof(GemStone), typeof(DataService), new PropertyMetadata(null));

        public Local SelectedLocal
        {
            get => (Local)GetValue(SelectedLocalProperty);
            set => SetValue(SelectedLocalProperty, value);
        }
        public static readonly DependencyProperty SelectedLocalProperty =
            DependencyProperty.Register("SelectedLocal", typeof(Local), typeof(DataService), new PropertyMetadata(null));

        public Monster SelectedMonster
        {
            get => (Monster)GetValue(SelectedMonsterProperty);
            set => SetValue(SelectedMonsterProperty, value);
        }
        public static readonly DependencyProperty SelectedMonsterProperty =
            DependencyProperty.Register("SelectedMonster", typeof(Monster), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedStar
        {
            get => (KeySource)GetValue(SelectedStarProperty);
            set => SetValue(SelectedStarProperty, value);
        }
        public static readonly DependencyProperty SelectedStarProperty =
            DependencyProperty.Register("SelectedStar", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Weapon SelectedWeapon
        {
            get => (Weapon)GetValue(SelectedWeaponProperty);
            set => SetValue(SelectedWeaponProperty, value);
        }
        public static readonly DependencyProperty SelectedWeaponProperty =
            DependencyProperty.Register("SelectedWeapon", typeof(Weapon), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedWeaponType
        {
            get => (KeySource)GetValue(SelectedWeaponTypeProperty);
            set => SetValue(SelectedWeaponTypeProperty, value);
        }
        public static readonly DependencyProperty SelectedWeaponTypeProperty =
            DependencyProperty.Register("SelectedWeaponType", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Weekly SelectedWeeklyTalent
        {
            get => (Weekly)GetValue(SelectedWeeklyTalentProperty);
            set => SetValue(SelectedWeeklyTalentProperty, value);
        }
        public static readonly DependencyProperty SelectedWeeklyTalentProperty =
            DependencyProperty.Register("SelectedWeeklyTalent", typeof(Weekly), typeof(DataService), new PropertyMetadata(null));
        #endregion

        #region LifeCycle
        private string Read(string filename)
        {
            string path = folderPath + filename;
            if (File.Exists(path))
            {
                FileStream fs = File.OpenRead(path);
                string json;
                using (StreamReader sr = new(fs))
                {
                    json = sr.ReadToEnd();
                }
                this.Log($"{filename} loaded.");
                return json;
            }
            return null;
        }
        private void Save<T>(ObservableCollection<T> collection, string filename) where T : Primitive
        {
            string json = Json.Stringify(collection.OrderByDescending(i => i.Star));
            using (StreamWriter sw = new StreamWriter(File.Create(folderPath + filename)))
            {
                sw.Write(json);
            }
            this.Log($"Save composed metadata to {filename}");
        }
        private void Save(ObservableCollection<KeySource> collection, string filename)
        {
            string json = Json.Stringify(collection);
            using (StreamWriter sw = new StreamWriter(File.Create(folderPath + filename)))
            {
                sw.Write(json);
            }
            this.Log($"Save metadata to {filename}");
        }
        public void Initialize() => this.Log("DataService Instantiated");
        public void UnInitialize()
        {
            Save(this.Bosses, "bosses.json");
            Save(this.Characters, "characters.json");
            Save(this.Cities, "cities.json");
            Save(this.DailyTalents, "dailytalents.json");
            Save(this.DailyWeapons, "dailyweapons.json");
            Save(this.Elements, "elements.json");
            Save(this.Elites, "elites.json");
            Save(this.GemStones, "gemstones.json");
            Save(this.Locals, "locals.json");
            Save(this.Monsters, "monsters.json");
            Save(this.Stars, "stars.json");
            Save(this.Weapons, "weapons.json");
            Save(this.WeaponTypes, "weapontypes.json");
            Save(this.WeeklyTalents, "weeklytalents.json");
        }
        #region 单例
        private static DataService instance;
        private static readonly object _lock = new();
        private DataService()
        {
            Initialize();
        }
        public static DataService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new DataService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

        #endregion

        #region CheckSource
        public int CurrentCount
        {
            get => (int)GetValue(CurrentCountProperty);
            set => SetValue(CurrentCountProperty, value);
        }
        public static readonly DependencyProperty CurrentCountProperty =
            DependencyProperty.Register("CurrentCount", typeof(int), typeof(DataService), new PropertyMetadata(0));

        public int TotalCount
        {
            get => (int)GetValue(TotalCountProperty);
            set => SetValue(TotalCountProperty, value);
        }
        public static readonly DependencyProperty TotalCountProperty =
            DependencyProperty.Register("TotalCount", typeof(int), typeof(DataService), new PropertyMetadata(0));

        public double Percent
        {
            get => (double)GetValue(PercentProperty);
            set => SetValue(PercentProperty, value);
        }
        public static readonly DependencyProperty PercentProperty =
            DependencyProperty.Register("Percent", typeof(double), typeof(DataService), new PropertyMetadata(0.0));

        public bool HasCheckCompleted
        {
            get => (bool)GetValue(HasCheckCompletedProperty);
            set => SetValue(HasCheckCompletedProperty, value);
        }
        public static readonly DependencyProperty HasCheckCompletedProperty =
            DependencyProperty.Register("HasCheckCompleted", typeof(bool), typeof(DataService), new PropertyMetadata(false, OnCompleteStateChanged));

        private static void OnCompleteStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            CompleteStateChanged?.Invoke((bool)e.NewValue);

        public static event Action<bool> CompleteStateChanged;

        private static int currentCount = 0;

        public async Task CheckIntegrityAsync<T>(ObservableCollection<T> collection, IProgress<int> progress) where T : KeySource
        {
            foreach (T t in collection)
            {
                try
                {
                    await FileCache.HitAsync(t.Source);
                }
                catch (Exception e)
                {
                    this.Log(e);
                }
                progress.Report(++currentCount);
            }
        }

        /// <summary>
        /// 检查基础缓存图片完整性，不完整的自动下载补全
        /// </summary>
        public async Task CheckAllIntegrityAsync()
        {
            this.HasCheckCompleted = false;
            this.CurrentCount = 0;
            this.TotalCount =
                this.Bosses.Count +
                this.Characters.Count +
                this.Cities.Count +
                this.DailyTalents.Count +
                this.DailyWeapons.Count +
                this.Elements.Count +
                this.Elites.Count +
                this.GemStones.Count +
                this.Locals.Count +
                this.Monsters.Count +
                this.Stars.Count +
                this.Weapons.Count +
                this.WeaponTypes.Count +
                this.WeeklyTalents.Count;
            Progress<int> progress = new Progress<int>(i => { this.CurrentCount = i; this.Percent = i * 1.0 / this.TotalCount; });

            await CheckIntegrityAsync(this.Bosses, progress);
            await CheckIntegrityAsync(this.Characters, progress);
            await CheckIntegrityAsync(this.Cities, progress);
            await CheckIntegrityAsync(this.DailyTalents, progress);
            await CheckIntegrityAsync(this.DailyWeapons, progress);
            await CheckIntegrityAsync(this.Elements, progress);
            await CheckIntegrityAsync(this.Elites, progress);
            await CheckIntegrityAsync(this.GemStones, progress);
            await CheckIntegrityAsync(this.Locals, progress);
            await CheckIntegrityAsync(this.Monsters, progress);
            await CheckIntegrityAsync(this.Stars, progress);
            await CheckIntegrityAsync(this.Weapons, progress);
            await CheckIntegrityAsync(this.WeaponTypes, progress);
            await CheckIntegrityAsync(this.WeeklyTalents, progress);

            this.HasCheckCompleted = true;
        }
        #endregion
    }
}