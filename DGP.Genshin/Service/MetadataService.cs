using DGP.Genshin.Service.Abstraction;
using DGP.Genshin.Service.Abstraction.IntegrityCheck;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Data.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DGP.Genshin.Service
{
    /// <inheritdoc cref="IMetadataService"/>
    [Service(typeof(IMetadataService), InjectAs.Transient)]
    internal class MetadataService : IMetadataService
    {
        private const string MetaUrl = "https://resource.snapgenshin.com/Metadata2/meta.json";
        private const string MetaFolder = "Metadata";
        private const string MetaFile = "meta.json";
        private const string MetaUrlFormat = "https://resource.snapgenshin.com/Metadata2/{0}.json";

        /// <inheritdoc/>
        public bool IsMetaPresent
        {
            get => PathContext.FileExists(MetaFolder, MetaFile);
        }

        /// <inheritdoc/>
        public async Task<bool> TryEnsureDataNewestAsync(IProgress<IIntegrityCheckService.IIntegrityCheckState> progress, CancellationToken cancellationToken = default)
        {
            try
            {
                progress.Report(new MetaState(1, 1, "检测元数据版本"));
                Dictionary<string, string>? remoteVersions = await Json.FromWebsiteAsync<Dictionary<string, string>>(MetaUrl, cancellationToken);
                Dictionary<string, string> localVersions = Json.FromFileOrNew<Dictionary<string, string>>(PathContext.Locate(MetaFolder, MetaFile));

                Must.NotNull(remoteVersions!);

                int count = 0;
                foreach ((string file, string remoteVersion) in remoteVersions)
                {
                    if (localVersions.TryGetValue(file, out string? localVersion)
                        && (new Version(remoteVersion) <= new Version(localVersion)))
                    {
                        this.Log($"Skip {file} of version {remoteVersion}.");
                        continue;
                    }

                    this.Log($"Download {file} of version {remoteVersion}.");
                    PathContext.CreateFolderOrIgnore(MetaFolder);
                    await Json.WebsiteToFileAsync(string.Format(MetaUrlFormat, file), PathContext.Locate(MetaFolder, $"{file}.json"), cancellationToken);
                    progress.Report(new MetaState(++count, remoteVersions.Count, $"{file}.json"));
                }

                Json.ToFile(PathContext.Locate(MetaFolder, MetaFile), remoteVersions);
                return true;
            }
            catch (Exception ex)
            {
                this.Log(ex);
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TryEnsureDataNewestAsync(CancellationToken cancellationToken = default)
        {
            Dictionary<string, string>? remoteVersions = await Json.FromWebsiteAsync<Dictionary<string, string>>(MetaUrl, cancellationToken);
            Dictionary<string, string> localVersions = Json.FromFileOrNew<Dictionary<string, string>>(PathContext.Locate(MetaFolder, MetaFile));

            if (remoteVersions is not null)
            {
                foreach ((string file, string remoteVersion) in remoteVersions)
                {
                    if (localVersions.TryGetValue(file, out string? localVersion)
                        && (new Version(remoteVersion) <= new Version(localVersion)))
                    {
                        this.Log($"Skip {file} of version {remoteVersion}.");
                        continue;
                    }

                    this.Log($"Download {file} of version {remoteVersion}.");
                    await Json.WebsiteToFileAsync(string.Format(MetaUrlFormat, file), PathContext.Locate(MetaFolder, $"{file}.json"), cancellationToken);
                }

                Json.ToFile(PathContext.Locate(MetaFolder, MetaFile), remoteVersions);
                return true;
            }
            else
            {
                return false;
            }
        }

        private class MetaState : IIntegrityCheckService.IIntegrityCheckState
        {
            /// <summary>
            /// 构造新的进度实例
            /// </summary>
            /// <param name="count">当前个数</param>
            /// <param name="totalCount">总个数</param>
            /// <param name="ks">当前检查完成的源</param>
            public MetaState(int count, int totalCount, string info)
            {
                CurrentCount = count;
                TotalCount = totalCount;
                Info = info;
            }

            /// <summary>
            /// 当前个数
            /// </summary>
            public int CurrentCount { get; init; }

            /// <summary>
            /// 总个数
            /// </summary>
            public int TotalCount { get; init; }

            /// <summary>
            /// 展示信息
            /// </summary>
            public string? Info { get; init; }
        }
    }
}