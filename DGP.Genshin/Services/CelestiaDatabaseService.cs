using DGP.Genshin.Data.Characters;
using DGP.Genshin.Data.Helpers;
using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Request;
using DGP.Genshin.Models.YoungMoe;
using DGP.Genshin.Models.YoungMoe.Collocation;
using DGP.Genshin.Models.YoungMoe.Recommand;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// 天空岛数据库服务
    /// </summary>
    public class CelestiaDatabaseService : Observable
    {
        private const string YoungMoeData = @"https://youngmoe.com/data";
        private const string ApiYoungMoe = @"https://api.youngmoe.com";

        #region Observable
        /// <summary>
        /// 全角色映射
        /// </summary>
        private List<AvatarSimple>? allAvatarMap;
        private List<DetailedAvatarInfo>? collocationAll;
        private List<AvatarInfo>? collocation11;
        private int totalSubmitted;
        private int abyssPassed;

        public List<AvatarSimple>? AvatarDictionary { get => this.allAvatarMap; set => this.Set(ref this.allAvatarMap, value); }
        public List<DetailedAvatarInfo>? CollocationAll { get => this.collocationAll; set => this.Set(ref this.collocationAll, value); }
        public List<AvatarInfo>? Collocation11 { get => this.collocation11; set => this.Set(ref this.collocation11, value); }
        public int TotalSubmitted { get => this.totalSubmitted; set => this.Set(ref this.totalSubmitted, value); }
        public int AbyssPassed { get => this.abyssPassed; set => this.Set(ref this.abyssPassed, value); }
        #endregion

        public bool IsInitialized => this.isInitialized;

        private bool isInitialized = false;
        public async Task InitializeAsync()
        {
            if (this.isInitialized)
            {
                return;
            }
            this.isInitialized = true;

            this.selectedFloor = this.Floors.First();

            this.AvatarDictionary = await this.GetAllAvatarAsync();
            this.CollocationAll = await this.GetCollocationRankFinalAsync();
            this.Collocation11 = await this.GetCollocationRankOf11FloorAsync();

            Gamers? totalSubmitted = await this.GetTotalSubmittedGamerAsync();
            Gamers? sprialAbyssPassed = await this.GetSprialAbyssPassedGamerAsync();

            if(totalSubmitted is not null && sprialAbyssPassed is not null)
            {
                if(totalSubmitted.Count is not null && sprialAbyssPassed.Count is not null)
                {
                    this.TotalSubmitted = Int32.Parse(totalSubmitted.Count);
                    this.AbyssPassed = Int32.Parse(sprialAbyssPassed.Count);
                }
            }

            await this.RefershRecommandsAsync();
        }

        #region API
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<AvatarInfo>?> GetCollocationRankOf11FloorAsync()
        {
            List<AvatarInfo>? resp = await Task.Run(() =>
            Json.FromWebsite<List<AvatarInfo>>($@"{YoungMoeData}/collocationRank_11.json"));
            return resp;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<DetailedAvatarInfo>?> GetCollocationRankFinalAsync()
        {
            List<DetailedAvatarInfo>? resp = await Task.Run(() =>
            Json.FromWebsite<List<DetailedAvatarInfo>>($@"{YoungMoeData}/collocationRank_fin.json"));
            return resp;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Gamers?> GetTotalSubmittedGamerAsync()
        {
            Response<Gamers>? resp = await Task.Run(() =>
            new Requester().Get<Gamers>($@"{ApiYoungMoe}/totalGamer"));
            return resp?.Data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Gamers?> GetSprialAbyssPassedGamerAsync()
        {
            Response<Gamers>? resp = await Task.Run(() =>
            new Requester().Get<Gamers>($@"{ApiYoungMoe}/currentAbyssGamer"));
            return resp?.Data;
        }
        /// <summary>
        /// 获取所有角色的图片与Id
        /// </summary>
        /// <returns></returns>
        public async Task<List<AvatarSimple>?> GetAllAvatarAsync()
        {
            List<AvatarSimple>? resp = await Task.Run(() =>
            Json.FromWebsite<List<AvatarSimple>>($@"{YoungMoeData}/allAvatarMin.json"));
            return resp;
        }
        /// <summary>
        /// <code>List{[a,a,a,a,b,b,b,b,count]}</code>
        /// </summary>
        /// <param name="floor">12,11,10,9</param>
        /// <returns></returns>
        public async Task<List<int[]>?> GetTeamRankRawDataAsync(int floor)
        {
            List<int[]>? resp = await Task.Run(() =>
            Json.FromWebsite<List<int[]>>($@"{YoungMoeData}/teamRankMin_{floor}_3.json"));
            return resp;
        }

        public async Task<string?> PostUid(string? uid)
        {
            if (uid is null)
            {
                return "当前无可用玩家信息";
            }
            Requester requester = new Requester(new RequestOptions
            {
                {"Content-Type","application/x-www-form-urlencoded;charset=UTF-8" }
            });

            Response<string>? resp = await Task.Run(() =>
            requester.Post<string>($@"{ApiYoungMoe}/postuid", $"uid={uid}"));
            return resp?.Data;
        }
        #endregion

        #region Recommand
        private List<Recommand>? recommands;
        public List<Recommand>? Recommands { get => this.recommands; set => this.Set(ref this.recommands, value); }

        private List<int> floors = new List<int> { 12, 11, 10, 9 };
        public List<int> Floors { get => this.floors; set => this.Set(ref this.floors, value); }

        private int selectedFloor;
        public int SelectedFloor
        {
            get => this.selectedFloor; set
            {
                this.selectedFloor = value;
                this.OnFloorChanged();
            }
        }

        private async void OnFloorChanged() => await this.RefershRecommandsAsync();

        public async Task RefershRecommandsAsync()
        {
            if (RecordService.Instance.CurrentRecord is null || this.allAvatarMap is null)
            {
                return;
            }

            this.Recommands = null;

            List<int[]>? teamDataRaw = await this.GetTeamRankRawDataAsync(this.SelectedFloor);
            List<int>? ownedAvatarRaw = RecordService.Instance.CurrentRecord.DetailedAvatars?
                .Where(i => this.allAvatarMap.Exists(a => a.CnName == i.Name))
                .Select(i => this.allAvatarMap.First(a => a.CnName == i.Name).Id)
                .ToList();

            if (ownedAvatarRaw is null || teamDataRaw is null)
            {
                return;
            }

            List<int> notOwnedAvatarRaw = this.allAvatarMap
                .Select(i => i.Id)
                .Where(i => !ownedAvatarRaw.Exists(j => j == i))
                .ToList();

            //filter out not owned avatar
            teamDataRaw.RemoveAll(raw => notOwnedAvatarRaw.Contains(raw[0]));
            teamDataRaw.RemoveAll(raw => notOwnedAvatarRaw.Contains(raw[1]));
            teamDataRaw.RemoveAll(raw => notOwnedAvatarRaw.Contains(raw[2]));
            teamDataRaw.RemoveAll(raw => notOwnedAvatarRaw.Contains(raw[3]));
            teamDataRaw.RemoveAll(raw => notOwnedAvatarRaw.Contains(raw[4]));
            teamDataRaw.RemoveAll(raw => notOwnedAvatarRaw.Contains(raw[5]));
            teamDataRaw.RemoveAll(raw => notOwnedAvatarRaw.Contains(raw[6]));
            teamDataRaw.RemoveAll(raw => notOwnedAvatarRaw.Contains(raw[7]));

            this.Recommands = teamDataRaw.Select(raw => new Recommand
            {
                UpHalf = new List<Character?>
                {
                    this.ToCharacter(this.allAvatarMap.Find(a => a.Id == raw[0])),
                    this.ToCharacter(this.allAvatarMap.Find(a => a.Id == raw[1])),
                    this.ToCharacter(this.allAvatarMap.Find(a => a.Id == raw[2])),
                    this.ToCharacter(this.allAvatarMap.Find(a => a.Id == raw[3]))
                },
                DownHalf = new List<Character?>
                {
                    this.ToCharacter(this.allAvatarMap.Find(a => a.Id == raw[4])),
                    this.ToCharacter(this.allAvatarMap.Find(a => a.Id == raw[5])),
                    this.ToCharacter(this.allAvatarMap.Find(a => a.Id == raw[6])),
                    this.ToCharacter(this.allAvatarMap.Find(a => a.Id == raw[7]))
                },
                Count = raw[8]
            }).TakeWhileAndPreserve(i => i.Count > this.TotalSubmitted * 0.0005, 10).AsParallel().ToList();
        }

        private Character? ToCharacter(AvatarSimple? avatar)
        {
            if (avatar == null)
            {
                return null;
            }
            if (avatar.CnName == "旅行者")
            {
                return new Character { Name = "旅行者", Star = StarHelper.FromRank(5) };
            }
            return MetaDataService.Instance.Characters.First(c => c.Name == avatar.CnName);
        }

        #endregion

        #region 单例
        private static CelestiaDatabaseService? instance;

        private static readonly object _lock = new();
        private CelestiaDatabaseService()
        {
        }
        public static CelestiaDatabaseService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new CelestiaDatabaseService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }

    public static class LINQHelper
    {
        public static IEnumerable<T> TakeWhileAndPreserve<T>(this IEnumerable<T> source, Func<T, bool> predicate, int leastCount)
        {
            IEnumerable<T> result = source.TakeWhile(predicate);
            if (result.Count() < leastCount)
            {
                result = source.Take(10);
            }
            return result;
        }
    }
}
