using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Common.Extensions.System.Collections.Generic;
using DGP.Genshin.Controls.Infrastructures.CachedImage;
using DGP.Genshin.DataModels;
using DGP.Genshin.DataModels.Characters;
using DGP.Genshin.Messages;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    [Send(typeof(SplashInitializationCompletedMessage))]
    public class SplashViewModel : ObservableObject
    {
        private readonly MetadataViewModel metadataViewModel;
        private readonly ISettingService settingService;
        private readonly ICookieService cookieService;

        private bool isCookieVisible = true;
        private bool hasCheckCompleted = false;
        private string currentStateDescription = "校验缓存完整性...";
        private IAsyncRelayCommand initializeCommand;
        private IAsyncRelayCommand setCookieCommand;

        public bool IsCookieVisible
        {
            get => isCookieVisible; set
            {
                SetProperty(ref isCookieVisible, value);
                TrySendCompletedMessage();
            }
        }

        private void TrySendCompletedMessage()
        {
            if (IsCookieVisible == false && HasCheckCompleted)
            {
                this.Log(IsCookieVisible);
                App.Messenger.Send(new SplashInitializationCompletedMessage(this));
            }
        }
        public bool HasCheckCompleted
        {
            get => hasCheckCompleted;
            set
            {
                SetProperty(ref hasCheckCompleted, value);
                TrySendCompletedMessage();
            }
        }

        public string CurrentStateDescription { get => currentStateDescription; set => SetProperty(ref currentStateDescription, value); }
        public IAsyncRelayCommand InitializeCommand
        {
            get => initializeCommand;
            [MemberNotNull(nameof(initializeCommand))]
            set => SetProperty(ref initializeCommand, value);
        }
        public IAsyncRelayCommand SetCookieCommand
        {
            get => setCookieCommand;
            [MemberNotNull(nameof(setCookieCommand))]
            set => SetProperty(ref setCookieCommand, value);
        }

        public SplashViewModel(MetadataViewModel metadataViewModel, ISettingService settingService, ICookieService cookieService)
        {
            this.metadataViewModel = metadataViewModel;
            this.settingService = settingService;
            this.cookieService = cookieService;

            IsCookieVisible = !cookieService.IsCookieAvailable;
            InitializeCommand = new AsyncRelayCommand(CheckAllIntegrityAsync);
            SetCookieCommand = new AsyncRelayCommand(SetCookieAsync);
        }

        private async Task SetCookieAsync()
        {
            await cookieService.SetCookieAsync();
            IsCookieVisible = !cookieService.IsCookieAvailable;
        }

        #region Integrity
        private int currentCount;
        public int CurrentCount { get => currentCount; set => SetProperty(ref currentCount, value); }

        private string? currentInfo;
        public string? CurrentInfo { get => currentInfo; set => SetProperty(ref currentInfo, value); }

        private int? totalCount;
        public int? TotalCount { get => totalCount; set => SetProperty(ref totalCount, value); }

        private double percent;
        public double Percent { get => percent; set => SetProperty(ref percent, value); }

        private int checkingCount;

        public async Task CheckIntegrityAsync<T>(ObservableCollection<T>? collection, IProgress<InitializeState> progress) where T : KeySource
        {
            if (collection is null)
            {
                return;
            }
            //restrict thread count.
            await collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new InitializeState(Interlocked.Increment(ref checkingCount), t.Source?.ToFileName()));
            });
        }

        public async Task CheckCharacterIntegrityAsync(ObservableCollection<Character>? collection, IProgress<InitializeState> progress)
        {
            if (collection is null)
            {
                return;
            }
            //restrict thread count.
            Task sourceTask = collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new InitializeState(Interlocked.Increment(ref checkingCount), t.Source?.ToFileName()));
            });
            Task profileTask = collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new InitializeState(Interlocked.Increment(ref checkingCount), t.Source?.ToFileName()));
            });
            Task gachasplashTask = collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new InitializeState(Interlocked.Increment(ref checkingCount), t.Source?.ToFileName()));
            });
            await Task.WhenAll(sourceTask, profileTask, gachasplashTask);
        }

        private bool hasEverChecked;

        /// <summary>
        /// 检查基础缓存图片完整性，不完整的自动下载补全
        /// 此次启动后若进行过检查则直接跳过
        /// </summary>
        public async Task CheckAllIntegrityAsync()
        {
            if (hasEverChecked)
            {
                return;
            }
            if (settingService.GetOrDefault(Setting.SkipCacheCheck, false))
            {
                this.Log("Integrity Check Suppressed by User Settings");
                HasCheckCompleted = true;
                return;
            }
            this.Log("Integrity Check Start");
            hasEverChecked = true;
            HasCheckCompleted = false;
            Progress<InitializeState> progress = new(i =>
            {
                CurrentCount = i.CurrentCount;
                Percent = (i.CurrentCount * 1D / TotalCount) ?? 0D;
                CurrentInfo = i.Info;
            });
            CurrentCount = 0;
            TotalCount =
                metadataViewModel.Bosses?.Count +
                metadataViewModel.Cities?.Count +
                (metadataViewModel.Characters?.Count * 3) +
                metadataViewModel.DailyTalents?.Count +
                metadataViewModel.DailyWeapons?.Count +
                metadataViewModel.Elements?.Count +
                metadataViewModel.Elites?.Count +
                metadataViewModel.GemStones?.Count +
                metadataViewModel.Locals?.Count +
                metadataViewModel.Monsters?.Count +
                metadataViewModel.Stars?.Count +
                metadataViewModel.Weapons?.Count +
                metadataViewModel.WeeklyTalents?.Count +
                metadataViewModel.WeaponTypes?.Count;

            Task[] integrityTasks =
            {
                CheckIntegrityAsync(metadataViewModel.Bosses, progress),
                CheckCharacterIntegrityAsync(metadataViewModel.Characters, progress),
                CheckIntegrityAsync(metadataViewModel.Cities, progress),
                CheckIntegrityAsync(metadataViewModel.DailyTalents, progress),
                CheckIntegrityAsync(metadataViewModel.DailyWeapons, progress),
                CheckIntegrityAsync(metadataViewModel.Elements, progress),
                CheckIntegrityAsync(metadataViewModel.Elites, progress),
                CheckIntegrityAsync(metadataViewModel.GemStones, progress),
                CheckIntegrityAsync(metadataViewModel.Locals, progress),
                CheckIntegrityAsync(metadataViewModel.Monsters, progress),
                CheckIntegrityAsync(metadataViewModel.Stars, progress),
                CheckIntegrityAsync(metadataViewModel.Weapons, progress),
                CheckIntegrityAsync(metadataViewModel.WeeklyTalents, progress),
                CheckIntegrityAsync(metadataViewModel.WeaponTypes, progress)
            };

            await Task.WhenAll(integrityTasks);
            this.Log("Integrity Check Stop");
            HasCheckCompleted = true;
        }

        public class InitializeState
        {
            public InitializeState(int count, string? info)
            {
                CurrentCount = count;
                Info = info;
            }
            public int CurrentCount { get; set; }
            public string? Info { get; set; }
        }
        #endregion
    }
}
