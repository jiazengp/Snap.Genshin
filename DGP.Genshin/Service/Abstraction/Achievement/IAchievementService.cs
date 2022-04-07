using DGP.Genshin.DataModel.Achievement;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DGP.Genshin.Service.Abstraction.Achievement
{
    /// <summary>
    /// 成就服务
    /// </summary>
    public interface IAchievementService
    {
        /// <summary>
        /// 读取已完成的项目数据
        /// </summary>
        /// <returns>已完成的项目列表</returns>
        List<IdTime> GetCompletedItems();

        /// <summary>
        /// 保存完成的项目
        /// </summary>
        /// <param name="achievements">成就列表</param>
        void SaveCompletedItems(ObservableCollection<DataModel.Achievement.Achievement> achievements);

        /// <summary>
        /// 尝试以指定的源与指定的路径获取导入数据
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="fileName">文件路径</param>
        /// <returns>可导入的成就数据</returns>
        IEnumerable<IdTime>? TryGetImportData(ImportAchievementSource source, string fileName);
    }
}