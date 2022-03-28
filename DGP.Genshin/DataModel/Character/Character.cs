using DGP.Genshin.DataModel.Material;
using System.Collections.Generic;

namespace DGP.Genshin.DataModel.Character
{
    /// <summary>
    /// 角色
    /// </summary>
    public class Character : Primitive
    {
        public string? Weapon { get; set; }
        public string? Element { get; set; }
        public string? Profile { get; set; }
        public string? GachaCard { get; set; }
        public string? GachaSplash { get; set; }
        public string? City { get; set; }
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
