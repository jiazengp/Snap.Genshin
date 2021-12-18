using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Common.Extensions.System.Collections.Generic;
using DGP.Genshin.Controls.Infrastructures.CachedImage;
using DGP.Genshin.DataModels;
using DGP.Genshin.DataModels.Characters;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{

    public class IntegrityCheckService : IIntegrityCheckService
    {
        private readonly ISettingService settingService;
        private readonly MetadataViewModel metadataViewModel;
        public IntegrityCheckService(ISettingService settingService, MetadataViewModel metadataViewModel)
        {
            this.settingService = settingService;
            this.metadataViewModel = metadataViewModel;
        }

        private int currentCheckingCount;

        public bool IntegrityCheckCompleted { get; private set; } = false;

        public async Task CheckIntegrityAsync<T>(ObservableCollection<T>? collection, int totalCount, 
            IProgress<IIntegrityCheckService.IIntegrityState> progress) where T : KeySource
        {
            if (collection is null)
            {
                return;
            }
            await collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new IntegrityState(Interlocked.Increment(ref currentCheckingCount), totalCount, t.Source?.ToFileName()));
            });
        }
        public async Task CheckCharacterIntegrityAsync(ObservableCollection<Character>? collection, int totalCount, 
            IProgress<IIntegrityCheckService.IIntegrityState> progress)
        {
            if (collection is null)
            {
                return;
            }
            Task sourceTask = collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new IntegrityState(Interlocked.Increment(ref currentCheckingCount), totalCount, t.Source?.ToFileName()));
            });
            Task profileTask = collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new IntegrityState(Interlocked.Increment(ref currentCheckingCount), totalCount, t.Source?.ToFileName()));
            });
            Task gachasSplashTask = collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new IntegrityState(Interlocked.Increment(ref currentCheckingCount), totalCount, t.Source?.ToFileName()));
            });
            await Task.WhenAll(sourceTask, profileTask, gachasSplashTask);
        }
        /// <summary>
        /// 检查基础缓存图片完整性，不完整的自动下载补全
        /// 此次启动后若进行过检查则直接跳过
        /// </summary>
        public async Task CheckAllIntegrityAsync(Action<IIntegrityCheckService.IIntegrityState> progressedCallback)
        {
            if (settingService.GetOrDefault(Setting.SkipCacheCheck, false))
            {
                this.Log("Integrity Check Suppressed by User Settings");
                IntegrityCheckCompleted = true;
                return;
            }
            this.Log("Integrity Check Start");
            IntegrityCheckCompleted = false;
            
            Progress<IIntegrityCheckService.IIntegrityState> progress = new(progressedCallback);

            int totalCount =
                metadataViewModel.Bosses!.Count +
                metadataViewModel.Cities!.Count +
                metadataViewModel.Characters!.Count * 3 +
                metadataViewModel.DailyTalents!.Count +
                metadataViewModel.DailyWeapons!.Count +
                metadataViewModel.Elements!.Count +
                metadataViewModel.Elites!.Count +
                metadataViewModel.GemStones!.Count +
                metadataViewModel.Locals!.Count +
                metadataViewModel.Monsters!.Count +
                metadataViewModel.Stars!.Count +
                metadataViewModel.Weapons!.Count +
                metadataViewModel.WeeklyTalents!.Count +
                metadataViewModel.WeaponTypes!.Count;

            Task[] integrityTasks =
            {
                CheckIntegrityAsync(metadataViewModel.Bosses, totalCount, progress),
                CheckCharacterIntegrityAsync(metadataViewModel.Characters, totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.Cities, totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.DailyTalents,totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.DailyWeapons,totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.Elements,totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.Elites,totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.GemStones,totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.Locals,totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.Monsters,totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.Stars,totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.Weapons,totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.WeeklyTalents,totalCount, progress),
                CheckIntegrityAsync(metadataViewModel.WeaponTypes,totalCount, progress)
            };

            await Task.WhenAll(integrityTasks);
            this.Log("Integrity Check Complete");
            IntegrityCheckCompleted = true;
        }


        /// <summary>
        /// 为完整性检查进度提供包装
        /// </summary>
        public class IntegrityState:IIntegrityCheckService.IIntegrityState
        {
            public IntegrityState(int count, int totalCount, string? info)
            {
                CurrentCount = count;
                TotalCount = totalCount;
                Info = info;
            }
            public int CurrentCount { get; set; }
            public int TotalCount { get; set; }
            public string? Info { get; set; }
        }
    }

}
