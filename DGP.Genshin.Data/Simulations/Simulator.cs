namespace DGP.Genshin.Data.Simulations
{
    public class Simulator
    {
        public SimulationInput Input { get; set; }

        public double GetLevelSupressDEFRate()
        {
            return (this.Input.CharacterLevel + 100) * 1.0
                / (this.Input.CharacterLevel + this.Input.MonsterLevel + 200)
                * (1 - this.Input.DEFReduction);
        }
        private double GetFianlDMGBonus() =>
            1 + this.Input.ElementDMGBonus + this.Input.WeaponDMGBonus + this.Input.ReliquaryDMGBonus;
        private double GetFinalResistance()
        {
            double fianlResistance;

            if (this.Input.ResistanceReduction > 0)
            {
                if (this.Input.ResistanceReduction > this.Input.MonsterResistance)
                {
                    fianlResistance = -(this.Input.ResistanceReduction - this.Input.MonsterResistance) / 2;
                }
                else
                {
                    fianlResistance = this.Input.ResistanceReduction - this.Input.MonsterResistance;
                }
            }
            else
            {
                fianlResistance = this.Input.MonsterResistance;
            }

            return fianlResistance;
        }
        private double GetFianlReactionBonus()
        {
            if (this.Input.ElementReactionRate == 1)
            {
                return 0d;
            }
            else
            {
                double mastery = this.Input.ELementMastery;
                //元素精通转反应增幅
                return (-0.0002 * (mastery * mastery)) + (0.4527 * mastery) + 1.0058;
            }
        }

        public double EvaluateDamageNoCrit()
        {
            return this.Input.ATK
                * this.Input.SkillRate
                * this.GetFianlDMGBonus()
                * (1 - this.GetFinalResistance())
                * this.GetLevelSupressDEFRate()
                * this.Input.ElementReactionRate
                * (1 + this.GetFianlReactionBonus());
        }
        public double EvaluateDamageCrit()
        {
            return this.Input.ATK
                * this.Input.SkillRate
                * (1 + this.Input.CritDMG)
                * this.GetFianlDMGBonus()
                * (1 - this.GetFinalResistance())
                * this.GetLevelSupressDEFRate()
                * this.Input.ElementReactionRate
                * (1 + this.GetFianlReactionBonus());
        }
        public double EvaluateDamageAverage()
        {
            return ((1 - Input.CritRate) * EvaluateDamageNoCrit() + Input.CritRate * EvaluateDamageCrit()) / 2;
        }
    }
}
