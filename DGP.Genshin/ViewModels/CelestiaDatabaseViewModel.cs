using DGP.Genshin.Common.Extensions.System.Collections.Generic;
using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.DataModel.Characters;
using DGP.Genshin.DataModel.Helpers;
using DGP.Genshin.DataModel.YoungMoe2;
using DGP.Genshin.Services;
using DGP.Genshin.Services.GameRecord;
using DGP.Genshin.YoungMoeAPI;
using DGP.Genshin.YoungMoeAPI.Collocation;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DGP.Genshin.Common.Extensions.System;
using System;
using Microsoft.Toolkit.Mvvm.Input;

namespace DGP.Genshin.ViewModels
{
    [ViewModel]
    public class CelestiaDatabaseViewModel : ObservableObject
    {
        private readonly CelestiaDatabaseService database;

        public CelestiaDatabaseViewModel(CelestiaDatabaseService database)
        {
            this.database = database;
            PageLoadadCommand = new AsyncRelayCommand(InitializeAsync);
        }

        public IAsyncRelayCommand PageLoadadCommand { get; set; }

        /// <summary>
        /// 全角色映射
        /// </summary>
        private List<AvatarSimple>? allAvatarMap;
        private IEnumerable<DetailedAvatarInfo2>? collocationAll;
        private IEnumerable<AvatarInfo>? collocation11;
        private int totalSubmitted;
        private int abyssPassed;

        public List<AvatarSimple>? AvatarDictionary { get => allAvatarMap; set => SetProperty(ref allAvatarMap, value); }
        public IEnumerable<DetailedAvatarInfo2>? CollocationAll { get => collocationAll; set => SetProperty(ref collocationAll, value); }
        public IEnumerable<AvatarInfo>? Collocation11 { get => collocation11; set => SetProperty(ref collocation11, value); }
        public int TotalSubmitted { get => totalSubmitted; set => SetProperty(ref totalSubmitted, value); }
        public int AbyssPassed { get => abyssPassed; set => SetProperty(ref abyssPassed, value); }

        public bool IsInitialized => isInitialized;
        private bool isInitialized = false;
        public async Task InitializeAsync()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;

            await Task.Delay(1000);

            AvatarDictionary = await database.GetAllAvatarAsync();
            CollocationAll = (await database.GetCollocationRankOfFinalAsync())?
                .Select(col => col.ToChild<DetailedAvatarInfo, DetailedAvatarInfo2>(d => {
                    d.CollocationWeapon = col.CollocationWeapon?.Select(w => w.ToChild<CollocationWeapon, CollocationWeapon2>());
                    d.CollocationAvatar = col.CollocationAvatar?.Select(a => a.ToChild<CollocationAvatar, CollocationAvatar2>());
                }))
                .OrderByDescending(col => col.UpRate);
            Collocation11 = (await database.GetCollocationRankOf11FloorAsync())?
                .OrderByDescending(col => col.UpRate); ;

            Gamers? totalSubmitted = await database.GetTotalSubmittedGamerAsync();
            Gamers? sprialAbyssPassed = await database.GetSprialAbyssPassedGamerAsync();

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

        private void PageLoaded()
        {
            
            
            //var recordService = App.GetService<RecordService>();
            //if (recordService.QueryHistory?.Count > 0)
            //{
            //    //if (recordService.CurrentRecord != null && recordService.CurrentRecord?.UserId != null)
            //    //{
            //    //    QueryAutoSuggestBox.Text = recordService.CurrentRecord?.UserId;
            //    //}
            //    //else if (recordService.QueryHistory.Count > 0)
            //    //{
            //    //    QueryAutoSuggestBox.Text = recordService.QueryHistory.First();
            //    //}
            //}

            //await InitializeAsync();
        }

        #region Recommand
        private List<Recommand>? recommands;
        public List<Recommand>? Recommands { get => recommands; set => SetProperty(ref recommands, value); }

        private List<int> floors = new() { 12, 11, 10, 9 };
        public List<int> Floors { get => floors; set => SetProperty(ref floors, value); }

        private int selectedFloor;
        public int SelectedFloor
        {
            get => selectedFloor; set
            {
                SetProperty(ref selectedFloor, value);
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
            //clear recommands
            Recommands = null;

            List<int[]>? teamDataRaw = await database.GetTeamRankRawDataAsync(SelectedFloor);
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
                : (MetadataViewModel.Instance.Characters?.First(c => c.Name == avatar.CnName));
        }

        #endregion
    }
}
