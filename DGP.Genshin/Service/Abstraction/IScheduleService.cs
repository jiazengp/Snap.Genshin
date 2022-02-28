using System.Threading.Tasks;

namespace DGP.Genshin.Service.Abstraction
{
    /// <summary>
    /// 计划服务
    /// </summary>
    public interface IScheduleService
    {
        Task InitializeAsync();
        void UnInitialize();
    }
}
