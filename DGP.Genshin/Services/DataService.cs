using DGP.Genshin.Data;
using DGP.Genshin.Data.Characters;
using DGP.Genshin.Data.Materials.GemStones;
using DGP.Genshin.Data.Materials.Locals;
using DGP.Genshin.Data.Materials.Monsters;
using DGP.Genshin.Data.Materials.Talents;
using DGP.Genshin.Data.Materials.Weeklys;
using DGP.Genshin.Data.Weapons;
using DGP.Snap.Framework.Core.LifeCycling;
using DGP.Snap.Framework.Data.Json;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace DGP.Genshin.Services
{
    internal class DataService : DependencyObject, ILifeCycleManaged
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
                if (charactersView == null)
                {
                    charactersView = CollectionViewSource.GetDefaultView(Characters);
                }
                return charactersView;
            }
        }

        public void ApplyCharacterFilters()
        {
            CharactersView.Filter = (i) => { return true; };
        }

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
            get { return (Boss)GetValue(SelectedBossProperty); }
            set { SetValue(SelectedBossProperty, value); }
        }
        public static readonly DependencyProperty SelectedBossProperty =
            DependencyProperty.Register("SelectedBoss", typeof(Boss), typeof(DataService), new PropertyMetadata(null));

        public Character SelectedCharacter
        {
            get { return (Character)GetValue(SelectedCharacterProperty); }
            set { SetValue(SelectedCharacterProperty, value); }
        }
        public static readonly DependencyProperty SelectedCharacterProperty =
            DependencyProperty.Register("SelectedCharacter", typeof(Character), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedCity
        {
            get { return (KeySource)GetValue(SelectedCityProperty); }
            set { SetValue(SelectedCityProperty, value); }
        }
        public static readonly DependencyProperty SelectedCityProperty =
            DependencyProperty.Register("SelectedCity", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Talent SelectedDailyTalent
        {
            get { return (Talent)GetValue(SelectedDailyTalentProperty); }
            set { SetValue(SelectedDailyTalentProperty, value); }
        }
        public static readonly DependencyProperty SelectedDailyTalentProperty =
            DependencyProperty.Register("SelectedDailyTalent", typeof(Talent), typeof(DataService), new PropertyMetadata(null));

        public Data.Materials.Weapons.Weapon SelectedDailyWeapon
        {
            get { return (Data.Materials.Weapons.Weapon)GetValue(SelectedDailyWeaponProperty); }
            set { SetValue(SelectedDailyWeaponProperty, value); }
        }
        public static readonly DependencyProperty SelectedDailyWeaponProperty =
            DependencyProperty.Register("SelectedDailyWeapon", typeof(Data.Materials.Weapons.Weapon), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedElement
        {
            get { return (KeySource)GetValue(SelectedElementProperty); }
            set { SetValue(SelectedElementProperty, value); }
        }
        public static readonly DependencyProperty SelectedElementProperty =
            DependencyProperty.Register("SelectedElement", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Elite SelectedElite
        {
            get { return (Elite)GetValue(SelectedEliteProperty); }
            set { SetValue(SelectedEliteProperty, value); }
        }
        public static readonly DependencyProperty SelectedEliteProperty =
            DependencyProperty.Register("SelectedElite", typeof(Elite), typeof(DataService), new PropertyMetadata(null));

        public GemStone SelectedGemStone
        {
            get { return (GemStone)GetValue(SelectedGemStoneProperty); }
            set { SetValue(SelectedGemStoneProperty, value); }
        }
        public static readonly DependencyProperty SelectedGemStoneProperty =
            DependencyProperty.Register("SelectedGemStone", typeof(GemStone), typeof(DataService), new PropertyMetadata(null));

        public Local SelectedLocal
        {
            get { return (Local)GetValue(SelectedLocalProperty); }
            set { SetValue(SelectedLocalProperty, value); }
        }
        public static readonly DependencyProperty SelectedLocalProperty =
            DependencyProperty.Register("SelectedLocal", typeof(Local), typeof(DataService), new PropertyMetadata(null));

        public Monster SelectedMonster
        {
            get { return (Monster)GetValue(SelectedMonsterProperty); }
            set { SetValue(SelectedMonsterProperty, value); }
        }
        public static readonly DependencyProperty SelectedMonsterProperty =
            DependencyProperty.Register("SelectedMonster", typeof(Monster), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedStar
        {
            get { return (KeySource)GetValue(SelectedStarProperty); }
            set { SetValue(SelectedStarProperty, value); }
        }
        public static readonly DependencyProperty SelectedStarProperty =
            DependencyProperty.Register("SelectedStar", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Weapon SelectedWeapon
        {
            get { return (Weapon)GetValue(SelectedWeaponProperty); }
            set { SetValue(SelectedWeaponProperty, value); }
        }
        public static readonly DependencyProperty SelectedWeaponProperty =
            DependencyProperty.Register("SelectedWeapon", typeof(Weapon), typeof(DataService), new PropertyMetadata(null));

        public KeySource SelectedWeaponType
        {
            get { return (KeySource)GetValue(SelectedWeaponTypeProperty); }
            set { SetValue(SelectedWeaponTypeProperty, value); }
        }
        public static readonly DependencyProperty SelectedWeaponTypeProperty =
            DependencyProperty.Register("SelectedWeaponType", typeof(KeySource), typeof(DataService), new PropertyMetadata(null));

        public Weekly SelectedWeeklyTalent
        {
            get { return (Weekly)GetValue(SelectedWeeklyTalentProperty); }
            set { SetValue(SelectedWeeklyTalentProperty, value); }
        }
        public static readonly DependencyProperty SelectedWeeklyTalentProperty =
            DependencyProperty.Register("SelectedWeeklyTalent", typeof(Weekly), typeof(DataService), new PropertyMetadata(null));
        #endregion

        private string Read(string filename)
        {
            string path = folderPath + filename;
            FileStream fs = !File.Exists(path) ? File.Create(path) : File.OpenRead(path);
            string json;
            using (StreamReader sr = new(fs))
            {
                json = sr.ReadToEnd();
            }
            return json;
        }
        private void Save(IEnumerable dict, string filename)
        {
            string json = Json.Stringify(dict);
            using StreamWriter sw = new StreamWriter(File.Create(folderPath + filename));
            sw.Write(json);
        }
        public void Initialize()
        {
        }
        public void UnInitialize()
        {
            //ConvertXAMLToJSON();
            Save(Bosses, "bosses.json");
            Save(Characters, "characters.json");
            Save(Cities, "cities.json");
            Save(DailyTalents, "dailytalents.json");
            Save(dailyWeapons, "dailyweapons.json");
            Save(Elements, "elements.json");
            Save(Elites, "elites.json");
            Save(GemStones, "gemstones.json");
            Save(Locals, "locals.json");
            Save(Monsters, "monsers.json");
            Save(Stars, "stars.json");
            Save(Weapons, "weapons.json");
            Save(WeaponTypes, "weapontypes.json");
            Save(WeeklyTalents, "weeklytalents.json");
        }

        #region xaml to json
        private void ConvertXAMLToJSON()
        {
            Directory.CreateDirectory(folderPath);

            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Characters/Characters.xaml", "characters.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Weapons/Weapons.xaml", "weapons.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Materials/GemStones/GemStones.xaml", "gemstones.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Materials/Locals/Locals.xaml", "locals.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Materials/Monsters/Bosses.xaml", "bosses.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Materials/Monsters/Elites.xaml", "elites.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Materials/Monsters/Monsters.xaml", "monsters.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Materials/Talents/DailyTalents.xaml", "dailytalents.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Materials/Weapons/Weapons.xaml", "dailyweapons.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Materials/Weeklys/WeeklyTalents.xaml", "weeklytalents.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Materials/Monsters/Bosses.xaml", "bosses.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Cities.xaml", "cities.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Elements.xaml", "elements.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/Stars.xaml", "stars.json");
            Convert($"pack://application:,,,/DGP.Genshin.Data;component/WeaponTypes.xaml", "weapontypes.json");
        }
        private static void Convert(string dictUri, string filename)
        {
            IDictionary dict = new ResourceDictionary
            {
                Source = new Uri(dictUri)
            };
            foreach(string key in dict.Keys)
            {
                dict[key] = new KeySource { Key = key, Source = (string)dict[key] };
            }
            string json = Json.Stringify(dict.Values);
            using StreamWriter sw = new StreamWriter(File.Create(folderPath + filename));
            sw.Write(json);
        }
        #endregion
    }

}
