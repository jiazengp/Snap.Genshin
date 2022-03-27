using DGP.Genshin.DataModel.Material;
using System.Collections.Generic;

namespace DGP.Genshin.DataModel.Character
{
    public class Character : Primitive
    {
        public string? Weapon { get; set; }
        public string? Element { get; set; }
        public string? Profile { get; set; }
        public string? GachaCard { get; set; }
        public string? GachaSplash { get; set; }
        public string? City { get; set; }

        #region City Helper
        public bool IsMondstdat()
        {
            return this.City == @"https://genshin.honeyhunterworld.com/img/rep/monstadt_rep_70.png";
        }

        public bool IsLiyue()
        {
            return this.City == @"https://genshin.honeyhunterworld.com/img/rep/liyue_rep_70.png";
        }

        public bool IsInazuma()
        {
            return this.City == @"https://genshin.honeyhunterworld.com/img/rep/inazuma_rep_70.png";
        }
        #endregion

        public string? BaseHP { get; set; }
        public string? BaseATK { get; set; }
        public string? BaseDEF { get; set; }
        public string? AscensionStat { get; set; }
        public string? AscensionStatValue { get; set; }
        public Talent? Talent { get; set; }
        public Material.Material? Boss { get; set; }
        public Material.Material? GemStone { get; set; }
        public Material.Material? Local { get; set; }
        public Material.Material? Monster { get; set; }
        public Material.Material? Weekly { get; set; }

        public override string? GetBadge()
        {
            return this.Element;
        }

        #region Extend Portion
        public List<NameValues<CharStatValues>>? CharStat { get; set; }
        public Constellation? Constellation { get; set; }
        public TableDescribedNameSource? NormalAttack { get; set; }
        public TableDescribedNameSource? TalentE { get; set; }
        public TableDescribedNameSource? TalentQ { get; set; }
        public List<DescribedNameSource>? PassiveTalents { get; set; }
        public string? Title { get; set; }
        public string? AstrolabeName { get; set; }
        public string? Description { get; set; }
        #endregion
    }
}
