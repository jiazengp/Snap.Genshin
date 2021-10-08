using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Request;
using DGP.Genshin.Models.YoungMoe;
using DGP.Genshin.Models.YoungMoe.Collocation;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using System;
using System.Collections.Generic;
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
        private List<AvatarSimple> avatarDictionary;
        private List<DetailedAvatarInfo> collocationAll;
        private List<AvatarInfo> collocation11;
        private int totalSubmitted;
        private int abyssPassed;

        public List<AvatarSimple> AvatarDictionary { get => this.avatarDictionary; set => Set(ref this.avatarDictionary, value); }
        public List<DetailedAvatarInfo> CollocationAll { get => this.collocationAll; set => Set(ref this.collocationAll, value); }
        public List<AvatarInfo> Collocation11 { get => this.collocation11; set => Set(ref this.collocation11, value); }
        public int TotalSubmitted { get => this.totalSubmitted; set => Set(ref this.totalSubmitted, value); }
        public int AbyssPassed { get => this.abyssPassed; set => Set(ref this.abyssPassed, value); }
        #endregion

        private bool isInitialized = false;
        public async Task Initialize()
        {
            if (this.isInitialized)
            {
                return;
            }
            this.isInitialized = true;

            this.AvatarDictionary = await GetAllAvatarAsync();
            this.CollocationAll = await GetCollocationRankFinalAsync();
            this.Collocation11 = await GetCollocationRankOf11FloorAsync();

            this.TotalSubmitted = Int32.Parse((await GetTotalSubmittedGamerAsync()).Count);
            this.AbyssPassed = Int32.Parse((await GetSprialAbyssPassedGamerAsync()).Count);
        }

        #region API
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<AvatarInfo>> GetCollocationRankOf11FloorAsync()
        {
            List<AvatarInfo> resp = await Task.Run(() =>
            Json.FromWebsite<List<AvatarInfo>>($@"{YoungMoeData}/collocationRank_11.json"));
            return resp;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<DetailedAvatarInfo>> GetCollocationRankFinalAsync()
        {
            List<DetailedAvatarInfo> resp = await Task.Run(() =>
            Json.FromWebsite<List<DetailedAvatarInfo>>($@"{YoungMoeData}/collocationRank_fin.json"));
            return resp;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Gamers> GetTotalSubmittedGamerAsync()
        {
            Requester requester = new Requester();
            Response<Gamers> resp = await Task.Run(() =>
            requester.Get<Gamers>($@"{ApiYoungMoe}/totalGamer"));
            return resp.Data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Gamers> GetSprialAbyssPassedGamerAsync()
        {
            Requester requester = new Requester();
            Response<Gamers> resp = await Task.Run(() =>
            requester.Get<Gamers>($@"{ApiYoungMoe}/currentAbyssGamer"));
            return resp.Data;
        }
        /// <summary>
        /// 获取所有角色的图片与Id
        /// </summary>
        /// <returns></returns>
        public async Task<List<AvatarSimple>> GetAllAvatarAsync()
        {
            List<AvatarSimple> resp = await Task.Run(() =>
            Json.FromWebsite<List<AvatarSimple>>($@"{YoungMoeData}/allAvatarMin.json"));
            return resp;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="floor">12,11,10,9</param>
        /// <returns></returns>
        public async Task<int[]> GetTeamRankDataRaw(int floor)
        {
            int[] resp = await Task.Run(() =>
            Json.FromWebsite<int[]>($@"{YoungMoeData}/teamRankMin_{floor}_3.json"));
            return resp;
        }
        #endregion

        #region 单例
        private static CelestiaDatabaseService instance;
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
}
