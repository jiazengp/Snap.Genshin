using DGP.Genshin.DataModel.Weapons;
using DGP.Genshin.Services;
using Newtonsoft.Json;
using System.Linq;

namespace DGP.Genshin.Models.YoungMoe.Collocation
{
    public class CollocationWeapon
    {
        [JsonProperty("name")] public string? Name { get; set; }
        [JsonProperty("src")] public string? Src { get; set; }
        [JsonProperty("rate")] public double Rate { get; set; }

        public string? Source => Weapon?.Source;
        public string? StarUrl => Weapon?.Star;

        public string? Type => Weapon.Type;

        private Weapon? weapon;
        private Weapon Weapon
        {
            get
            {
                if (weapon == null)
                {
                    weapon = MetaDataService.Instance.Weapons.FirstOrDefault(c => c.Name == Name);
                }
                return weapon ?? new Weapon();
            }

        }
    }

}
