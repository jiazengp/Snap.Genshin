using DGP.Genshin.Controls.CachedImage;
using DGP.Genshin.Data;
using DGP.Genshin.Data.Characters;
using DGP.Genshin.Data.Materials.GemStones;
using DGP.Genshin.Data.Materials.Locals;
using DGP.Genshin.Data.Materials.Monsters;
using DGP.Genshin.Data.Materials.Talents;
using DGP.Genshin.Data.Materials.Weeklys;
using DGP.Genshin.Data.Weapons;
using DGP.Genshin.Models.MiHoYo.Gacha.Statistics;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    internal class MetaDataService : Observable
    {
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
        private static readonly string folderPath = @"Metadata\";

        #region Collections
        public ObservableCollection<Boss> Bosses
        {
            get
            {
                if (this.bosses == null)
                {
                    this.bosses = Json.ToObject<ObservableCollection<Boss>>(Read(BossesJson));
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
                    this.characters = Json.ToObject<ObservableCollection<Character>>(Read(CharactersJson));
                }
                return this.characters;
            }
            set => this.characters = value;
        }
        private ObservableCollection<Character> characters;
        #endregion
        public ObservableCollection<KeySource> Cities
        {
            get
            {
                if (this.cities == null)
                {
                    this.cities = Json.ToObject<ObservableCollection<KeySource>>(Read(CitiesJson));
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
                    this.dailyTalents = Json.ToObject<ObservableCollection<Talent>>(Read(DailyTalentsJson));
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
                    this.dailyWeapons = Json.ToObject<ObservableCollection<Data.Materials.Weapons.Weapon>>(Read(DailyWeaponsJson));
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
                    this.elements = Json.ToObject<ObservableCollection<KeySource>>(Read(ElementsJson));
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
                    this.elites = Json.ToObject<ObservableCollection<Elite>>(Read(ElitesJson));
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
                    this.gemstones = Json.ToObject<ObservableCollection<GemStone>>(Read(GemStonesJson));
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
                    this.locals = Json.ToObject<ObservableCollection<Local>>(Read(LocalsJson));
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
                    this.monsters = Json.ToObject<ObservableCollection<Monster>>(Read(MonstersJson));
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
                    this.stars = Json.ToObject<ObservableCollection<KeySource>>(Read(StarsJson));
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
                    this.weapons = Json.ToObject<ObservableCollection<Weapon>>(Read(WeaponsJson));
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
                    this.weaponTypes = Json.ToObject<ObservableCollection<KeySource>>(Read(WeaponTypesJson));
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
                    this.weeklyTalents = Json.ToObject<ObservableCollection<Weekly>>(Read(WeeklyTalentsJson));
                }
                return this.weeklyTalents;
            }
            set => this.weeklyTalents = value;
        }
        private ObservableCollection<Weekly> weeklyTalents;

        #endregion

        #region GachaEvents
        private List<SpecificBanner> specificBanners;
        public List<SpecificBanner> SpecificBanners
        {
            get
            {
                if (this.specificBanners == null)
                {
                    this.specificBanners = Json.ToObject<List<SpecificBanner>>(Read(GachaEventJson));
                }
                return this.specificBanners;
            }
            set => this.specificBanners = value;
        }
        #endregion

        #region Selected Bindable
        private Character selectedCharacter;
        public Character SelectedCharacter { get => this.selectedCharacter; set => Set(ref this.selectedCharacter, value); }

        private Weapon selectedWeapon;
        public Weapon SelectedWeapon { get => this.selectedWeapon; set => Set(ref this.selectedWeapon, value); }
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
        private void Save(List<SpecificBanner> collection, string filename)
        {
            string json = Json.Stringify(collection);
            using (StreamWriter sw = new StreamWriter(File.Create(folderPath + filename)))
            {
                sw.Write(json);
            }
            this.Log($"Save composed metadata to {filename}");
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
            Save(this.Bosses, BossesJson);
            Save(this.Characters, CharactersJson);
            Save(this.Cities, CitiesJson);
            Save(this.DailyTalents, DailyTalentsJson);
            Save(this.DailyWeapons, DailyWeaponsJson);
            Save(this.Elements, ElementsJson);
            Save(this.Elites, ElitesJson);
            Save(this.GemStones, GemStonesJson);
            Save(this.Locals, LocalsJson);
            Save(this.Monsters, MonstersJson);
            Save(this.Stars, StarsJson);
            Save(this.Weapons, WeaponsJson);
            Save(this.WeaponTypes, WeaponTypesJson);
            Save(this.WeeklyTalents, WeeklyTalentsJson);

            Save(this.SpecificBanners, GachaEventJson);
        }
        #region 单例
        private static MetaDataService instance;
        private static readonly object _lock = new();
        private MetaDataService()
        {
            Initialize();
        }
        public static MetaDataService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new MetaDataService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

        #endregion

        #region CheckIntegrity
        private int currentCount;
        public int CurrentCount { get => this.currentCount; set => Set(ref this.currentCount, value); }

        private int totalCount;
        public int TotalCount { get => this.totalCount; set => Set(ref this.totalCount, value); }

        private double percent;
        public double Percent { get => this.percent; set => Set(ref this.percent, value); }

        private bool hasCheckCompleted;
        public bool HasCheckCompleted
        {
            get => this.hasCheckCompleted; set
            {
                Set(ref this.hasCheckCompleted, value);
                CompleteStateChanged?.Invoke(value);
            }
        }


        public event Action<bool> CompleteStateChanged;

        private static int checkingCount = 0;

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
                progress.Report(++checkingCount);
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