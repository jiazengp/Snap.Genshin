using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Data.Privacy;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.Services.GachaStatistics.Statistics;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.GachaStatistics
{

    /// <summary>
    /// 抽卡记录服务
    /// </summary>
    [Service(typeof(IGachaStatisticService), ServiceType.Transient)]
    public class GachaStatisticService : IGachaStatisticService
    {
        private readonly ISettingService settingService;
        private readonly LocalGachaLogWorker localGachaLogWorker;

        /// <summary>
        /// 核心数据
        /// </summary>
        private readonly GachaDataCollection gachaDataCollection = new();

        public GachaStatisticService(ISettingService settingService)
        {
            this.settingService = settingService;
            localGachaLogWorker = new(gachaDataCollection);
            this.Log("initialized");
        }

        public IEnumerable<PrivateString> GetUids()
        {
            bool showFullUid = settingService.GetOrDefault(Setting.ShowFullUID, false);
            return gachaDataCollection.Keys.Select(uid => new PrivateString(uid, PrivateString.DefaultMasker, showFullUid));
        }

        /// <summary>
        /// 获得当前的祈愿记录工作器
        /// </summary>
        /// <returns>如果无可用的Url则返回null</returns>
        public async Task<GachaLogWorker?> GetGachaLogWorkerAsync(GachaLogUrlMode mode)
        {
            (_, string? url) = await GachaLogUrlProvider.GetUrlAsync(mode);
            return url is null ? null : (new(url, gachaDataCollection));
        }

        public async Task<(bool isOk, string? uid)> RefreshAsync(GachaLogUrlMode mode, Action<FetchProgress> progressCallback, bool full = false)
        {
            (bool isOk, string? url) = await GachaLogUrlProvider.GetUrlAsync(mode);
            if (!isOk)
            {
                return (false, null);
            }
            if (url is null)
            {
                await new ContentDialog()
                {
                    Title = "获取祈愿记录失败",
                    Content = GetUrlFailHintByMode(mode),
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
                return (false, null);
            }
            else
            {
                (bool isSuccess, string? uid) = await RefreshInternalAsync(mode, progressCallback, full);
                if (!isSuccess)
                {
                    await new ContentDialog()
                    {
                        Title = "获取祈愿配置信息失败",
                        Content = "可能是验证密钥已过期",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
                return (isSuccess, uid);
            }
        }

        private string GetUrlFailHintByMode(GachaLogUrlMode mode)
        {
            return mode switch
            {
                GachaLogUrlMode.GameLogFile => "请在游戏中打开祈愿历史记录页面后尝试刷新",
                GachaLogUrlMode.ManualInput => "请重新输入有效的Url",
                _ => string.Empty,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns>卡池配置是否可用</returns>
        private async Task<(bool isOk, string? uid)> RefreshInternalAsync(GachaLogUrlMode mode, Action<FetchProgress> progressCallback, bool full = false)
        {
            GachaLogWorker? worker = await GetGachaLogWorkerAsync(mode);
            if (worker is null)
            {
                return (false, null);
            }
            return await FetchGachaLogsAsync(worker, progressCallback, full);
        }

        /// <summary>
        /// 获取祈愿记录
        /// </summary>
        /// <param name="worker">工作器对象</param>
        /// <param name="full">是否增量获取</param>
        /// <returns>是否获取成功</returns>
        private async Task<(bool isOk, string? uid)> FetchGachaLogsAsync(GachaLogWorker worker, Action<FetchProgress> progressCallback, bool full = false)
        {
            Config? gachaConfigTypes = await worker.GetCurrentGachaConfigAsync();
            if (gachaConfigTypes?.Types != null)
            {
                string? uid = null;
                foreach (ConfigType pool in gachaConfigTypes.Types)
                {
                    if (full)
                    {
                        uid = await worker.FetchGachaLogAggressivelyAsync(pool, progressCallback);
                    }
                    else
                    {
                        uid = await worker.FetchGachaLogIncrementAsync(pool, progressCallback);
                    }
                    if (worker.IsFetchDelayEnabled)
                    {
                        await Task.Delay(worker.GetRandomDelay());
                    }
                }
                localGachaLogWorker.SaveAllLogs();
                return (true, uid);
            }
            else
            {
                return (false, null);
            }
        }

        public async Task<Statistic> GetStatisticAsync(string uid)
        {
            return await Task.Run(() => StatisticFactory.ToStatistic(gachaDataCollection[uid], uid));
        }

        #region Im/Export
        /// <summary>
        /// 导出数据到Excel
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task ExportDataToExcelAsync(string path, string uid)
        {
            await Task.Run(() => localGachaLogWorker.SaveLocalGachaDataToExcel(uid, path));
        }

        public async Task ExportDataToJsonAsync(string path, string uid)
        {
            await Task.Run(() => localGachaLogWorker.ExportToUIGFJ(uid, path));
        }

        public async Task ImportFromUIGFWAsync(string path)
        {
            if (!await Task.Run(() => localGachaLogWorker.ImportFromUIGFW(path)))
            {
                await new ContentDialog()
                {
                    Title = "导入祈愿记录失败",
                    Content = "选择的Excel文件不是标准的可交换格式",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
        }

        public async Task ImportFromUIGFJAsync(string path)
        {
            if (!await Task.Run(() => localGachaLogWorker.ImportFromUIGFJ(path)))
            {
                await new ContentDialog()
                {
                    Title = "导入祈愿记录失败",
                    Content = "选择的Json文件不是标准的可交换格式",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
        }
        #endregion
    }
}