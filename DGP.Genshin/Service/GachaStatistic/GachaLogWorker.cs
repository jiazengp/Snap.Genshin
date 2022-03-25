using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.MiHoYoAPI.Request;
using DGP.Genshin.MiHoYoAPI.Response;
using DGP.Genshin.Service.Abstraction.GachaStatistic;
using Microsoft;
using Snap.Core.Logging;
using Snap.Data.Primitive;
using Snap.Net.QueryString;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Service.GachaStatistic
{
    /// <summary>
    /// 联机抽卡记录工作器
    /// </summary>
    public class GachaLogWorker : IGachaLogWorker
    {
        private readonly Random random = new();
        private readonly int batchSize;
        private readonly string gachaLogUrl;

        private (int min, int max) delay = (500, 1000);
        public int GetRandomDelay()
        {
            return DelayRange.min + random.Next(DelayRange.max - DelayRange.min, DelayRange.max);
        }

        public GachaDataCollection WorkingGachaData { get; set; }
        public string? WorkingUid { get; private set; }
        /// <summary>
        /// 设置祈愿接口获取延迟是否启用
        /// </summary>
        public bool IsFetchDelayEnabled { get; set; } = true;
        /// <summary>
        /// 随机延迟的范围
        /// </summary>
        public (int min, int max) DelayRange
        {
            get => delay;

            set
            {
                Requires.Range(value.min <= value.max, "祈愿记录获取延迟的最小值不能大于最大值");
                delay = value;
            }
        }

        /// <summary>
        /// 初始化联机抽卡记录工作器
        /// </summary>
        /// <param name="gachaLogUrl">url</param>
        /// <param name="gachaData">需要操作的祈愿数据</param>
        /// <param name="batchSize">每次请求获取的批大小 最大20 默认20</param>
        public GachaLogWorker(string gachaLogUrl, GachaDataCollection gachaData, int batchSize = 20)
        {
            this.gachaLogUrl = gachaLogUrl;
            WorkingGachaData = gachaData;
            this.batchSize = batchSize;
        }

        private Config? gachaConfig;
        public async Task<Config?> GetCurrentGachaConfigAsync()
        {
            gachaConfig ??= await GetGachaConfigAsync();
            return gachaConfig;
        }

        /// <summary>
        /// 获取祈愿池信息
        /// </summary>
        /// <returns>网络问题导致的可能会返回null</returns>
        private async Task<Config?> GetGachaConfigAsync()
        {
            Requester requester = new(new RequestOptions
            {
                {"Accept", RequestOptions.Json },
                {"User-Agent", RequestOptions.CommonUA2101 }
            });
            Response<Config>? resp = await requester.GetAsync<Config>(gachaLogUrl?.Replace("getGachaLog?", "getConfigList?"));
            this.Log(resp?.Data);
            return resp?.Data;
        }

        /// <summary>
        /// 获取单个奖池的祈愿记录增量信息
        /// 并自动合并数据
        /// </summary>
        /// <param name="type">卡池类型</param>
        public async Task<string?> FetchGachaLogIncreaselyAsync(ConfigType type, IProgress<FetchProgress> progress)
        {
            List<GachaLogItem> increment = new();
            int currentPage = 0;
            long endId = 0;
            do
            {
                progress.Report(new(type.Name, ++currentPage));
                (bool Succeed, GachaLog log) = await TryGetBatchAsync(type, endId);
                if (Succeed)
                {
                    Requires.NotNull(log.List!, nameof(log.List));

                    foreach (GachaLogItem item in log.List)
                    {
                        WorkingUid = item.Uid;
                        //this one is increment
                        if (item.TimeId > WorkingGachaData.GetNewestTimeIdOf(type, item.Uid))
                        {
                            increment.Add(item);
                        }
                        else//already done the new item
                        {
                            await MergeIncrementAsync(type, increment);
                            return WorkingUid;
                        }
                    }
                    //last page
                    if (log.List.Count < batchSize)
                    {
                        break;
                    }
                    endId = log.List.Last().TimeId;
                }
                else
                {
                    WorkingUid = null;
                    Verify.FailOperation("提供的Url无效");
                }
                if (IsFetchDelayEnabled)
                {
                    await Task.Delay(GetRandomDelay());
                }
            } while (true);
            //first time fecth could go here
            await MergeIncrementAsync(type, increment);
            return WorkingUid;
        }

        /// <summary>
        /// 获取单个奖池的祈愿记录全量信息
        /// 并自动合并数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<string?> FetchGachaLogAggressivelyAsync(ConfigType type, IProgress<FetchProgress> progress)
        {
            List<GachaLogItem> full = new();
            int currentPage = 0;
            long endId = 0;
            do
            {
                progress.Report(new(type.Name, ++currentPage));
                (bool Succeed, GachaLog log) = await TryGetBatchAsync(type, endId);
                if (Succeed)
                {
                    if (log.List is not null)
                    {
                        foreach (GachaLogItem item in log.List)
                        {
                            WorkingUid = item.Uid;
                            full.Add(item);
                        }
                        //last page
                        if (log.List.Count < batchSize)
                        {
                            break;
                        }
                        endId = log.List.Last().TimeId;
                    }
                }
                else
                {
                    WorkingUid = null;
                    Verify.FailOperation("提供的Url无效");
                }
                if (IsFetchDelayEnabled)
                {
                    await Task.Delay(GetRandomDelay());
                }

            } while (true);
            //first time fecth could go here
            await MergeFullAsync(type, full);
            return WorkingUid;
        }

        /// <summary>
        /// 合并增量
        /// </summary>
        /// <param name="type">卡池类型</param>
        /// <param name="increment">增量</param>
        private async Task MergeIncrementAsync(ConfigType type, List<GachaLogItem> increment)
        {
            await Task.Yield();
            //卡池内没有物品导致无法判断Uid
            if (WorkingUid is null)
            {
                return;
            }
            if (!WorkingGachaData.HasUid(WorkingUid))
            {
                WorkingGachaData.Add(WorkingUid, new GachaData());
            }
            //简单的将老数据插入到增量后侧，最后重置数据
            GachaData data = WorkingGachaData[WorkingUid]!;
            string? key = type.Key;
            if (key is not null)
            {
                if (data.ContainsKey(key))
                {
                    List<GachaLogItem>? local = data[key];
                    if (local is not null)
                    {
                        increment.AddRange(local);
                    }
                }
                data[key] = increment;
            }
        }

        /// <summary>
        /// 合并全量
        /// </summary>
        /// <param name="type">卡池类型</param>
        private async Task MergeFullAsync(ConfigType type, List<GachaLogItem> full)
        {
            await Task.Yield();
            //卡池内没有物品导致无法判断Uid
            if (WorkingUid is null)
            {
                return;
            }
            if (!WorkingGachaData.HasUid(WorkingUid))
            {
                WorkingGachaData.Add(WorkingUid, new GachaData());
            }
            //将老数据插入到后侧，最后重置数据
            GachaData data = WorkingGachaData[WorkingUid]!;
            string? key = type.Key;
            if (key is not null)
            {
                if (data.ContainsKey(key))
                {
                    List<GachaLogItem>? local = data[key];
                    if (local is not null)
                    {
                        //fix InvalidOperationException at full.Last()
                        if (full.Count > 0)
                        {
                            int lastIndex = local.FindLastIndex(i => i.TimeId == full.Last().TimeId);
                            if (lastIndex >= 0)
                            {
                                local = local.GetRange(lastIndex + 1, local.Count - 1 - (lastIndex + 1) + 1);
                            }
                        }
                        full.AddRange(local);
                    }
                }
                data[key] = full;
            }
        }

        /// <summary>
        /// 尝试获得 <see cref="batchSize"/> 个奖池物品
        /// </summary>
        /// <param name="type"></param>
        /// <param name="endId"></param>
        /// <returns></returns>
        private async Task<Result<bool, GachaLog>> TryGetBatchAsync(ConfigType type, long endId)
        {
            //modify the url
            string[]? splitedUrl = gachaLogUrl?.Split('?');
            string? baseUrl = splitedUrl?[0];

            //parse querystrings
            QueryString query = QueryString.Parse(splitedUrl?[1]);
            query.Set("gacha_type", type.Key);
            //20 is the max size the api can return
            query.Set("size", batchSize.ToString());
            query.Set("lang", "zh-cn");
            query.Set("end_id", endId.ToString());
            string finalUrl = $"{baseUrl}?{query}";

            Requester requester = new(new RequestOptions
            {
                {"Accept", RequestOptions.Json }
            });
            Response<GachaLog>? resp = await requester.GetAsync<GachaLog>(finalUrl);
            return new(Response.IsOk(resp), resp?.Data ?? new());
        }
    }
}
