using DGP.Genshin.DataModel.Update;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Service.Abstraction
{
    /// <summary>
    /// 更新服务
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// 当前App版本
        /// </summary>
        Version CurrentVersion { get; }

        /// <summary>
        /// 更新API获取的App版本
        /// </summary>
        Version? NewVersion { get; }

        /// <summary>
        /// 下载更新包的Url
        /// </summary>
        Uri? PackageUri { get; }

        /// <summary>
        /// 发行日志
        /// </summary>
        string? ReleaseNote { get; }

        /// <summary>
        /// 异步检查更新
        /// </summary>
        /// <returns>更新状态</returns>
        Task<UpdateState> CheckUpdateStateAsync();

        /// <summary>
        /// 下载并安装更新包
        /// 尽量避免在捕获的上下文中使用
        /// </summary>
        Task DownloadAndInstallPackageAsync();
    }

    /// <summary>
    /// 更新检查器接口
    /// </summary>
    public interface IUpdateChecker
    {
        public Task<UpdateInfomation?> GetUpdateInfomationAsync();
    }

    /// <summary>
    /// 更新API
    /// </summary>
    public enum UpdateAPI
    {
        /// <summary>
        /// 使用 Github API 检查更新
        /// </summary>
        GithubAPI = 0,

        /// <summary>
        /// 使用 Patch API 检查更新
        /// </summary>
        PatchAPI = 1,
    }

    /// <summary>
    /// 更新状态枚举
    /// </summary>
    public enum UpdateState
    {
        /// <summary>
        /// 需要更新
        /// </summary>
        NeedUpdate = 0,

        /// <summary>
        /// 最新版本
        /// </summary>
        IsNewestRelease = 1,

        /// <summary>
        /// 内部开发版本
        /// </summary>
        IsInsiderVersion = 2,

        /// <summary>
        /// 更新不可用
        /// </summary>
        NotAvailable = 3
    }
}
