using CommunityToolkit.Mvvm.Input;
using DGP.Genshin.Control.GenshinElement;
using DGP.Genshin.DataModel;
using DGP.Genshin.DataModel.Character;
using DGP.Genshin.DataModel.GachaStatistic.Banner;
using DGP.Genshin.DataModel.Material;
using DGP.Genshin.Service.Abstraction.IntegrityCheck;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Data.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        private const string CharactersJson = "characters.json";
        private const string DailyTalentsJson = "dailytalents.json";
        private const string DailyWeaponsJson = "dailyweapons.json";
        private const string WeaponsJson = "weapons.json";
        private const string GachaEventJson = "gachaevents.json";

        private const string FolderPath = "Metadata\\";

        private ObservableCollection<Character> characters = null!;
        private ObservableCollection<DataModelWeapon> weapons = null!;
        private List<Talent> dailyTalents = null!;
        private List<WeaponMaterial> dailyWeapons = null!;
        private List<SpecificBanner> specificBanners = null!;

        private Character? selectedCharacter;
        private DataModelWeapon? selectedWeapon;

        /// <summary>
        /// 构造一个新的元数据视图模型
        /// </summary>
        public MetadataViewModel()
        {
            if (!PathContext.FolderExists(FolderPath))
            {
                Verify.FailOperation("未找到Metadata文件夹，请确认完整解压了Snap Genshin 的压缩包");
            }

            CharacterInitializeCommand = new RelayCommand(() => { SelectedCharacter ??= Characters?.First(); });
            WeaponInitializeCommand = new RelayCommand(() => { SelectedWeapon ??= Weapons?.First(); });
            GachaSplashCommand = new RelayCommand(() =>
            {
                new CharacterGachaSplashWindow()
                {
                    Source = SelectedCharacter?.GachaSplash,
                    Owner = App.Current.MainWindow,
                }.ShowDialog();
            });
        }

        /// <summary>
        /// 角色集合
        /// </summary>
        [IntegrityAware]
        public ObservableCollection<Character> Characters
        {
            get => ProxyCollcetion(ref characters, CharactersJson, collection =>
                {
                    return new(collection
                            .OrderByDescending(i => i.Star)
                            .ThenBy(i => i.Element)
                            .ThenBy(i => i.Name));
                });
        }

        /// <summary>
        /// 武器集合
        /// </summary>
        [IntegrityAware]
        public ObservableCollection<DataModelWeapon> Weapons
        {
            get => ProxyCollcetion(ref weapons, WeaponsJson, collection =>
            {
                return new(collection
                        .OrderByDescending(i => i.Star)
                        .ThenBy(i => i.Type)
                        .ThenBy(i => i.Name));
            });
        }

        /// <summary>
        /// 日常天赋集合
        /// </summary>
        [IntegrityAware]
        public List<Talent> DailyTalents
        {
            get => ProxyCollcetion(ref dailyTalents, DailyTalentsJson);
        }

        /// <summary>
        /// 日常武器集合
        /// </summary>
        [IntegrityAware]
        public List<WeaponMaterial> DailyWeapons
        {
            get => ProxyCollcetion(ref dailyWeapons, DailyWeaponsJson);
        }

        /// <summary>
        /// 卡池信息集合
        /// </summary>
        public List<SpecificBanner> SpecificBanners
        {
            get => ProxyCollcetion(ref specificBanners, GachaEventJson);
        }

        /// <summary>
        /// 当前选择的角色
        /// </summary>
        public Character? SelectedCharacter
        {
            get => selectedCharacter;

            set => SetProperty(ref selectedCharacter, value);
        }

        /// <summary>
        /// 当前选择的武器
        /// </summary>
        public DataModelWeapon? SelectedWeapon
        {
            get => selectedWeapon;

            set => SetProperty(ref selectedWeapon, value);
        }

        /// <summary>
        /// 角色页面初始化命令
        /// </summary>
        public ICommand CharacterInitializeCommand { get; }

        /// <summary>
        /// 武器页面初始化命令
        /// </summary>
        public ICommand WeaponInitializeCommand { get; }

        /// <summary>
        /// 卡池立绘展示命令
        /// </summary>
        public ICommand GachaSplashCommand { get; }

        /// <summary>
        /// 根据名称在角色与武器集合中查找合适的元件
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>元件</returns>
        public Primitive? FindPrimitiveByName(string? name)
        {
            if (name is null)
            {
                return null;
            }

            return (Primitive?)characters?.FirstOrDefault(c => c.Name == name)
                ?? weapons?.FirstOrDefault(w => w.Name == name)
                ?? null;
        }

        /// <summary>
        /// 根据名称查找合适的url
        /// 仅限角色与武器
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>Url</returns>
        public string? FindSourceByName(string? name)
        {
            if (name is null)
            {
                return null;
            }

            return FindPrimitiveByName(name)?.Source;
        }

        private T ProxyCollcetion<T>(ref T collection, string fileName, Func<T, T>? firstTimeProcessor = null)
            where T : new()
        {
            if (collection is not null)
            {
                return collection;
            }
            else
            {
                string path = PathContext.Locate(FolderPath, fileName);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    this.Log($"{fileName} loaded.");
                    collection ??= Json.ToObjectOrNew<T>(json);
                    if (firstTimeProcessor != null)
                    {
                        collection = firstTimeProcessor(collection);
                    }

                    return collection;
                }
                else
                {
                    throw Verify.FailOperation($"初始化列表{fileName}失败");
                }
            }
        }
    }
}