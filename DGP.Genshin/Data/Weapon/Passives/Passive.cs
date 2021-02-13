using DGP.Genshin.Simulation.Calculation;
using DGP.Snap.Framework.Data.Behavior;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DGP.Genshin.Data.Weapon.Passives
{
    public abstract class Passive : Observable
    {
        public string Description { get; set; }
        public string DescriptionAll
        {
            get
            {
                string d = this.Description;
                if (this.Values != null)
                {
                    string v = System.String.Join("/", this.Values.Select(i => i * 100 + "%"));
                    d = d.Replace("*value*", "[" + v + "]");
                }
                if (this.Times != null)
                {
                    string t = System.String.Join("/", this.Times);
                    d = d.Replace("*time*", "[" + t + "]");
                }
                return d;
            }
        }
        public string DescriptionByRefineLevel
        {
            get
            {
                string d = this.Description;
                if (this.Values != null)
                {
                    //string v = string.Join("/", Values.Select(i => i * 100 + "%"));
                    string v = this.Values[this.refineLevel - 1] * 100 + "%";
                    d = d.Replace("*value*", " " + v + " ");
                }
                if (this.Times != null)
                {
                    string t = this.Times[this.refineLevel - 1].ToString();
                    d = d.Replace("*time*", " " + t + " ");
                }
                return d;
            }
        }
        public DoubleCollection Values { get; set; }
        protected double CurrentValue => this.Values[this.RefineLevel - 1];
        public DoubleCollection Times { get; set; }

        private int refineLevel = 1;
        public int RefineLevel
        {
            get => this.refineLevel; set
            {
                this.Set(ref this.refineLevel, value);
                this.OnPropertyChanged("DescriptionByRefineLevel");
            }
        }
        //for triggerable
        public bool IsTriggered { get; set; } = true;
        //for stackable
        public int MaxStack { get; set; } = 1;
        public int CurrentStack { get; set; } = 1;
        public Int32Collection Stacks
        {
            get
            {
                Int32Collection stacks = new Int32Collection();
                for (int i = 1; i <= this.MaxStack; i++)
                {
                    stacks.Add(i);
                }

                return stacks;
            }
        }
        //for conditional
        public double Rate { get; set; } = 1;
        public string ConditionText { get; set; }
        public Visibility ConditionVisibility { get; set; } = Visibility.Collapsed;
        public bool IsSatisfied { get; set; } = true;

        public abstract void Apply(Calculator c);
    }
    public class NormalPassive : Passive
    {
        public override void Apply(Calculator c)
        {
        }
    }
    public class HPPercentPassive : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered)
            {
                c.HP.BonusByPercent += this.CurrentValue * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class HPToATK : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered)
            {
                c.Attack.Bonus += c.HP * this.CurrentValue * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class DealedDMGBonus : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered)
            {
                c.DamageBonus += this.CurrentValue * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class NormalChargedDealedDMGBonus : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered && (c.Target == Target.NormalAttack || c.Target == Target.NormalAttack))
            {
                c.DamageBonus += this.CurrentValue * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class BaseATKPercentBonus : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered)
            {
                c.Attack.Base *= (1 + this.CurrentValue * this.CurrentStack) * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class ATKPercentBonus : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered)
            {
                c.Attack.BonusByPercent += this.CurrentValue * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class ATKBonus : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered)
            {
                c.Attack.Bonus += this.CurrentValue * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class ATKDEFPercentBonus : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered)
            {
                c.Attack.BonusByPercent += this.CurrentValue * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
                c.Defence.BonusByPercent += this.CurrentValue * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class ATKToDamage : Passive
    {
        public double Probability { get; set; } = 1;
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered)
            {
                //将一击与额外伤害合并计算
                c.ATKToDMGRate = (1 + this.CurrentValue) * this.Probability * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class CritRateBonus : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered)
            {
                c.CritRate += this.CurrentValue * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class ElementSkillDealedDMGBonus : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered && c.Target == Target.ElementSkill)
            {
                c.DamageBonus += this.CurrentValue * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class ElementSkillCritRateBonus : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered && c.Target == Target.ElementSkill)
            {
                c.CritRate += this.CurrentValue * this.CurrentStack * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
    public class AttackSpeedBonus : Passive
    {
        public override void Apply(Calculator c)
        {
            if (this.IsTriggered && c.Target == Target.ElementSkill)
            {
                c.AttackSpeed += this.CurrentValue * (this.IsSatisfied ? this.Rate : 1);
            }
        }
    }
}
