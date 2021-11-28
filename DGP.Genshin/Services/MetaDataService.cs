using DGP.Genshin.Common.Data.Behavior;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Common.Extensions.System.Collections.Generic;
using DGP.Genshin.Controls.Infrastructures.CachedImage;
using DGP.Genshin.DataModel;
using DGP.Genshin.DataModel.Characters;
using DGP.Genshin.DataModel.Materials.GemStones;
using DGP.Genshin.DataModel.Materials.Locals;
using DGP.Genshin.DataModel.Materials.Monsters;
using DGP.Genshin.DataModel.Materials.Talents;
using DGP.Genshin.DataModel.Materials.Weeklys;
using DGP.Genshin.DataModel.Weapons;
using DGP.Genshin.Services.GachaStatistics.Statistics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using MaterialWeapon = DGP.Genshin.DataModel.Materials.Weapons.Weapon;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// 元数据服务
    /// </summary>
    public class MetadataService : Observable
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
        public ObservableCollection<Boss>? Bosses
        {
            get
            {
                if (bosses == null)
                {
                    bosses = Json.ToObject<ObservableCollection<Boss>>(Read(BossesJson));
                }
                return bosses;
            }
            set => bosses = value;
        }
        private ObservableCollection<Boss>? bosses;

        public ObservableCollection<Character>? Characters
        {
            get
            {
                if (characters == null)
                {
                    characters = Json.ToObject<ObservableCollection<Character>>(Read(CharactersJson));
                }
                return characters;
            }
            set => characters = value;
        }
        private ObservableCollection<Character>? characters;

        public ObservableCollection<KeySource>? Cities
        {
            get
            {
                if (cities == null)
                {
                    cities = Json.ToObject<ObservableCollection<KeySource>>(Read(CitiesJson));
                }
                return cities;
            }
            set => cities = value;
        }
        private ObservableCollection<KeySource>? cities;

        public ObservableCollection<Talent>? DailyTalents
        {
            get
            {
                if (dailyTalents == null)
                {
                    dailyTalents = Json.ToObject<ObservableCollection<Talent>>(Read(DailyTalentsJson));
                }
                return dailyTalents;
            }
            set => dailyTalents = value;
        }
        private ObservableCollection<Talent>? dailyTalents;

        public ObservableCollection<MaterialWeapon>? DailyWeapons
        {
            get
            {
                if (dailyWeapons == null)
                {
                    dailyWeapons = Json.ToObject<ObservableCollection<DataModel.Materials.Weapons.Weapon>>(Read(DailyWeaponsJson));
                }
                return dailyWeapons;
            }
            set => dailyWeapons = value;
        }
        private ObservableCollection<MaterialWeapon>? dailyWeapons;

        public ObservableCollection<KeySource>? Elements
        {
            get
            {
                if (elements == null)
                {
                    elements = Json.ToObject<ObservableCollection<KeySource>>(Read(ElementsJson));
                }
                return elements;
            }
            set => elements = value;
        }
        private ObservableCollection<KeySource>? elements;

        public ObservableCollection<Elite>? Elites
        {
            get
            {
                if (elites == null)
                {
                    elites = Json.ToObject<ObservableCollection<Elite>>(Read(ElitesJson));
                }
                return elites;
            }
            set => elites = value;
        }
        private ObservableCollection<Elite>? elites;

        public ObservableCollection<GemStone>? GemStones
        {
            get
            {
                if (gemstones == null)
                {
                    gemstones = Json.ToObject<ObservableCollection<GemStone>>(Read(GemStonesJson));
                }
                return gemstones;
            }
            set => gemstones = value;
        }
        private ObservableCollection<GemStone>? gemstones;

        public ObservableCollection<Local>? Locals
        {
            get
            {
                if (locals == null)
                {
                    locals = Json.ToObject<ObservableCollection<Local>>(Read(LocalsJson));
                }
                return locals;
            }
            set => locals = value;
        }
        private ObservableCollection<Local>? locals;

        public ObservableCollection<Monster>? Monsters
        {
            get
            {
                if (monsters == null)
                {
                    monsters = Json.ToObject<ObservableCollection<Monster>>(Read(MonstersJson));
                }
                return monsters;
            }
            set => monsters = value;
        }
        private ObservableCollection<Monster>? monsters;

        public ObservableCollection<KeySource>? Stars
        {
            get
            {
                if (stars == null)
                {
                    stars = Json.ToObject<ObservableCollection<KeySource>>(Read(StarsJson));
                }
                return stars;
            }
            set => stars = value;
        }
        private ObservableCollection<KeySource>? stars;

        public ObservableCollection<Weapon>? Weapons
        {
            get
            {
                if (weapons == null)
                {
                    weapons = Json.ToObject<ObservableCollection<Weapon>>(Read(WeaponsJson));
                }
                return weapons;
            }
            set => weapons = value;
        }
        private ObservableCollection<Weapon>? weapons;

        public ObservableCollection<KeySource>? WeaponTypes
        {
            get
            {
                if (weaponTypes == null)
                {
                    weaponTypes = Json.ToObject<ObservableCollection<KeySource>>(Read(WeaponTypesJson));
                }
                return weaponTypes;
            }
            set => weaponTypes = value;
        }
        private ObservableCollection<KeySource>? weaponTypes;

        public ObservableCollection<Weekly>? WeeklyTalents
        {
            get
            {
                if (weeklyTalents == null)
                {
                    weeklyTalents = Json.ToObject<ObservableCollection<Weekly>>(Read(WeeklyTalentsJson));
                }
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
        public Boss? SelectedBoss { get => selectedBoss; set => Set(ref selectedBoss, value); }

        private KeySource? selectedCity;
        public KeySource? SelectedCity { get => selectedCity; set => Set(ref selectedCity, value); }

        private Character? selectedCharacter;
        public Character? SelectedCharacter { get => selectedCharacter; set => Set(ref selectedCharacter, value); }

        private Talent? selectedDailyTalent;
        public Talent? SelectedDailyTalent { get => selectedDailyTalent; set => Set(ref selectedDailyTalent, value); }

        private MaterialWeapon? selectedDailyWeapon;
        public MaterialWeapon? SelectedDailyWeapon { get => selectedDailyWeapon; set => Set(ref selectedDailyWeapon, value); }

        private Elite? selectedElite;
        public Elite? SelectedElite { get => selectedElite; set => Set(ref selectedElite, value); }

        private Local? selectedLocal;
        public Local? SelectedLocal { get => selectedLocal; set => Set(ref selectedLocal, value); }

        private Monster? selectedMonster;
        public Monster? SelectedMonster { get => selectedMonster; set => Set(ref selectedMonster, value); }

        private Weapon? selectedWeapon;
        public Weapon? SelectedWeapon { get => selectedWeapon; set => Set(ref selectedWeapon, value); }

        private Weekly? selectedWeeklyTalent;
        public Weekly? SelectedWeeklyTalent { get => selectedWeeklyTalent; set => Set(ref selectedWeeklyTalent, value); }
        #endregion

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

        #region LifeCycle
        private string Read(string filename)
        {
            string path = folderPath + filename;
            if (File.Exists(path))
            {
                string json;
                using (StreamReader sr = new(File.OpenRead(path)))
                {
                    json = sr.ReadToEnd();
                }
                this.Log($"{filename} loaded.");
                return json;
            }
            return "";
        }
        private void Save(List<SpecificBanner>? collection, string filename)
        {
            string json = Json.Stringify(collection);
            using (StreamWriter sw = new(File.Create(folderPath + filename)))
            {
                sw.Write(json);
            }
            this.Log($"Save gachaevent metadata to {filename}");
        }
        private void Save<T>(ObservableCollection<T>? collection, string filename) where T : Primitive
        {
            string json = Json.Stringify(collection?.OrderByDescending(i => i.Star));
            using (StreamWriter sw = new(File.Create(folderPath + filename)))
            {
                sw.Write(json);
            }
            this.Log($"Save primitive metadata to {filename}");
        }
        private void Save(ObservableCollection<KeySource>? collection, string filename)
        {
            string json = Json.Stringify(collection);
            using (StreamWriter sw = new(File.Create(folderPath + filename)))
            {
                sw.Write(json);
            }
            this.Log($"Save metadata to {filename}");
        }
        public void Initialize()
        {
            this.Log("instantiated");
        }
        public void UnInitialize()
        {
            Save(Bosses, BossesJson);
            Save(Characters, CharactersJson);
            Save(Cities, CitiesJson);
            Save(DailyTalents, DailyTalentsJson);
            Save(DailyWeapons, DailyWeaponsJson);
            Save(Elements, ElementsJson);
            Save(Elites, ElitesJson);
            Save(GemStones, GemStonesJson);
            Save(Locals, LocalsJson);
            Save(Monsters, MonstersJson);
            Save(Stars, StarsJson);
            Save(Weapons, WeaponsJson);
            Save(WeaponTypes, WeaponTypesJson);
            Save(WeeklyTalents, WeeklyTalentsJson);

            Save(SpecificBanners, GachaEventJson);
            this.Log("uninitialized");
        }

        #region 单例
        private static volatile MetadataService? instance;
        [SuppressMessage("", "IDE0044")]
        private static object _locker = new();
        private MetadataService() { Initialize(); }
        public static MetadataService Instance
        {
            get
            {
                if (instance is null)
                {
                    lock (_locker)
                    {
                        instance ??= new();
                    }
                }
                return instance;
            }
        }
        #endregion

        #endregion

        #region Integrity

        private int currentCount;
        public int CurrentCount { get => currentCount; set => Set(ref currentCount, value); }

        private string? currentInfo;
        public string? CurrentInfo { get => currentInfo; set => Set(ref currentInfo, value); }

        private int? totalCount;
        public int? TotalCount { get => totalCount; set => Set(ref totalCount, value); }

        private double percent;
        public double Percent { get => percent; set => Set(ref percent, value); }

        private bool hasCheckCompleted;
        public bool HasCheckCompleted { get => hasCheckCompleted; set { Set(ref hasCheckCompleted, value); CompleteStateChanged?.Invoke(value); } }

        private int checkingCount;

        public event Action<bool>? CompleteStateChanged;

        public async Task CheckIntegrityAsync<T>(ObservableCollection<T>? collection, IProgress<InitializeState> progress) where T : KeySource
        {
            if (collection is null)
            {
                this.Log("初始化时遇到了空的集合");
                return;
            }
            //restrict thread count.
            await collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new InitializeState(Interlocked.Increment(ref checkingCount), t.Source?.ToFileName()));
            });
        }

        public async Task CheckCharacterIntegrityAsync(ObservableCollection<Character>? collection, IProgress<InitializeState> progress)
        {
            if (collection is null)
            {
                this.Log("初始化时遇到了空的集合");
                return;
            }
            //restrict thread count.
            Task sourceTask = collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new InitializeState(Interlocked.Increment(ref checkingCount), t.Source?.ToFileName()));
            });
            Task profileTask = collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new InitializeState(Interlocked.Increment(ref checkingCount), t.Source?.ToFileName()));
            });
            Task gachasplashTask = collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new InitializeState(Interlocked.Increment(ref checkingCount), t.Source?.ToFileName()));
            });
            await Task.WhenAll(sourceTask, profileTask, gachasplashTask);
        }

        private bool hasEverChecked;

        /// <summary>
        /// 检查基础缓存图片完整性，不完整的自动下载补全
        /// 此次启动后若进行过检查则直接跳过
        /// </summary>
        public async Task CheckAllIntegrityAsync()
        {
            if (hasEverChecked)
            {
                return;
            }
            if (Settings.SettingService.Instance.GetOrDefault(Settings.Setting.SkipCacheCheck, false))
            {
                this.Log("Integrity Check Suppressed by User Settings");
                HasCheckCompleted = true;
                return;
            }
            this.Log("Integrity Check Start");
            hasEverChecked = true;
            HasCheckCompleted = false;
            Progress<InitializeState> progress = new(i =>
            {
                CurrentCount = i.CurrentCount;
                Percent = (i.CurrentCount * 1D / TotalCount) ?? 0D;
                CurrentInfo = i.Info;
            });
            CurrentCount = 0;
            TotalCount =
                Bosses?.Count +
                Cities?.Count +
                (Characters?.Count * 3) +
                DailyTalents?.Count +
                DailyWeapons?.Count +
                Elements?.Count +
                Elites?.Count +
                GemStones?.Count +
                Locals?.Count +
                Monsters?.Count +
                Stars?.Count +
                Weapons?.Count +
                WeeklyTalents?.Count +
                WeaponTypes?.Count;

            Task[] integrityTasks =
            {
                CheckIntegrityAsync(Bosses, progress),
                CheckCharacterIntegrityAsync(Characters, progress),
                CheckIntegrityAsync(Cities, progress),
                CheckIntegrityAsync(DailyTalents, progress),
                CheckIntegrityAsync(DailyWeapons, progress),
                CheckIntegrityAsync(Elements, progress),
                CheckIntegrityAsync(Elites, progress),
                CheckIntegrityAsync(GemStones, progress),
                CheckIntegrityAsync(Locals, progress),
                CheckIntegrityAsync(Monsters, progress),
                CheckIntegrityAsync(Stars, progress),
                CheckIntegrityAsync(Weapons, progress),
                CheckIntegrityAsync(WeeklyTalents, progress),
                CheckIntegrityAsync(WeaponTypes, progress)
            };

            await Task.WhenAll(integrityTasks);

            this.Log("Integrity Check Stop");
            HasCheckCompleted = true;
        }

        public class InitializeState
        {
            public InitializeState(int count, string? info)
            {
                CurrentCount = count;
                Info = info;
            }
            public int CurrentCount { get; set; }
            public string? Info { get; set; }
        }
        #endregion
    }
}