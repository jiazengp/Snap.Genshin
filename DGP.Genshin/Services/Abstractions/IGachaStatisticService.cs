using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Services.GachaStatistics;
using DGP.Genshin.Services.GachaStatistics.Statistics;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface IGachaStatisticService
    {
        Task ExportDataToExcelAsync(string path, string uid);
        Task ExportDataToJsonAsync(string path, string uid);
        Task<GachaLogWorker?> GetGachaLogWorkerAsync(GachaLogUrlMode mode);
        Task<Statistic> GetStatisticAsync(string uid);
        Task ImportFromUIGFJAsync(string path);
        Task ImportFromUIGFWAsync(string path);
        Task<(bool isOk, string? uid)> RefreshAsync(GachaLogUrlMode mode, Action<FetchProgress> progressCallback, bool full = false);
    }
}