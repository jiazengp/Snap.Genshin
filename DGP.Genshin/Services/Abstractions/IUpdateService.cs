using Octokit;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    /// <summary>
    /// 更新服务
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// 当前App版本
        /// </summary>
        Version? CurrentVersion { get; }

        /// <summary>
        /// 更新API获取的App版本
        /// </summary>
        Version? NewVersion { get; set; }

        /// <summary>
        /// 下载更新包的Url
        /// </summary>
        Uri? PackageUri { get; set; }

        /// <summary>
        /// 发行版
        /// </summary>
        Release? Release { get; set; }

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
