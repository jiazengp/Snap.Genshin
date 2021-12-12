using DGP.Genshin.DataModel.Weapons;
using DGP.Genshin.Services;
using DGP.Genshin.YoungMoeAPI.Collocation;
using System.Linq;

namespace DGP.Genshin.DataModel.YoungMoe2
{
    public class CollocationWeapon2 : CollocationWeapon
    {
        public string? Source => Weapon.Source;
        public string? StarUrl => Weapon.Star;

        public string? Type => Weapon.Type;

        private Weapon? weapon;
        private Weapon Weapon
        {
            get
            {
                if (weapon == null)
                {
                    weapon = MetadataViewModel.Instance.Weapons?.FirstOrDefault(c => c.Name == Name) ?? new();
                }
                return weapon;
            }
        }
    }
}
