using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.DataModel.Character;
using DGP.Genshin.DataModel.Material;
using DGP.Genshin.Factory.Abstraction;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DataModelWeapon = DGP.Genshin.DataModel.Weapon;
using WeaponMaterial = DGP.Genshin.DataModel.Material.Weapon;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 日常材料服务
    /// </summary>
    [ViewModel(InjectAs.Singleton)]
    internal class DailyViewModel : ObservableObject2, ISupportCancellation
    {
        private const string MondstadtIcon = "https://upload-bbs.mihoyo.com/game_record/genshin/city_icon/UI_ChapterIcon_Mengde.png";
        private const string LiyueIcon = "https://upload-bbs.mihoyo.com/game_record/genshin/city_icon/UI_ChapterIcon_Liyue.png";
        private const string InazumaIcon = "https://upload-bbs.mihoyo.com/game_record/genshin/city_icon/UI_ChapterIcon_Daoqi.png";


        private readonly MetadataViewModel metadata;
        public CancellationToken CancellationToken { get; set; }

        internal record City
        {
            public City(string name, string source, IList<Indexed<Talent, Character>> characters, IList<Indexed<WeaponMaterial, DataModelWeapon>> weapons)
            {
                this.Name = name;
                this.Source = source;
                this.Characters = characters;
                this.Weapons = weapons;
            }

            public string Name { get; set; }
            public string Source { get; set; }
            public IList<Indexed<Talent, Character>> Characters { get; set; }
            public IList<Indexed<WeaponMaterial, DataModelWeapon>> Weapons { get; set; }
        }

        private IList<City>? cities;

        public IList<City>? Cities { get => this.cities; set => this.SetProperty(ref this.cities, value); }

        public ICommand OpenUICommand { get; }

        public DailyViewModel(MetadataViewModel metadataViewModel, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.metadata = metadataViewModel;

            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
        }

        private async Task OpenUIAsync()
        {
            try
            {
                await Task.Delay(500, this.CancellationToken);
                this.BuildCities();
            }
            catch (TaskCanceledException)
            {
                this.Log("Open UI cancelled");
            }
        }

        private void BuildCities()
        {
            List<Indexed<Talent, Character>> mondstadtCharacter = new()
            {
                this.IndexedFromTalentName(Talent.Freedom),
                this.IndexedFromTalentName(Talent.Resistance),
                this.IndexedFromTalentName(Talent.Ballad)
            };
            List<Indexed<WeaponMaterial, DataModelWeapon>> mondstadtWeapon = new()
            {
                this.IndexedFromMaterialName(WeaponMaterial.Decarabian),
                this.IndexedFromMaterialName(WeaponMaterial.BorealWolf),
                this.IndexedFromMaterialName(WeaponMaterial.DandelionGladiator)
            };
            City mondstadt = new("蒙德", MondstadtIcon, mondstadtCharacter, mondstadtWeapon);

            List<Indexed<Talent, Character>> liyueCharacter = new()
            {
                this.IndexedFromTalentName(Talent.Prosperity),
                this.IndexedFromTalentName(Talent.Diligence),
                this.IndexedFromTalentName(Talent.Gold)
            };
            List<Indexed<WeaponMaterial, DataModelWeapon>> liyueWeapon = new()
            {
                this.IndexedFromMaterialName(WeaponMaterial.Guyun),
                this.IndexedFromMaterialName(WeaponMaterial.MistVeiled),
                this.IndexedFromMaterialName(WeaponMaterial.Aerosiderite)
            };
            City liyue = new("璃月", LiyueIcon, liyueCharacter, liyueWeapon);

            List<Indexed<Talent, Character>> inazumaCharacter = new()
            {
                this.IndexedFromTalentName(Talent.Transience),
                this.IndexedFromTalentName(Talent.Elegance),
                this.IndexedFromTalentName(Talent.Light)
            };
            List<Indexed<WeaponMaterial, DataModelWeapon>> inazumaWeapon = new()
            {
                this.IndexedFromMaterialName(WeaponMaterial.DistantSea),
                this.IndexedFromMaterialName(WeaponMaterial.Narukami),
                this.IndexedFromMaterialName(WeaponMaterial.Mask)
            };
            City inazuma = new("稻妻", InazumaIcon, inazumaCharacter, inazumaWeapon);

            this.Cities = new List<City>()
            {
                mondstadt,
                liyue,
                inazuma
            };
        }
        private Indexed<Talent, Character> IndexedFromTalentName(string talentName)
        {
            return new(this.metadata.DailyTalents.First(t => t.Source == talentName),
                this.metadata.Characters.Where(c => c.Talent!.Source == talentName).ToList());
        }
        private Indexed<WeaponMaterial, DataModelWeapon> IndexedFromMaterialName(string materialName)
        {
            return new(this.metadata.DailyWeapons.First(t => t.Source == materialName),
                this.metadata.Weapons.Where(c => c.Ascension!.Source == materialName).ToList());
        }
    }
}
