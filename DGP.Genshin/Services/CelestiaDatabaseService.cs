using DGP.Genshin.Common;
using DGP.Genshin.DataModel.Characters;
using DGP.Genshin.DataModel.Helpers;
using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Request;
using DGP.Genshin.Models.YoungMoe;
using DGP.Genshin.Models.YoungMoe.Collocation;
using DGP.Genshin.Models.YoungMoe.Recommand;
using DGP.Snap.Framework.Data.Behavior;
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

        public List<AvatarSimple>? AvatarDictionary { get => allAvatarMap; set => Set(ref allAvatarMap, value); }
        public List<DetailedAvatarInfo>? CollocationAll { get => collocationAll; set => Set(ref collocationAll, value); }
        public List<AvatarInfo>? Collocation11 { get => collocation11; set => Set(ref collocation11, value); }
        public int TotalSubmitted { get => totalSubmitted; set => Set(ref totalSubmitted, value); }
        public int AbyssPassed { get => abyssPassed; set => Set(ref abyssPassed, value); }
        #endregion

        public bool IsInitialized => isInitialized;

        private bool isInitialized = false;
        public async Task InitializeAsync()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;

            selectedFloor = Floors.First();

            AvatarDictionary = await GetAllAvatarAsync();
            CollocationAll = await GetCollocationRankFinalAsync();
            Collocation11 = await GetCollocationRankOf11FloorAsync();

            Gamers? totalSubmitted = await GetTotalSubmittedGamerAsync();
            Gamers? sprialAbyssPassed = await GetSprialAbyssPassedGamerAsync();

            if (totalSubmitted is not null && sprialAbyssPassed is not null)
            {
                if (totalSubmitted.Count is not null && sprialAbyssPassed.Count is not null)
                {
                    TotalSubmitted = int.Parse(totalSubmitted.Count);
                    AbyssPassed = int.Parse(sprialAbyssPassed.Count);
                }
            }

            await RefershRecommandsAsync();
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
        public List<Recommand>? Recommands { get => recommands; set => Set(ref recommands, value); }

        private List<int> floors = new List<int> { 12, 11, 10, 9 };
        public List<int> Floors { get => floors; set => Set(ref floors, value); }

        private int selectedFloor;
        public int SelectedFloor
        {
            get => selectedFloor; set
            {
                selectedFloor = value;
                OnFloorChanged();
            }
        }

        private async void OnFloorChanged()
        {
            await RefershRecommandsAsync();
        }

        public async Task RefershRecommandsAsync()
        {
            if (RecordService.Instance.CurrentRecord is null || allAvatarMap is null)
            {
                return;
            }

            Recommands = null;

            List<int[]>? teamDataRaw = await GetTeamRankRawDataAsync(SelectedFloor);
            List<int>? ownedAvatarRaw = RecordService.Instance.CurrentRecord.DetailedAvatars?
                .Where(i => allAvatarMap.Exists(a => a.CnName == i.Name))
                .Select(i => allAvatarMap.First(a => a.CnName == i.Name).Id)
                .ToList();

            if (ownedAvatarRaw is null || teamDataRaw is null)
            {
                return;
            }

            List<int> notOwnedAvatarRaw = allAvatarMap
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

            Recommands = teamDataRaw.Select(raw => new Recommand
            {
                UpHalf = new List<Character?>
                {
                    ToCharacter(allAvatarMap.Find(a => a.Id == raw[0])),
                    ToCharacter(allAvatarMap.Find(a => a.Id == raw[1])),
                    ToCharacter(allAvatarMap.Find(a => a.Id == raw[2])),
                    ToCharacter(allAvatarMap.Find(a => a.Id == raw[3]))
                },
                DownHalf = new List<Character?>
                {
                    ToCharacter(allAvatarMap.Find(a => a.Id == raw[4])),
                    ToCharacter(allAvatarMap.Find(a => a.Id == raw[5])),
                    ToCharacter(allAvatarMap.Find(a => a.Id == raw[6])),
                    ToCharacter(allAvatarMap.Find(a => a.Id == raw[7]))
                },
                Count = raw[8]
            }).TakeWhileAndPreserve(i => i.Count > TotalSubmitted * 0.0005, 10).AsParallel().ToList();
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
