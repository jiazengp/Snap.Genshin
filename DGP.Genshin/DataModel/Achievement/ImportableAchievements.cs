using System.Collections.Generic;

namespace DGP.Genshin.DataModel.Achievement
{
    /// <summary>
    /// 表示可导入的成就列表
    /// </summary>
    public class ImportableAchievements : List<ImportableAchievement>
    {

    }

    /// <summary>
    /// 可导入的成就
    /// </summary>
    public class ImportableAchievement
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 完成日期
        /// </summary>
        public DateTime Date { get; set; }
    }
}