using DGP.Genshin.Control.Infrastructure.CachedImage;
using DGP.Genshin.DataModel;
using DGP.Genshin.Service.Abstraction;
using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Reflection;
using Snap.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using IState = DGP.Genshin.Service.Abstraction.IIntegrityCheckService.IIntegrityCheckState;

namespace DGP.Genshin.Service
{
    /// <summary>
    /// 完整性检查服务的默认实现
    /// </summary>
    [Service(typeof(IIntegrityCheckService), InjectAs.Transient)]
    internal class IntegrityCheckService : IIntegrityCheckService
    {
        private readonly ISettingService settingService;
        private readonly MetadataViewModel metadataViewModel;

        public IntegrityCheckService(ISettingService settingService, MetadataViewModel metadataViewModel)
        {
            this.settingService = settingService;
            this.metadataViewModel = metadataViewModel;
        }

        /// <summary>
        /// 累计检查的个数
        /// </summary>
        private int cumulatedCount;

        public bool IntegrityCheckCompleted { get; private set; } = false;

        /// <summary>
        /// 检查单个集合的Source
        /// </summary>
        /// <typeparam name="T">包含的物品类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="totalCount">总个数</param>
        /// <param name="progress">进度</param>
        private async Task CheckIntegrityAsync<T>(IEnumerable<T>? collection, int totalCount, IProgress<IState> progress) where T : KeySource
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
                progress.Report(new IntegrityState(Interlocked.Increment(ref cumulatedCount), totalCount, t));
            });
        }

        /// <summary>
        /// 检查角色集合的Source
        /// </summary>
        /// <param name="collection">角色集合</param>
        /// <param name="totalCount">总个数</param>
        /// <param name="progress">进度</param>
        private async Task CheckCharacterIntegrityAsync(IEnumerable<Character>? collection, int totalCount, IProgress<IState> progress)
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
                progress.Report(new IntegrityState(Interlocked.Increment(ref cumulatedCount), totalCount, t));
            });
            Task profileTask = collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new IntegrityState(Interlocked.Increment(ref cumulatedCount), totalCount, t));
            });
            Task gachasSplashTask = collection.ParallelForEachAsync(async (t) =>
            {
                if (!FileCache.Exists(t.Source))
                {
                    using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                }
                progress.Report(new IntegrityState(Interlocked.Increment(ref cumulatedCount), totalCount, t));
            });
            await Task.WhenAll(sourceTask, profileTask, gachasSplashTask);
        }

        public async Task CheckMetadataIntegrityAsync(Action<IState> progressedCallback)
        {
            this.Log("Integrity Check Start");
            IntegrityCheckCompleted = false;

            if (settingService.GetOrDefault(Setting.SkipCacheCheck, false))
            {
                this.Log("Integrity Check Suppressed by User Settings");
                IntegrityCheckCompleted = true;
                return;
            }

            Progress<IState> progress = new(progressedCallback);
            int totalCount = GetTotalCount(metadataViewModel);
            await Task.WhenAll(BuildIntegrityTasks(metadataViewModel, totalCount, progress));
            this.Log($"Integrity Check Complete with {totalCount} entries");
            IntegrityCheckCompleted = true;
        }

        private int GetTotalCount(MetadataViewModel metadata)
        {
            int totalCount = 0;
            metadata.ForEachPropertyWithAttribute<IntegrityAwareAttribute>((prop, aware) =>
            {
                int count = prop.GetPropertyValueByName<int>("Count");
                totalCount += aware.IsCharacter ? count * 3 : count;
            });
            return totalCount;
        }

        /// <summary>
        /// 构造检查任务
        /// </summary>
        /// <param name="metadata">元数据视图模型</param>
        /// <param name="totalCount">总个数</param>
        /// <param name="progress">进度</param>
        /// <returns>等待执行的检查任务</returns>
        private List<Task> BuildIntegrityTasks(MetadataViewModel metadata, int totalCount, IProgress<IState> progress)
        {
            List<Task> tasks = new();
            metadata.ForEachPropertyInfoWithAttribute<IntegrityAwareAttribute>((propInfo, aware) =>
            {
                if (aware.IsCharacter)
                {
                    IEnumerable<Character> characters = metadata.GetPropertyByInfo<IEnumerable<Character>>(propInfo)!;
                    tasks.Add(CheckCharacterIntegrityAsync(characters, totalCount, progress));
                }
                else
                {
                    IEnumerable<KeySource> keySources = metadata.GetPropertyByInfo<IEnumerable<KeySource>>(propInfo)!;
                    tasks.Add(CheckIntegrityAsync(keySources, totalCount, progress));
                }
            });
            return tasks;
        }

        /// <summary>
        /// <inheritdoc cref="IState"/>
        /// </summary>
        public class IntegrityState : IState
        {
            /// <summary>
            /// 构造新的进度实例
            /// </summary>
            /// <param name="count"></param>
            /// <param name="totalCount"></param>
            /// <param name="ks"></param>
            public IntegrityState(int count, int totalCount, KeySource? ks)
            {
                CurrentCount = count;
                TotalCount = totalCount;
                Info = ks?.Source?.ToShortFileName();
            }
            public int CurrentCount { get; set; }
            public int TotalCount { get; set; }
            public string? Info { get; set; }
        }
    }
}
