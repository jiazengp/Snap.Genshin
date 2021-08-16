using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Record;
using DGP.Genshin.Models.MiHoYo.Record.Avatar;
using DGP.Genshin.Models.MiHoYo.Record.SpiralAbyss;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// this service shouldn't be disposed during the runtime cause re-request web produces a lot lag
    /// </summary>
    internal class RecordService : Observable
    {
        private static readonly string QueryHistoryFile = "history.dat";
        private static readonly string BaseUrl = $@"https://api-takumi.mihoyo.com/game_record/genshin/api";

        #region Observable
        private Record currentRecord;
        public Record CurrentRecord { get => this.currentRecord; set => this.Set(ref this.currentRecord, value); }

        private DetailedAvatar selectedAvatar;
        public DetailedAvatar SelectedAvatar
        {
            get => this.selectedAvatar; set
            {
                this.Set(ref this.selectedAvatar, value);
                this.SelectedReliquary = this.SelectedAvatar?.Reliquaries.Count() > 0 ? this.SelectedAvatar?.Reliquaries.First() : null;
            }
        }

        private Reliquary selectedReliquary;
        public Reliquary SelectedReliquary { get => this.selectedReliquary; set => this.Set(ref this.selectedReliquary, value); }
        #endregion

        public List<string> QueryHistory { get; set; } = new List<string>();
        internal void AddQueryHistory(string uid)
        {
            if (!this.QueryHistory.Contains(uid))
                this.QueryHistory.Add(uid);
        }
        public async Task<Record> GetRecordAsync(string uid)
        {
            Requester requester = new Requester(await CookieManager.GetCookieAsync());
            //figure out the server
            string server = null;
            try
            {
                server = new Dictionary<char, string>()
                {
                    { '1', "cn_gf01" },
                    { '5', "cn_qd01" },
                    //server below is not supported by this api
                    //{ '6', "os_usa" },
                    //{ '7', "os_euro" },
                    //{ '8', "os_asia" },
                    //{ '9', "os_cht" }
                }[uid[0]];
            }
            catch
            {
                return new Record("不支持查询此UID");
            }

            Response<PlayerInfo> playerInfo = await Task.Run(() =>
            {
                return requester.Get<PlayerInfo>(
                    $@"{BaseUrl}/index?role_id={uid}&server={server}");
            });
            if (playerInfo.ReturnCode != 0)
                return new Record($"获取玩家基本信息失败：\n{playerInfo.Message}");

            Response<SpiralAbyss> spiralAbyss = await Task.Run(() =>
            {
                return requester.Get<SpiralAbyss>(
                    $@"{BaseUrl}/spiralAbyss?schedule_type=1&server={server}&role_id={uid}");
            });
            if (spiralAbyss.ReturnCode != 0)
                return new Record($"获取本期深渊螺旋信息失败：\n{spiralAbyss.Message}");

            Response<SpiralAbyss> lastSpiralAbyss = await Task.Run(() =>
            {
                return requester.Get<SpiralAbyss>(
                   $@"{BaseUrl}/spiralAbyss?schedule_type=2&server={server}&role_id={uid}");
            });
            if (lastSpiralAbyss.ReturnCode != 0)
                return new Record($"获取上期深渊螺旋信息失败：\n{lastSpiralAbyss.Message}");

            Response<dynamic> activitiesInfo = await Task.Run(() =>
            {
                return requester.Get<dynamic>(
                   $@"{BaseUrl}/activities?server={server}&role_id={uid}");
            });
            if (activitiesInfo.ReturnCode != 0)
                return new Record($"获取活动信息失败：\n{activitiesInfo.Message}");

            Response<DetailedAvatarInfo> roles = await Task.Run(() =>
            {
                return requester.Post<DetailedAvatarInfo>(
                   $@"{BaseUrl}/character", new
                   {
                       character_ids = playerInfo.Data.Avatars.Select(x => x.Id).ToList(),
                       role_id = uid,
                       server = server
                   });
            });
            if (roles.ReturnCode != 0)
                return new Record($"获取详细角色信息失败：\n{roles.Message}");
            //return
            return roles.ReturnCode != 0 ? new Record(roles.Message) : new Record
            {
                Success = true,
                UserId = uid,
                Server = server,
                PlayerInfo = playerInfo.Data,
                SpiralAbyss = spiralAbyss.Data,
                LastSpiralAbyss = lastSpiralAbyss.Data,
                DetailedAvatars = roles.Data.Avatars,
                Activities = activitiesInfo.Data
            };
        }

        #region 单例
        private static RecordService instance;
        private static readonly object _lock = new();
        private RecordService()
        {
            if (File.Exists(QueryHistoryFile))
            {
                this.QueryHistory = Json.ToObject<List<string>>(File.ReadAllText(QueryHistoryFile));
            }
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
        }
        #endregion
    }
}
