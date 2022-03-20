using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Service.GachaStatistic;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Service.Abstraction.GachaStatistic
{
    public interface IGachaLogWorker
    {
        (int min, int max) DelayRange { get; set; }
        bool IsFetchDelayEnabled { get; set; }
        GachaDataCollection WorkingGachaData { get; set; }
        string? WorkingUid { get; }

        Task<string?> FetchGachaLogAggressivelyAsync(ConfigType type, IProgress<FetchProgress> progress);
        Task<string?> FetchGachaLogIncreaselyAsync(ConfigType type, IProgress<FetchProgress> progress);
        Task<Config?> GetCurrentGachaConfigAsync();
        int GetRandomDelay();
    }
}