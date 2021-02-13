using DGP.Genshin.Controls;
using DGP.Genshin.Data.Talent;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Data.Character
{
    public class CharacterCollection : List<Character>
    {
        public CharacterCollection(IEnumerable<Character> collection) : base(collection)
        {
        }

        public CharacterCollection OfTalentMaterial(TalentMaterial talentMaterial) => new CharacterCollection(this.Where(c => c.TalentMaterial == talentMaterial));

        public IEnumerable<CharacterIcon> Freedom =>
            this.OfTalentMaterial(TalentMaterial.Freedom)
            .Select(c => new CharacterIcon() { Character = c });
        public IEnumerable<CharacterIcon> Resistance =>
            this.OfTalentMaterial(TalentMaterial.Resistance)
            .Select(c => new CharacterIcon() { Character = c });
        public IEnumerable<CharacterIcon> Ballad =>
            this.OfTalentMaterial(TalentMaterial.Ballad)
            .Select(c => new CharacterIcon() { Character = c });
        public IEnumerable<CharacterIcon> Prosperity =>
            this.OfTalentMaterial(TalentMaterial.Prosperity)
            .Select(c => new CharacterIcon() { Character = c });
        public IEnumerable<CharacterIcon> Diligence =>
            this.OfTalentMaterial(TalentMaterial.Diligence)
            .Select(c => new CharacterIcon() { Character = c });
        public IEnumerable<CharacterIcon> Gold =>
            this.OfTalentMaterial(TalentMaterial.Gold)
            .Select(c => new CharacterIcon() { Character = c });
    }

}
