using DGP.Genshin.DataModels.MiHoYo2;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    /// <summary>
    /// 玩家查询服务
    /// </summary>
    public interface IRecordService
    {
        /// <summary>
        /// 查询玩家信息
        /// </summary>
        /// <param name="uid">uid</param>
        /// <returns>查询完成的记录封装</returns>
        Task<Record> GetRecordAsync(string? uid);
    }
}