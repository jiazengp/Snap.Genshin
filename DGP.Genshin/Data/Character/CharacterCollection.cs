using DGP.Genshin.Controls;
using DGP.Genshin.Data.Talent;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Data.Character
{
    public class CharacterCollection : List<Character>
    {
        public CharacterCollection(IEnumerable<Character> collection) : base(collection) { }

        private CharacterCollection OfTalentMaterial(TalentMaterial talentMaterial) => new CharacterCollection(this.Where(c => c.TalentMaterial == talentMaterial));

        #region mengde
        public IEnumerable<CharacterIcon> Freedom =>
            this.OfTalentMaterial(TalentMaterial.Freedom)
            .AsCharacterIcon();
        public IEnumerable<CharacterIcon> Resistance =>
            this.OfTalentMaterial(TalentMaterial.Resistance)
            .AsCharacterIcon();
        public IEnumerable<CharacterIcon> Ballad =>
            this.OfTalentMaterial(TalentMaterial.Ballad)
            .AsCharacterIcon();
        #endregion

        #region liyue
        public IEnumerable<CharacterIcon> Prosperity =>
            this.OfTalentMaterial(TalentMaterial.Prosperity)
            .AsCharacterIcon();
        public IEnumerable<CharacterIcon> Diligence =>
            this.OfTalentMaterial(TalentMaterial.Diligence)
            .AsCharacterIcon();
        public IEnumerable<CharacterIcon> Gold =>
            this.OfTalentMaterial(TalentMaterial.Gold)
            .AsCharacterIcon();
        #endregion

        #region 风魔龙
        public IEnumerable<CharacterIcon> DvalinsPlume =>
            this.Where(c => c.TalentWeekly.MaterialName == "东风之翎")
            .AsCharacterIcon(true);
        public IEnumerable<CharacterIcon> DvalinsClaw =>
            this.Where(c => c.TalentWeekly.MaterialName == "东风之爪")
            .AsCharacterIcon(true);
        public IEnumerable<CharacterIcon> DvalinsSigh =>
            this.Where(c => c.TalentWeekly.MaterialName == "东风的吐息")
            .AsCharacterIcon(true);
        #endregion

        #region 北风狼
        public IEnumerable<CharacterIcon> TailofBoreas =>
            this.Where(c => c.TalentWeekly.MaterialName == "北风之尾")
            .AsCharacterIcon(true);
        public IEnumerable<CharacterIcon> RingofBoreas =>
            this.Where(c => c.TalentWeekly.MaterialName == "北风之环")
            .AsCharacterIcon(true);
        public IEnumerable<CharacterIcon> SpiritLocketofBoreas =>
            this.Where(c => c.TalentWeekly.MaterialName == "北风的魂匣")
            .AsCharacterIcon(true);
        #endregion

        #region 公子
        public IEnumerable<CharacterIcon> TuskofMonocerosCaeli =>
            this.Where(c => c.TalentWeekly.MaterialName == "吞天之鲸·只角")
            .AsCharacterIcon(true);
        public IEnumerable<CharacterIcon> ShardofFoulLegacy =>
            this.Where(c => c.TalentWeekly.MaterialName == "魔王之刃·残片")
            .AsCharacterIcon(true);
        public IEnumerable<CharacterIcon> ShadowoftheWarrior =>
            this.Where(c => c.TalentWeekly.MaterialName == "武炼之魂·孤影")
            .AsCharacterIcon(true);
        #endregion
    }
    public static class CharacterCollectionExtension
    {
        public static IEnumerable<CharacterIcon> AsCharacterIcon(this IEnumerable<Character> characters, bool isSmall = false) => characters.Select(c =>
                                                                                                                                            {
                                                                                                                                                CharacterIcon characterIcon = new CharacterIcon() { Character = c };
                                                                                                                                                if (isSmall)
                                                                                                                                                {
                                                                                                                                                    characterIcon.Width = 50;
                                                                                                                                                    characterIcon.Height = 50;
                                                                                                                                                }
                                                                                                                                                return characterIcon;
                                                                                                                                            });
    }
}
