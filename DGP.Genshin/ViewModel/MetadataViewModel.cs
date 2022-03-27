using DGP.Genshin.Control.GenshinElement;
using DGP.Genshin.DataModel;
using DGP.Genshin.DataModel.Character;
using DGP.Genshin.DataModel.GachaStatistic.Banner;
using DGP.Genshin.DataModel.Material;
using DGP.Genshin.Helper;
using DGP.Genshin.Service.Abstraction.IntegrityCheck;
using Microsoft;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Data.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
            get => this.ProxyCollcetion(ref this.characters, CharactersJson);
        }
        private ObservableCollection<Character> characters = null!;

        [IntegrityAware]
        public ObservableCollection<DataModelWeapon> Weapons
        {
            get => this.ProxyCollcetion(ref this.weapons, WeaponsJson);
        }
        private ObservableCollection<DataModelWeapon> weapons = null!;

        [IntegrityAware]
        public List<Talent> DailyTalents
        {
            get => this.ProxyCollcetion(ref this.dailyTalents, DailyTalentsJson);
        }
        private List<Talent> dailyTalents = null!;

        [IntegrityAware]
        public List<WeaponMaterial> DailyWeapons
        {
            get => this.ProxyCollcetion(ref this.dailyWeapons, DailyWeaponsJson);
        }
        private List<WeaponMaterial> dailyWeapons = null!;

        public List<SpecificBanner> SpecificBanners
        {
            get => this.ProxyCollcetion(ref this.specificBanners, GachaEventJson);
        }
        private List<SpecificBanner> specificBanners = null!;
        #endregion

        #region Selected Bindable
        private Character? selectedCharacter;
        public Character? SelectedCharacter
        {
            get => this.selectedCharacter;

            set => this.SetProperty(ref this.selectedCharacter, value);
        }

        private DataModelWeapon? selectedWeapon;
        public DataModelWeapon? SelectedWeapon
        {
            get => this.selectedWeapon;

            set => this.SetProperty(ref this.selectedWeapon, value);
        }
        #endregion

        #region Command
        public ICommand CharacterInitializeCommand { get; }
        public ICommand WeaponInitializeCommand { get; }
        public ICommand GachaSplashCommand { get; }
        #endregion

        public MetadataViewModel()
        {
            if (!PathContext.FolderExists(folderPath))
            {
                Verify.FailOperation("未找到Metadata文件夹，请确认完整解压了Snap Genshin 的压缩包");
            }
            this.CharacterInitializeCommand = new RelayCommand(() => { this.SelectedCharacter ??= this.Characters?.First(); });
            this.WeaponInitializeCommand = new RelayCommand(() => { this.SelectedWeapon ??= this.Weapons?.First(); });
            this.GachaSplashCommand = new RelayCommand(() =>
            {
                new CharacterGachaSplashWindow()
                {
                    Source = this.SelectedCharacter?.GachaSplash,
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
            return (Primitive?)this.characters?.FirstOrDefault(c => c.Name == name) ??
                this.weapons?.FirstOrDefault(w => w.Name == name) ?? null;
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
            return this.FindPrimitiveByName(name)?.Source;
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
                    throw Verify.FailOperation($"初始化列表{fileName}失败");
                }
            }
        }
        #endregion
    }
}