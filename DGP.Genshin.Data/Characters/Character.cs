using DGP.Genshin.Data.Materials.GemStones;
using DGP.Genshin.Data.Materials.Locals;
using DGP.Genshin.Data.Materials.Monsters;
using DGP.Genshin.Data.Materials.Talents;
using DGP.Genshin.Data.Materials.Weeklys;

namespace DGP.Genshin.Data.Characters
{
    public class Character : Primitive
    {
        public string Weapon { get; set; }
        public string Element { get; set; }
        public Talent Talent { get; set; }
        public Boss Boss { get; set; }
        public GemStone GemStone { get; set; }
        public Local Local { get; set; }
        public Monster Monster { get; set; }
        public Weekly Weekly { get; set; }

        public Character Instance => this;
    }
}
