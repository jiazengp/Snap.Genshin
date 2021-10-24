using DGP.Genshin.DataModel.Characters;
using DGP.Genshin.DataModel.Helpers;
using DGP.Genshin.Services.GameRecord;
using DGP.Genshin.YoungMoeAPI;
using DGP.Genshin.YoungMoeAPI.Collocation;
using DGP.Snap.Framework.Data.Behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.CelestiaDatabase
{
    /// <summary>
    /// 天空岛数据库服务
    /// </summary>
    public class CelestiaDatabaseService : Observable
    {
        private readonly YoungMoeAPI.CelestiaDatabase database;

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
        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;

            AvatarDictionary = database.GetAllAvatar();
            CollocationAll = database.GetCollocationRankOfFinal();
            Collocation11 = database.GetCollocationRankOf11Floor();

            Gamers? totalSubmitted = database.GetTotalSubmittedGamer();
            Gamers? sprialAbyssPassed = database.GetSprialAbyssPassedGamer();

            if (totalSubmitted is not null && sprialAbyssPassed is not null)
            {
                if (totalSubmitted.Count is not null && sprialAbyssPassed.Count is not null)
                {
                    TotalSubmitted = int.Parse(totalSubmitted.Count);
                    AbyssPassed = int.Parse(sprialAbyssPassed.Count);
                }
            }

            SelectedFloor = Floors.First();
        }

        #region Recommand
        private List<Recommand>? recommands;
        public List<Recommand>? Recommands { get => recommands; set => Set(ref recommands, value); }

        private List<int> floors = new() { 12, 11, 10, 9 };
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
            await Task.Run(RefershRecommands);
        }

        public void RefershRecommands()
        {
            if (RecordService.Instance.CurrentRecord is null || allAvatarMap is null)
            {
                return;
            }
            //clear recommands
            Recommands = null;

            List<int[]>? teamDataRaw = database.GetTeamRankRawData(SelectedFloor);
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
            return avatar == null 
                ? null 
                : avatar.CnName == "旅行者" 
                ? new Character { Name = "旅行者", Star = StarHelper.FromRank(5) } 
                : (MetaDataService.Instance.Characters?.First(c => c.Name == avatar.CnName));
        }

        #endregion

        #region 单例
        private static CelestiaDatabaseService? instance;

        private static readonly object _lock = new();
        private CelestiaDatabaseService()
        {
            database = new();
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

    /// <summary>
    /// 推荐的配队
    /// </summary>
    public class Recommand
    {
        public List<Character?>? UpHalf { get; set; }
        public List<Character?>? DownHalf { get; set; }

        public int Count { get; set; }
    }
}
