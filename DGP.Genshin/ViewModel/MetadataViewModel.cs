using DGP.Genshin.Control.GenshinElement;
using DGP.Genshin.DataModel;
using DGP.Genshin.DataModel.Character;
using DGP.Genshin.DataModel.GachaStatistic.Banner;
using DGP.Genshin.DataModel.Material;
using DGP.Genshin.Helper;
using DGP.Genshin.Service.Abstraction.IntegrityCheck;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Data.Json;
using Snap.Exception;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using DataModelWeapon = DGP.Genshin.DataModel.Weapon;
using WeaponMaterial = DGP.Genshin.DataModel.Material.Weapon;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 元数据视图模型
    /// 存有各类共享物品数据
    /// </summary>
    [ViewModel(InjectAs.Singleton)]
    internal class MetadataViewModel : ObservableObject2
    {
        #region Consts
        private const string CharactersJson = "characters.json";
        private const string DailyTalentsJson = "dailytalents.json";
        private const string DailyWeaponsJson = "dailyweapons.json";
        private const string WeaponsJson = "weapons.json";
        private const string GachaEventJson = "gachaevents.json";

        private const string folderPath = "Metadata\\";
        #endregion

        #region Collections
        [IntegrityAware]
        public ObservableCollection<Character> Characters
        {
            get => ProxyCollcetion(ref characters, CharactersJson);
        }
        private ObservableCollection<Character> characters = null!;

        [IntegrityAware]
        public ObservableCollection<DataModelWeapon> Weapons
        {
            get => ProxyCollcetion(ref weapons, WeaponsJson);
        }
        private ObservableCollection<DataModelWeapon> weapons = null!;

        [IntegrityAware]
        public List<Talent> DailyTalents
        {
            get => ProxyCollcetion(ref dailyTalents, DailyTalentsJson);
        }
        private List<Talent> dailyTalents = null!;

        [IntegrityAware]
        public List<WeaponMaterial> DailyWeapons
        {
            get => ProxyCollcetion(ref dailyWeapons, DailyWeaponsJson);
        }
        private List<WeaponMaterial> dailyWeapons = null!;

        public List<SpecificBanner> SpecificBanners
        {
            get => ProxyCollcetion(ref specificBanners, GachaEventJson);
        }
        private List<SpecificBanner> specificBanners = null!;
        #endregion

        #region Selected Bindable
        private Character? selectedCharacter;
        public Character? SelectedCharacter
        {
            get => selectedCharacter;

            set => SetProperty(ref selectedCharacter, value);
        }

        private DataModelWeapon? selectedWeapon;
        public DataModelWeapon? SelectedWeapon
        {
            get => selectedWeapon;

            set => SetProperty(ref selectedWeapon, value);
        }
        #endregion

        #region Command
        public ICommand CharacterInitializeCommand { get; }
        public ICommand WeaponInitializeCommand { get; }
        public ICommand FilterCharacterCommand { get; }
        public ICommand GachaSplashCommand { get; }
        #endregion

        public MetadataViewModel()
        {
            if (!PathContext.FolderExists(folderPath))
            {
                throw new SnapGenshinInternalException("未找到Metadata文件夹，请确认完整解压了Snap Genshin 的压缩包");
            }
            CharacterInitializeCommand = new RelayCommand(() => { SelectedCharacter ??= Characters?.First(); });
            WeaponInitializeCommand = new RelayCommand(() => { SelectedWeapon ??= Weapons?.First(); });
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
        /// 仅限角色与武器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Primitive? FindPrimitiveByName(string? name)
        {
            if (name is null)
            {
                return null;
            }
            return (Primitive?)characters?.FirstOrDefault(c => c.Name == name) ??
                weapons?.FirstOrDefault(w => w.Name == name) ?? null;
        }
        /// <summary>
        /// 根据名称查找合适的url
        /// 仅限角色与武器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string? FindSourceByName(string? name)
        {
            if (name is null)
            {
                return null;
            }
            return FindPrimitiveByName(name)?.Source;
        }


        private readonly List<KeySource> elements = new()
        {
            new("dendro", "https://genshin.honeyhunterworld.com/img/icons/element/dendro.png"),
            new("anemo", "https://genshin.honeyhunterworld.com/img/icons/element/anemo.png"),
            new("pyro", "https://genshin.honeyhunterworld.com/img/icons/element/pyro.png"),
            new("cryo", "https://genshin.honeyhunterworld.com/img/icons/element/cryo.png"),
            new("hydro", "https://genshin.honeyhunterworld.com/img/icons/element/hydro.png"),
            new("electro", "https://genshin.honeyhunterworld.com/img/icons/element/electro.png"),
            new("geo", "https://genshin.honeyhunterworld.com/img/icons/element/geo.png")
        };
        private readonly List<KeySource> weaponTypes = new()
        {
            new("Bow", "https://genshin.honeyhunterworld.com/img/skills/s_213101.png"),
            new("Sword", "https://genshin.honeyhunterworld.com/img/skills/s_33101.png"),
            new("Catalyst", "https://genshin.honeyhunterworld.com/img/skills/s_43101.png"),
            new("Claymore", "https://genshin.honeyhunterworld.com/img/skills/s_163101.png"),
            new("Polearm", "https://genshin.honeyhunterworld.com/img/skills/s_233101.png")
        };
        /// <summary>
        /// 对角色与武器的视图应用当前筛选条件
        /// </summary>
        public void FilterCharacterAndWeapon()
        {
            ICollectionView? cview = CollectionViewSource.GetDefaultView(Characters);
            cview.Filter = c =>
            {
                Character? ch = c as Character;
                return weaponTypes.Any(w => w.IsSelected && ch?.Weapon == w.Source)
                && elements.Any(e => e.IsSelected && ch?.Element == e.Source);
            };
            cview.MoveCurrentToFirst();
            cview.Refresh();

            ICollectionView? wview = CollectionViewSource.GetDefaultView(Weapons);
            wview.Filter = w =>
            {
                DataModelWeapon? we = w as DataModelWeapon;
                return weaponTypes.Any(w => w.IsSelected && we?.Type == w.Source);
            };
            wview.MoveCurrentToFirst();
            wview.Refresh();
        }
        #endregion

        #region Read
        private T ProxyCollcetion<T>(ref T collection, string fileName) where T : new()
        {
            if (collection is not null)
            {
                return collection;
            }
            else
            {
                string path = PathContext.Locate(folderPath, fileName);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    this.Log($"{fileName} loaded.");
                    collection ??= Json.ToObjectOrNew<T>(json);
                    return collection;
                }
                else
                {
                    throw new SnapGenshinInternalException($"初始化列表{fileName}失败");
                }
            }
        }
        #endregion
    }
}