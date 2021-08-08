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
        private static readonly string folderPath = $"{AppDomain.CurrentDomain.BaseDirectory}Metadata\\";

        #region Collections
        public ObservableCollection<Boss> Bosses
        {
            get
            {
                if (this.bosses == null)
                {
                    this.bosses = Json.ToObject<ObservableCollection<Boss>>(this.Read("bosses.json"));
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
                    this.characters = Json.ToObject<ObservableCollection<Character>>(this.Read("characters.json"));
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
                    this.filterElements = this.Elements.ToObservableSelectable(true, this.UpdateCharacterView);
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
                    this.cities = Json.ToObject<ObservableCollection<KeySource>>(this.Read("cities.json"));
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
                    this.dailyTalents = Json.ToObject<ObservableCollection<Talent>>(this.Read("dailytalents.json"));
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
                    this.dailyWeapons = Json.ToObject<ObservableCollection<Data.Materials.Weapons.Weapon>>(this.Read("dailyweapons.json"));
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
                    this.elements = Json.ToObject<ObservableCollection<KeySource>>(this.Read("elements.json"));
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
                    this.elites = Json.ToObject<ObservableCollection<Elite>>(this.Read("elites.json"));
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
                    this.gemstones = Json.ToObject<ObservableCollection<GemStone>>(this.Read("gemstones.json"));
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
                    this.locals = Json.ToObject<ObservableCollection<Local>>(this.Read("locals.json"));
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
                    this.monsters = Json.ToObject<ObservableCollection<Monster>>(this.Read("monsters.json"));
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
                    this.stars = Json.ToObject<ObservableCollection<KeySource>>(this.Read("stars.json"));
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
                    this.weapons = Json.ToObject<ObservableCollection<Weapon>>(this.Read("weapons.json"));
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
                    this.weaponTypes = Json.ToObject<ObservableCollection<KeySource>>(this.Read("weapontypes.json"));
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
                    this.weeklyTalents = Json.ToObject<ObservableCollection<Weekly>>(this.Read("weeklytalents.json"));
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
            get => (Boss)this.GetValue(SelectedBossProperty);
            set => this.SetValue(SelectedBossProperty, value);
        }
        public static readonly DependencyProperty SelectedBossProperty =
            DependencyProperty.Register("SelectedBoss", typeof(Boss), typeof(DataService), new PropertyMetadata(null));

        public Character SelectedCharacter
        {
            get => (Character)this.GetValue(SelectedCharacterProperty);
            set => this.SetValue(SelectedCharacterProperty, value);
        }
        public static readonly DependencyProperty SelectedCharacterProperty =
            DependencyProperty.Register("SelectedCharacter", typeof(Character), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedCity
        {
            get => (KeySource)this.GetValue(SelectedCityProperty);
            set => this.SetValue(SelectedCityProperty, value);
        }
        public static readonly DependencyProperty SelectedCityProperty =
            DependencyProperty.Register("SelectedCity", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Talent SelectedDailyTalent
        {
            get => (Talent)this.GetValue(SelectedDailyTalentProperty);
            set => this.SetValue(SelectedDailyTalentProperty, value);
        }
        public static readonly DependencyProperty SelectedDailyTalentProperty =
            DependencyProperty.Register("SelectedDailyTalent", typeof(Talent), typeof(DataService), new PropertyMetadata(null));

        public Data.Materials.Weapons.Weapon SelectedDailyWeapon
        {
            get => (Data.Materials.Weapons.Weapon)this.GetValue(SelectedDailyWeaponProperty);
            set => this.SetValue(SelectedDailyWeaponProperty, value);
        }
        public static readonly DependencyProperty SelectedDailyWeaponProperty =
            DependencyProperty.Register("SelectedDailyWeapon", typeof(Data.Materials.Weapons.Weapon), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedElement
        {
            get => (KeySource)this.GetValue(SelectedElementProperty);
            set => this.SetValue(SelectedElementProperty, value);
        }
        public static readonly DependencyProperty SelectedElementProperty =
            DependencyProperty.Register("SelectedElement", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Elite SelectedElite
        {
            get => (Elite)this.GetValue(SelectedEliteProperty);
            set => this.SetValue(SelectedEliteProperty, value);
        }
        public static readonly DependencyProperty SelectedEliteProperty =
            DependencyProperty.Register("SelectedElite", typeof(Elite), typeof(DataService), new PropertyMetadata(null));

        public GemStone SelectedGemStone
        {
            get => (GemStone)this.GetValue(SelectedGemStoneProperty);
            set => this.SetValue(SelectedGemStoneProperty, value);
        }
        public static readonly DependencyProperty SelectedGemStoneProperty =
            DependencyProperty.Register("SelectedGemStone", typeof(GemStone), typeof(DataService), new PropertyMetadata(null));

        public Local SelectedLocal
        {
            get => (Local)this.GetValue(SelectedLocalProperty);
            set => this.SetValue(SelectedLocalProperty, value);
        }
        public static readonly DependencyProperty SelectedLocalProperty =
            DependencyProperty.Register("SelectedLocal", typeof(Local), typeof(DataService), new PropertyMetadata(null));

        public Monster SelectedMonster
        {
            get => (Monster)this.GetValue(SelectedMonsterProperty);
            set => this.SetValue(SelectedMonsterProperty, value);
        }
        public static readonly DependencyProperty SelectedMonsterProperty =
            DependencyProperty.Register("SelectedMonster", typeof(Monster), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedStar
        {
            get => (KeySource)this.GetValue(SelectedStarProperty);
            set => this.SetValue(SelectedStarProperty, value);
        }
        public static readonly DependencyProperty SelectedStarProperty =
            DependencyProperty.Register("SelectedStar", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Weapon SelectedWeapon
        {
            get => (Weapon)this.GetValue(SelectedWeaponProperty);
            set => this.SetValue(SelectedWeaponProperty, value);
        }
        public static readonly DependencyProperty SelectedWeaponProperty =
            DependencyProperty.Register("SelectedWeapon", typeof(Weapon), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedWeaponType
        {
            get => (KeySource)this.GetValue(SelectedWeaponTypeProperty);
            set => this.SetValue(SelectedWeaponTypeProperty, value);
        }
        public static readonly DependencyProperty SelectedWeaponTypeProperty =
            DependencyProperty.Register("SelectedWeaponType", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Weekly SelectedWeeklyTalent
        {
            get => (Weekly)this.GetValue(SelectedWeeklyTalentProperty);
            set => this.SetValue(SelectedWeeklyTalentProperty, value);
        }
        public static readonly DependencyProperty SelectedWeeklyTalentProperty =
            DependencyProperty.Register("SelectedWeeklyTalent", typeof(Weekly), typeof(DataService), new PropertyMetadata(null));
        #endregion

        #region LifeCycle
        private string Read(string filename)
        {
            string path = folderPath + filename;
            FileStream fs = !File.Exists(path) ? File.Create(path) : File.OpenRead(path);
            string json;
            using (StreamReader sr = new(fs))
            {
                json = sr.ReadToEnd();
            }
            this.Log($"{filename} loaded.");
            return json;
        }
        private void Save<T>(ObservableCollection<T> collection, string filename) where T : Primitive
        {
            string json = Json.Stringify(collection.OrderByDescending(i => i.Star));
            using StreamWriter sw = new StreamWriter(File.Create(folderPath + filename));
            sw.Write(json);
            this.Log($"Save composed metadata to {filename}");
        }
        private void Save(ObservableCollection<KeySource> collection, string filename)
        {
            string json = Json.Stringify(collection);
            using StreamWriter sw = new StreamWriter(File.Create(folderPath + filename));
            sw.Write(json);
            this.Log($"Save metadata to {filename}");
        }
        public void Initialize() => this.Log("DataService Instantiated");
        public void UnInitialize()
        {
            //ConvertXAMLToJSON();
            this.Save(this.Bosses, "bosses.json");
            this.Save(this.Characters, "characters.json");
            this.Save(this.Cities, "cities.json");
            this.Save(this.DailyTalents, "dailytalents.json");
            this.Save(this.DailyWeapons, "dailyweapons.json");
            this.Save(this.Elements, "elements.json");
            this.Save(this.Elites, "elites.json");
            this.Save(this.GemStones, "gemstones.json");
            this.Save(this.Locals, "locals.json");
            this.Save(this.Monsters, "monsters.json");
            this.Save(this.Stars, "stars.json");
            this.Save(this.Weapons, "weapons.json");
            this.Save(this.WeaponTypes, "weapontypes.json");
            this.Save(this.WeeklyTalents, "weeklytalents.json");
        }

        #region 单例
        private static DataService instance;
        private static readonly object _lock = new();
        private DataService()
        {
            this.Initialize();
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
            get => (int)this.GetValue(CurrentCountProperty);
            set => this.SetValue(CurrentCountProperty, value);
        }
        public static readonly DependencyProperty CurrentCountProperty =
            DependencyProperty.Register("CurrentCount", typeof(int), typeof(DataService), new PropertyMetadata(0));

        public int TotalCount
        {
            get => (int)this.GetValue(TotalCountProperty);
            set => this.SetValue(TotalCountProperty, value);
        }
        public static readonly DependencyProperty TotalCountProperty =
            DependencyProperty.Register("TotalCount", typeof(int), typeof(DataService), new PropertyMetadata(0));

        public double Percent
        {
            get => (double)this.GetValue(PercentProperty);
            set => this.SetValue(PercentProperty, value);
        }
        public static readonly DependencyProperty PercentProperty =
            DependencyProperty.Register("Percent", typeof(double), typeof(DataService), new PropertyMetadata(0.0));

        public bool HasCheckCompleted
        {
            get => (bool)this.GetValue(HasCheckCompletedProperty);
            set => this.SetValue(HasCheckCompletedProperty, value);
        }
        public static readonly DependencyProperty HasCheckCompletedProperty =
            DependencyProperty.Register("HasCheckCompleted", typeof(bool), typeof(DataService), new PropertyMetadata(false, OnCompleteStateChanged));

        private static void OnCompleteStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => CompleteStateChanged?.Invoke((bool)e.NewValue);

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
                catch { }
                progress.Report(++currentCount);
            }
        }

        /// <summary>
        /// 检查缓存图片完整性，不完整的自动下载补全
        /// </summary>
        public async Task CheckAllIntegrityAsync()
        {
            this.HasCheckCompleted = false;
            this.CurrentCount = 0;
            this.TotalCount = this.Bosses.Count + this.Characters.Count + this.Cities.Count +
                           this.DailyTalents.Count + this.DailyWeapons.Count + this.Elements.Count +
                           this.Elites.Count + this.GemStones.Count + this.Locals.Count +
                           this.Monsters.Count + this.Stars.Count + this.Weapons.Count +
                           this.WeaponTypes.Count + this.WeeklyTalents.Count;
            Progress<int> progress = new Progress<int>(i => { this.CurrentCount = i; this.Percent = i * 1.0 / this.TotalCount; });

            await this.CheckIntegrityAsync(this.Bosses, progress);
            await this.CheckIntegrityAsync(this.Characters, progress);
            await this.CheckIntegrityAsync(this.Cities, progress);
            await this.CheckIntegrityAsync(this.DailyTalents, progress);
            await this.CheckIntegrityAsync(this.DailyWeapons, progress);
            await this.CheckIntegrityAsync(this.Elements, progress);
            await this.CheckIntegrityAsync(this.Elites, progress);
            await this.CheckIntegrityAsync(this.GemStones, progress);
            await this.CheckIntegrityAsync(this.Locals, progress);
            await this.CheckIntegrityAsync(this.Monsters, progress);
            await this.CheckIntegrityAsync(this.Stars, progress);
            await this.CheckIntegrityAsync(this.Weapons, progress);
            await this.CheckIntegrityAsync(this.WeaponTypes, progress);
            await this.CheckIntegrityAsync(this.WeeklyTalents, progress);

            this.HasCheckCompleted = true;
        }
        #endregion
    }
}
