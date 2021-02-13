using DGP.Genshin.Controls;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Data.Weapon
{
    public class WeaponCollection : List<Weapon>
    {
        public WeaponCollection(IEnumerable<Weapon> collection) : base(collection)
        {
        }

        public WeaponCollection OfMaterial(WeaponMaterial weaponMaterial) => new WeaponCollection(this.Where(c => c.Material == weaponMaterial));

        public IEnumerable<WeaponIcon> Decarabians =>
            this.OfMaterial(WeaponMaterial.Decarabians)
            .Select(w => new WeaponIcon() { Weapon = w });
        public IEnumerable<WeaponIcon> Guyun =>
            this.OfMaterial(WeaponMaterial.Guyun)
            .Select(w => new WeaponIcon() { Weapon = w });
        public IEnumerable<WeaponIcon> Boreal =>
            this.OfMaterial(WeaponMaterial.Boreal)
            .Select(w => new WeaponIcon() { Weapon = w });
        public IEnumerable<WeaponIcon> MistVeiled =>
            this.OfMaterial(WeaponMaterial.MistVeiled)
            .Select(w => new WeaponIcon() { Weapon = w });
        public IEnumerable<WeaponIcon> DandelionGladiator =>
            this.OfMaterial(WeaponMaterial.DandelionGladiator)
            .Select(w => new WeaponIcon() { Weapon = w });
        public IEnumerable<WeaponIcon> Aerosiderite =>
            this.OfMaterial(WeaponMaterial.Aerosiderite)
            .Select(w => new WeaponIcon() { Weapon = w });
    }

}
