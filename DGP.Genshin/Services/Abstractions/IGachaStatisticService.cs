using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Services.GachaStatistics;
using DGP.Genshin.Services.GachaStatistics.Statistics;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface IGachaStatisticService
    {
        Task<Statistic> GetStatisticAsync(GachaDataCollection gachaData, string uid);
        Task<(bool isOk, string? uid)> RefreshAsync(GachaDataCollection gachaData, GachaLogUrlMode mode, Action<FetchProgress> progressCallback, bool full = false);
        Task<GachaLogWorker?> GetGachaLogWorkerAsync(GachaDataCollection gachaData, GachaLogUrlMode mode);
        Task ExportDataToExcelAsync(GachaDataCollection gachaData, string uid, string path);
        Task ExportDataToJsonAsync(GachaDataCollection gachaData, string uid, string path);
        Task ImportFromUIGFWAsync(GachaDataCollection gachaData, string path);
        Task ImportFromUIGFJAsync(GachaDataCollection gachaData, string path);
        void LoadLocalGachaData(GachaDataCollection gachaData);
    }
}