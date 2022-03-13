using DGP.Genshin.Control.Infrastructure.CachedImage;
using DGP.Genshin.DataModel;
using DGP.Genshin.Service.Abstraction.IntegrityCheck;
using DGP.Genshin.Service.Abstraction.Setting;
using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Data.Primitive;
using Snap.Reflection;
using Snap.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using IState = DGP.Genshin.Service.Abstraction.IntegrityCheck.IIntegrityCheckService.IIntegrityCheckState;

namespace DGP.Genshin.Service
{
    /// <summary>
    /// 完整性检查服务的默认实现
    /// </summary>
    [Service(typeof(IIntegrityCheckService), InjectAs.Transient)]
    internal class IntegrityCheckService : IIntegrityCheckService
    {
        private readonly MetadataViewModel metadataViewModel;

        public IntegrityCheckService(MetadataViewModel metadataViewModel)
        {
            this.metadataViewModel = metadataViewModel;
        }

        /// <summary>
        /// 累计检查的个数
        /// </summary>
        private int cumulatedCount;

        public WorkWatcher IntegrityChecking { get; } = new(false);

        /// <summary>
        /// 检查单个集合的Source
        /// </summary>
        /// <typeparam name="T">包含的物品类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="totalCount">总个数</param>
        /// <param name="progress">进度</param>
        private async Task CheckIntegrityAsync<T>(IEnumerable<T>? collection, int totalCount, IProgress<IState> progress) where T : KeySource
        {
            if (collection is not null)
            {
                await collection.ParallelForEachAsync(async t =>
                {
                    if (!FileCache.Exists(t.Source))
                    {
                        using MemoryStream? memoryStream = await FileCache.HitAsync(t.Source);
                    }
                    progress.Report(new IntegrityState(++cumulatedCount, totalCount, t));
                });
            }
        }

        public async Task CheckMetadataIntegrityAsync(IProgress<IState> progress)
        {
            this.Log("Integrity Check Start");
            using (IntegrityChecking.Watch())
            {
                if (Setting2.SkipCacheCheck)
                {
                    this.Log("Integrity Check Suppressed by User Settings");
                    return;
                }
                int totalCount = GetTotalCount(metadataViewModel);

                await Task.WhenAll(BuildIntegrityTasks(metadataViewModel, totalCount, progress));
                this.Log($"Integrity Check Complete with {totalCount} entries");
            }
        }

        private int GetTotalCount(MetadataViewModel metadata)
        {
            int totalCount = 0;
            metadata.ForEachPropertyWithAttribute<IntegrityAwareAttribute>((prop, _) =>
            {
                totalCount += prop.GetPropertyValueByName<int>("Count");
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

            metadata.ForEachPropertyWithAttribute<IEnumerable<KeySource>, IntegrityAwareAttribute>((keySources, attr) =>
            {
                tasks.Add(CheckIntegrityAsync(keySources, totalCount, progress));
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
