using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.MiHoYoAPI.Request;
using DGP.Genshin.MiHoYoAPI.Response;
using DGP.Genshin.Service.Abstraction.GachaStatistic;
using Snap.Core.Logging;
using Snap.Data.Primitive;
using Snap.Net.QueryString;
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

        private (int Min, int Max) delay = (500, 1000);
        private Config? gachaConfig;

        /// <summary>
        /// 初始化联机抽卡记录工作器
        /// </summary>
        /// <param name="gachaLogUrl">url</param>
        /// <param name="gachaData">需要操作的祈愿数据</param>
        /// <param name="batchSize">每次请求获取的批大小 最大20 默认20</param>
        public GachaLogWorker(string gachaLogUrl, GachaDataCollection gachaData, int batchSize = 20)
        {
            this.gachaLogUrl = gachaLogUrl;
            this.WorkingGachaData = gachaData;
            this.batchSize = batchSize;
        }

        /// <inheritdoc/>
        public GachaDataCollection WorkingGachaData { get; set; }

        /// <inheritdoc/>
        public string? WorkingUid { get; private set; }

        /// <inheritdoc/>
        public bool IsFetchDelayEnabled { get; set; } = true;

        /// <summary>
        /// 随机延迟的范围
        /// </summary>
        public (int Min, int Max) DelayRange
        {
            get => this.delay;

            set
            {
                Requires.Range(value.Min <= value.Max, "祈愿记录获取延迟的最小值不能大于最大值");
                this.delay = value;
            }
        }

        /// <inheritdoc/>
        public int GetRandomDelay()
        {
            return this.DelayRange.Min + this.random.Next(this.DelayRange.Max - this.DelayRange.Min, this.DelayRange.Max);
        }

        /// <inheritdoc/>
        public async Task<Config?> GetCurrentGachaConfigAsync()
        {
            this.gachaConfig ??= await this.GetGachaConfigAsync();
            return this.gachaConfig;
        }

        /// <inheritdoc/>
        public async Task<string?> FetchGachaLogIncreaselyAsync(ConfigType type, IProgress<FetchProgress> progress)
        {
            List<GachaLogItem> increment = new();
            int currentPage = 0;
            long endId = 0;
            do
            {
                progress.Report(new(type.Name, ++currentPage));
                (bool isOk, GachaLog log) = await this.TryGetBatchAsync(type, endId);
                if (isOk)
                {
                    Requires.NotNull(log.List!, nameof(log.List));

                    foreach (GachaLogItem item in log.List)
                    {
                        this.WorkingUid = item.Uid;

                        // this one is increment
                        if (item.TimeId > this.WorkingGachaData.GetNewestTimeIdOf(type, item.Uid))
                        {
                            increment.Add(item);
                        }
                        else
                        {
                            // already done the new item
                            this.MergeIncrement(type, increment);
                            return this.WorkingUid;
                        }
                    }

                    // last page
                    if (log.List.Count < this.batchSize)
                    {
                        break;
                    }

                    endId = log.List.Last().TimeId;
                }
                else
                {
                    this.WorkingUid = null;
                    Verify.FailOperation("提供的Url无效");
                }

                if (this.IsFetchDelayEnabled)
                {
                    await Task.Delay(this.GetRandomDelay());
                }
            }
            while (true);

            // first time fecth could go here
            this.MergeIncrement(type, increment);
            return this.WorkingUid;
        }

        /// <inheritdoc/>
        public async Task<string?> FetchGachaLogAggressivelyAsync(ConfigType type, IProgress<FetchProgress> progress)
        {
            List<GachaLogItem> full = new();
            int currentPage = 0;
            long endId = 0;
            do
            {
                progress.Report(new(type.Name, ++currentPage));
                (bool isOk, GachaLog log) = await this.TryGetBatchAsync(type, endId);
                if (isOk)
                {
                    Requires.NotNull(log.List!, nameof(log.List));

                    foreach (GachaLogItem item in log.List)
                    {
                        this.WorkingUid = item.Uid;
                        full.Add(item);
                    }

                    // last page
                    if (log.List.Count < this.batchSize)
                    {
                        break;
                    }

                    endId = log.List.Last().TimeId;
                }
                else
                {
                    this.WorkingUid = null;
                    Verify.FailOperation("提供的Url无效");
                }

                if (this.IsFetchDelayEnabled)
                {
                    await Task.Delay(this.GetRandomDelay());
                }
            }
            while (true);

            this.MergeAggregation(type, full);
            return this.WorkingUid;
        }

        private async Task<Config?> GetGachaConfigAsync()
        {
            Requester requester = new(new RequestOptions
            {
                { "Accept", RequestOptions.Json },
                { "User-Agent", RequestOptions.CommonUA2101 },
            });
            Response<Config>? resp = await requester.GetAsync<Config>(this.gachaLogUrl?.Replace("getGachaLog?", "getConfigList?"));
            this.Log(resp?.Data);
            return resp?.Data;
        }

        /// <summary>
        /// 合并增量
        /// </summary>
        /// <param name="type">卡池类型</param>
        /// <param name="increment">增量</param>
        private void MergeIncrement(ConfigType type, List<GachaLogItem> increment)
        {
            // 卡池内没有物品导致无法判断Uid
            if (this.WorkingUid is null)
            {
                return;
            }

            if (!this.WorkingGachaData.HasUid(this.WorkingUid))
            {
                this.WorkingGachaData.Add(this.WorkingUid, new());
            }

            // 简单的将老数据插入到增量后侧，最后重置数据
            GachaData data = this.WorkingGachaData[this.WorkingUid]!;
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
        private void MergeAggregation(ConfigType type, List<GachaLogItem> full)
        {
            // 卡池内没有物品导致无法判断Uid
            if (this.WorkingUid is null)
            {
                return;
            }

            if (!this.WorkingGachaData.HasUid(this.WorkingUid))
            {
                this.WorkingGachaData.Add(this.WorkingUid, new GachaData());
            }

            // 将老数据插入到后侧，最后重置数据
            GachaData data = this.WorkingGachaData[this.WorkingUid]!;
            string? key = type.Key;
            if (key is not null)
            {
                if (data.ContainsKey(key))
                {
                    List<GachaLogItem>? local = data[key];
                    if (local is not null)
                    {
                        // fix InvalidOperationException at full.Last()
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
        /// <param name="type">卡池类型</param>
        /// <param name="endId">分页结尾Id</param>
        /// <returns>查询的结果</returns>
        private async Task<Result<bool, GachaLog>> TryGetBatchAsync(ConfigType type, long endId)
        {
            // modify the url
            string[] splitedUrl = this.gachaLogUrl.Split('?');

            // should only contain 2 element
            Assumes.True(splitedUrl.Length == 2);
            string baseUrl = splitedUrl[0];

            // parse querystrings
            QueryString query = QueryString.Parse(splitedUrl[1]);

            // 20 is the Max size the api can return
            query.Set("size", this.batchSize.ToString());
            query.Set("gacha_type", type.Key);
            query.Set("lang", "zh-cn");
            query.Set("end_id", endId.ToString());

            string finalUrl = $"{baseUrl}?{query}";

            Requester requester = new(new RequestOptions
            {
                { "Accept", RequestOptions.Json },
            });
            Response<GachaLog>? resp = await requester.GetAsync<GachaLog>(finalUrl);
            return new(Response.IsOk(resp), resp?.Data ?? new());
        }
    }
}