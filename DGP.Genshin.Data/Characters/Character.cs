using DGP.Genshin.Data.Materials.GemStones;
using DGP.Genshin.Data.Materials.Locals;
using DGP.Genshin.Data.Materials.Monsters;
using DGP.Genshin.Data.Materials.Talents;
using DGP.Genshin.Data.Materials.Weeklys;
using System.Windows.Media;

namespace DGP.Genshin.Data.Characters
{
    public class Character : Primitive
    {
        public string Weapon { get; set; }
        public string Element { get; set; }
        public string Profile { get; set; }
        public string GachaCard { get; set; }
        public string GachaSplash { get; set; }
        public string City { get; set; }
        public string AscensionStat { get; set; }
        public string AscensionStatValue { get; set; }
        public Talent Talent { get; set; }
        public Boss Boss { get; set; }
        public GemStone GemStone { get; set; }
        public Local Local { get; set; }
        public Monster Monster { get; set; }
        public Weekly Weekly { get; set; }
    }
}
