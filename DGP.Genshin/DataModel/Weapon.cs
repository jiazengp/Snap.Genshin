using DGP.Genshin.DataModel.Material;

namespace DGP.Genshin.DataModel
{
    public class Weapon : Primitive
    {
        public string? Type { get; set; }
        public string? ATK { get; set; }
        public string? SubStat { get; set; }
        public string? SubStatValue { get; set; }
        public string? Passive { get; set; }
        public string? PassiveDescription { get; set; }
        public Material.Weapon? Ascension { get; set; }
        public Elite? Elite { get; set; }
        public Monster? Monster { get; set; }

        public override string? GetBadge()
        {
            return Type;
        }
    }
}
