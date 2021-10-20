using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Record;
using DGP.Genshin.Models.MiHoYo.Record.Avatar;
using DGP.Genshin.Models.MiHoYo.Record.SpiralAbyss;
using DGP.Genshin.Models.MiHoYo.Request;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// this service shouldn't be disposed during the runtime cause re-request web really slow
    /// </summary>
    public class RecordService : Observable
    {
        private const string QueryHistoryFile = "history.dat";
        private const string BaseUrl = @"https://api-takumi.mihoyo.com/game_record/app/genshin/api";
        private const string Referer = @"https://webstatic.mihoyo.com/app/community-game-records/index.html?v=6";

        #region Observable
        private Record? currentRecord;
        public Record? CurrentRecord { get => this.currentRecord; set => this.Set(ref this.currentRecord, value); }

        private DetailedAvatar? selectedAvatar;
        public DetailedAvatar? SelectedAvatar
        {
            get => this.selectedAvatar; set
            {
                this.Set(ref this.selectedAvatar, value);
                this.SelectedReliquary = this.SelectedAvatar?.Reliquaries?.Count() > 0 ? this.SelectedAvatar.Reliquaries?.First() : null;
            }
        }

        private Reliquary? selectedReliquary;
        public Reliquary? SelectedReliquary { get => this.selectedReliquary; set => this.Set(ref this.selectedReliquary, value); }
        #endregion

        public List<string> QueryHistory { get; set; } = new List<string>();
        internal void AddQueryHistory(string uid)
        {
            if (!this.QueryHistory.Contains(uid))
                this.QueryHistory.Add(uid);
        }

        [SuppressMessage("", "IDE0037")]
        [SuppressMessage("", "IDE0050")]
        public async Task<Record> GetRecordAsync(string uid)
        {
            this.Log($"querying uid:{uid}");
            string? cookie = CookieManager.Cookie;
            if (cookie is null)
            {
                return new Record("请提供Cookie");
            }
            Requester requester = new Requester(new RequestOptions
            {
                {"Accept", RequestOptions.Json },
                {"x-rpc-app_version", DynamicSecretProvider2.AppVersion },
                {"User-Agent", RequestOptions.CommonUA2_11_1 },
                {"x-rpc-client_type", "5" },
                {"Referer",Referer },
                {"Cookie", cookie },
                {"X-Requested-With", RequestOptions.Hyperion }
            });
            //figure out the server
            if (!this.TryEvaluateUidRegion(uid, out string? server))
            {
                RecordProgressed?.Invoke(null);
                return new Record("不支持查询此UID");
            }

            Response<PlayerInfo>? playerInfo = null;
            if (!await Task.Run(() => this.TryGet("正在获取 玩家基础统计信息 (1/5)",
                $@"{BaseUrl}/index?server={server}&role_id={uid}", requester, out playerInfo)))
            {
                RecordProgressed?.Invoke(null);
                return new Record($"获取玩家基本信息失败：\n{playerInfo?.Message}");
            }
            Response<SpiralAbyss>? spiralAbyss = null;
            if (!await Task.Run(() => this.TryGet("正在获取 本期深境螺旋信息 (2/5)",
                $@"{BaseUrl}/spiralAbyss?schedule_type=1&server={server}&role_id={uid}", requester, out spiralAbyss)))
            {
                RecordProgressed?.Invoke(null);
                return new Record($"获取本期深境螺旋信息失败：\n{spiralAbyss?.Message}");
            }
            Response<SpiralAbyss>? lastSpiralAbyss = null;
            if (!await Task.Run(() => this.TryGet("正在获取 上期深境螺旋信息 (3/5)",
                $@"{BaseUrl}/spiralAbyss?schedule_type=2&server={server}&role_id={uid}", requester, out lastSpiralAbyss)))
            {
                RecordProgressed?.Invoke(null);
                return new Record($"获取上期深境螺旋信息失败：\n{lastSpiralAbyss?.Message}");
            }
            Response<dynamic>? activitiesInfo = null;
            if (!await Task.Run(() => this.TryGet("正在获取 活动挑战信息 (4/5)",
                $@"{BaseUrl}/activities?server={server}&role_id={uid}", requester, out activitiesInfo)))
            {
                RecordProgressed?.Invoke(null);
                return new Record($"获取活动信息失败：\n{activitiesInfo?.Message}");
            }
            //prepare for character
            requester.Headers.Remove("x-rpc-device_id");
            var data = new
            {
                character_ids = playerInfo?.Data?.Avatars?.Select(x => x.Id).ToList(),
                role_id = uid,
                server = server
            };
            Response<DetailedAvatarInfo>? roles = null;
            if (!await Task.Run(() => this.TryPost("正在获取 详细角色信息 (5/5)",
                $@"{BaseUrl}/character", data, requester, out roles)))
            {
                RecordProgressed?.Invoke(null);
                return new Record($"获取详细角色信息失败：\n{roles?.Message}");
            }

            RecordProgressed?.Invoke(null);
            //return
            return roles?.ReturnCode != 0 ? new Record(roles?.Message) : new Record
            {
                Success = true,
                UserId = uid,
                Server = server,
                PlayerInfo = playerInfo?.Data,
                SpiralAbyss = spiralAbyss?.Data,
                LastSpiralAbyss = lastSpiralAbyss?.Data,
                DetailedAvatars = roles.Data?.Avatars,
                Activities = activitiesInfo?.Data
            };
        }

        private bool TryEvaluateUidRegion(string uid, out string? result)
        {
            if (String.IsNullOrEmpty(uid))
            {
                result = null;
                return false;
            }
            Dictionary<char, string> serverDict = new Dictionary<char, string>()
            {
                { '1', "cn_gf01" },
                { '2', "cn_gf01" },
                { '5', "cn_qd01" },
                //{ '6', "os_usa" },
                //{ '7', "os_euro" },
                //{ '8', "os_asia" },
                //{ '9', "os_cht" }
            };
            return serverDict.TryGetValue(uid[0], out result);
        }
        private bool TryGet<T>(string info, string url, Requester requester, out Response<T> response)
        {
            RecordProgressed?.Invoke(info);
            requester.Headers["DS"] = DynamicSecretProvider2.Create(url);
            response = requester.Get<T>(url);
            return response.ReturnCode == 0;
        }
        private bool TryPost<T>(string info, string url, object data, Requester requester, out Response<T> response)
        {
            RecordProgressed?.Invoke(info);
            requester.Headers["DS"] = DynamicSecretProvider2.Create(url, data);
            response = requester.Post<T>(url, data);
            return response.ReturnCode == 0;
        }

        public static event RecordProgressedHandler? RecordProgressed;

        #region 单例
        private static RecordService? instance;
        private static readonly object _lock = new();
        private RecordService()
        {
            if (File.Exists(QueryHistoryFile))
            {
                this.QueryHistory = Json.ToObject<List<string>>(File.ReadAllText(QueryHistoryFile));
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

        ~RecordService()
        {
            File.WriteAllText(QueryHistoryFile, Json.Stringify(this.QueryHistory));
            this.Log("uninitialized");
        }
        #endregion
    }

    public delegate void RecordProgressedHandler(string? info);
}