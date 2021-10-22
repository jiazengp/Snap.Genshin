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

        public string? Source => this.Weapon?.Source;
        public string? StarUrl => this.Weapon?.Star;

        public string? Type => this.Weapon.Type;

        private Weapon? weapon;
        private Weapon Weapon
        {
            get
            {
                if (this.weapon == null)
                {
                    this.weapon = MetaDataService.Instance.Weapons.FirstOrDefault(c => c.Name == this.Name);
                }
                return this.weapon ?? new Weapon();
            }

        }
    }

}
