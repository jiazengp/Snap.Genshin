using DGP.Genshin.DataModel.Updating;
using System.Threading.Tasks;

namespace DGP.Genshin.Service.Abstraction.Updating
{
    /// <summary>
    /// 更新检查器接口
    /// </summary>
    public interface IUpdateChecker
    {
        public Task<UpdateInfomation?> GetUpdateInfomationAsync();
    }
}
