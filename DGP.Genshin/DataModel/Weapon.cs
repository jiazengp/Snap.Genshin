namespace DGP.Genshin.DataModel
{
    /// <summary>
    /// 武器
    /// </summary>
    public class Weapon : Primitive
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// 攻击力
        /// </summary>
        public string? ATK { get; set; }

        /// <summary>
        /// 副属性
        /// </summary>
        public string? SubStat { get; set; }

        /// <summary>
        /// 副属性的值
        /// </summary>
        public string? SubStatValue { get; set; }

        /// <summary>
        /// 被动
        /// </summary>
        public string? Passive { get; set; }

        /// <summary>
        /// 被动名称
        /// </summary>
        public string? PassiveDescription { get; set; }

        /// <summary>
        /// 突破材料
        /// </summary>
        public Material.Weapon? Ascension { get; set; }

        /// <summary>
        /// 精英怪物材料
        /// </summary>
        public Material.Material? Elite { get; set; }

        /// <summary>
        /// 普通怪物材料
        /// </summary>
        public Material.Material? Monster { get; set; }

        /// <inheritdoc/>
        public override string? GetBadge()
        {
            return Type;
        }
    }
}