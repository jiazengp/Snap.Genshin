using DGP.Genshin.Common;
using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.Record;
using DGP.Genshin.MiHoYoAPI.Record.Avatar;
using DGP.Genshin.MiHoYoAPI.Record.SpiralAbyss;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Extensions.System;
using System.Collections.Generic;
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
            return await Task.Run(() =>
            {
                if (uid is null)
                {
                    return new Record("请输入Uid");
                }

                RecordProvider recordProvider = new(CookieManager.Cookie);

                //figure out the server
                string? server = recordProvider.EvaluateUidRegion(uid);
                if (server is null)
                {
                    RecordProgressed?.Invoke(null);
                    return new Record("不支持查询此UID");
                }

                RecordProgressed?.Invoke("正在获取 玩家基础统计信息 (1/5)");
                PlayerInfo? playerInfo = recordProvider.GetPlayerInfo(uid, server);
                if (playerInfo is null)
                {
                    RecordProgressed?.Invoke(null);
                    return new Record($"获取玩家基本信息失败");
                }

                RecordProgressed?.Invoke("正在获取 本期深境螺旋信息 (2/5)");
                SpiralAbyss? spiralAbyss = recordProvider.GetSpiralAbyss(uid, server, 1);
                if (spiralAbyss is null)
                {
                    RecordProgressed?.Invoke(null);
                    return new Record($"获取本期深境螺旋信息失败");
                }

                RecordProgressed?.Invoke("正在获取 上期深境螺旋信息 (3/5)");
                SpiralAbyss? lastSpiralAbyss = recordProvider.GetSpiralAbyss(uid, server, 2);
                if (lastSpiralAbyss is null)
                {
                    RecordProgressed?.Invoke(null);
                    return new Record($"获取上期深境螺旋信息失败");
                }

                RecordProgressed?.Invoke("正在获取 活动挑战信息 (4/5)");
                dynamic? activitiesInfo = recordProvider.GetActivities(uid, server);
                if (activitiesInfo is null)
                {
                    RecordProgressed?.Invoke(null);
                    return new Record($"获取活动信息失败");
                }

                RecordProgressed?.Invoke("正在获取 详细角色信息 (5/5)");
                DetailedAvatarInfo? detailedAvatarInfo = recordProvider.GetDetailAvaterInfo(uid, server, playerInfo);
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

        public static event RecordProgressedHandler? RecordProgressed;

        #region 单例
        private const string QueryHistoryFile = "history.dat";

        private static RecordService? instance;
        private static readonly object _lock = new();
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
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new RecordService();
                        }
                    }
                }
                return instance;
            }
        }

        public void UnInitialize()
        {
            File.WriteAllText(QueryHistoryFile, Json.Stringify(QueryHistory));
            this.Log("uninitialized");
        }
        #endregion
    }

    public delegate void RecordProgressedHandler(string? info);
}