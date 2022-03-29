using DGP.Genshin.DataModel.Promotion;

namespace DGP.Genshin.Service.Abstraction
{
    /// <summary>
    /// 材料清单服务
    /// </summary>
    public interface IMaterialListService
    {
        /// <summary>
        /// 加载并获取清单数据
        /// </summary>
        /// <returns>清单</returns>
        MaterialList Load();

        /// <summary>
        /// 保存清单
        /// </summary>
        /// <param name="materialList">待保存的清单</param>
        void Save(MaterialList? materialList);
    }
}