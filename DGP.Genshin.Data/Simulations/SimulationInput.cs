using System;

namespace DGP.Genshin.Data.Simulations
{
    [Obsolete]
    public class SimulationInput
    {
        public double ATK { get; set; }
        public double CritRate { get; set; }
        public double CritDMG { get; set; }
        public double SkillRate { get; set; }
        public int CharacterLevel { get; set; }
        public int MonsterLevel { get; set; }
        public double ElementDMGBonus { get; set; }
        public double WeaponDMGBonus { get; set; }
        public double ReliquaryDMGBonus { get; set; }
        public double ResistanceReduction { get; set; }
        public double MonsterResistance { get; set; }
        public double DEFReduction { get; set; }
        public double ElementReactionRate { get; set; }
        public double ELementMastery { get; set; }
    }
}
