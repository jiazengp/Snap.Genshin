using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Service.GachaStatistic;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Service.Abstraction.GachaStatistic
{
    public interface IGachaLogWorker
    {
        (int min, int max) Delay { get; set; }
        bool IsFetchDelayEnabled { get; set; }
        GachaDataCollection WorkingGachaData { get; set; }
        string? WorkingUid { get; }

        Task<string?> FetchGachaLogAggressivelyAsync(ConfigType type, Action<FetchProgress> progressCallBack);
        Task<string?> FetchGachaLogIncreaselyAsync(ConfigType type, Action<FetchProgress> progressCallBack);
        Task<Config?> GetCurrentGachaConfigAsync();
        int GetRandomDelay();
    }
}