using DGP.Genshin.Common.Data.Behavior;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.Record;
using DGP.Genshin.MiHoYoAPI.Record.Avatar;
using DGP.Genshin.MiHoYoAPI.Record.SpiralAbyss;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.GameRecord
{
    /// <summary>
    /// 
    /// </summary>
    public class RecordService : Observable
    {
        #region Observable
        private Record? currentRecord;
        public Record? CurrentRecord { get => currentRecord; set => Set(ref currentRecord, value); }
        #endregion

        public List<string> QueryHistory { get; set; } = new();
        internal void AddQueryHistory(string? uid)
        {
            if (uid is not null)
            {
                if (!QueryHistory.Contains(uid))
                {
                    QueryHistory.Add(uid);
                }
            }
        }

        /// <summary>
        /// 查询玩家信息
        /// </summary>
        /// <param name="uid">uid</param>
        /// <returns></returns>
        public async Task<Record> GetRecordAsync(string? uid)
        {
            return await Task.Run(async () =>
            {
                if (uid is null)
                {
                    return new Record("请输入Uid");
                }

                RecordProvider recordProvider = new(CookieManager.CurrentCookie);

                //figure out the server
                string? server = recordProvider.EvaluateUidRegion(uid);
                if (server is null)
                {
                    RecordProgressed?.Invoke(null);
                    return new Record("不支持查询此UID");
                }

                RecordProgressed?.Invoke("正在获取 玩家基础统计信息 (1/5)");
                PlayerInfo? playerInfo = await recordProvider.GetPlayerInfoAsync(uid, server);
                if (playerInfo is null)
                {
                    RecordProgressed?.Invoke(null);
                    return new Record($"获取玩家基本信息失败");
                }

                RecordProgressed?.Invoke("正在获取 本期深境螺旋信息 (2/5)");
                SpiralAbyss? spiralAbyss = await recordProvider.GetSpiralAbyssAsync(uid, server, 1);
                if (spiralAbyss is null)
                {
                    RecordProgressed?.Invoke(null);
                    return new Record($"获取本期深境螺旋信息失败");
                }

                RecordProgressed?.Invoke("正在获取 上期深境螺旋信息 (3/5)");
                SpiralAbyss? lastSpiralAbyss = await recordProvider.GetSpiralAbyssAsync(uid, server, 2);
                if (lastSpiralAbyss is null)
                {
                    RecordProgressed?.Invoke(null);
                    return new Record($"获取上期深境螺旋信息失败");
                }

                RecordProgressed?.Invoke("正在获取 活动挑战信息 (4/5)");
                dynamic? activitiesInfo = await recordProvider.GetActivitiesAsync(uid, server);
                if (activitiesInfo is null)
                {
                    RecordProgressed?.Invoke(null);
                    return new Record($"获取活动信息失败");
                }

                RecordProgressed?.Invoke("正在获取 详细角色信息 (5/5)");
                DetailedAvatarInfo? detailedAvatarInfo = await recordProvider.GetDetailAvaterInfoAsync(uid, server, playerInfo);
                if (detailedAvatarInfo is null)
                {
                    RecordProgressed?.Invoke(null);
                    return new Record($"获取详细角色信息失败");
                }
                RecordProgressed?.Invoke(null);
                //return
                return new Record
                {
                    Success = true,
                    UserId = uid,
                    Server = server,
                    PlayerInfo = playerInfo,
                    SpiralAbyss = spiralAbyss,
                    LastSpiralAbyss = lastSpiralAbyss,
                    DetailedAvatars = detailedAvatarInfo.Avatars,
                    Activities = activitiesInfo
                };
            });
        }

        public static event Action<string?>? RecordProgressed;

        private const string QueryHistoryFile = "history.dat";

        public void UnInitialize()
        {
            File.WriteAllText(QueryHistoryFile, Json.Stringify(QueryHistory));
            this.Log("uninitialized");
        }

        #region 单例
        private static volatile RecordService? instance;
        [SuppressMessage("", "IDE0044")]
        private static object _locker = new();
        private RecordService()
        {
            if (File.Exists(QueryHistoryFile))
            {
                QueryHistory = Json.ToObject<List<string>>(File.ReadAllText(QueryHistoryFile)) ?? new();
            }
            this.Log("initialized");
        }
        public static RecordService Instance
        {
            get
            {
                if (instance is null)
                {
                    lock (_locker)
                    {
                        instance ??= new();
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}