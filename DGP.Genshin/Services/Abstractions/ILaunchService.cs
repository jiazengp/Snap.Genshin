using DGP.Genshin.DataModels.Launching;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    /// <summary>
    /// 游戏启动服务
    /// </summary>
    public interface ILaunchService
    {
        /// <summary>
        /// 游戏配置文件
        /// </summary>
        IniData GameConfig { get; }

        /// <summary>
        /// 启动器配置文件
        /// </summary>
        IniData LauncherConfig { get; }

        /// <summary>
        /// 异步启动游戏
        /// </summary>
        /// <param name="scheme">启动方案</param>
        /// <param name="failAction">启动失败回调</param>
        /// <param name="isBorderless">是否启用无边框</param>
        /// <param name="isFullScreen">是否启用全屏</param>
        /// <param name="waitForExit">是否应等待游戏进程退出</param>
        Task LaunchAsync(LaunchScheme? scheme, Action<Exception> failAction, bool isBorderless, bool isFullScreen, bool waitForExit = false);

        /// <summary>
        /// 加载配置文件数据
        /// </summary>
        /// <param name="launcherPath">启动器路径</param>
        /// <returns>是否加载成功</returns>
        bool TryLoadIniData(string? launcherPath);

        /// <summary>
        /// 启动官方启动器
        /// </summary>
        /// <param name="failAction">启动失败回调</param>
        void OpenOfficialLauncher(Action<Exception>? failAction);

        /// <summary>
        /// 保存启动方案到配置文件
        /// </summary>
        /// <param name="scheme">启动方案</param>
        void SaveLaunchScheme(LaunchScheme? scheme);

        /// <summary>
        /// 选择启动器路径, 若提供有效的路径则直接返回, 否则应使用户选择路径
        /// </summary>
        /// <param name="launcherPath">待检验的启动器路径</param>
        /// <returns>启动器路径</returns>
        string? SelectLaunchDirectoryIfNull(string? launcherPath);

        /// <summary>
        /// 异步等待原神进程退出
        /// </summary>
        Task WaitGenshinImpactExitAsync();
        void SaveAllAccounts(IEnumerable<GenshinAccount> accounts);
        ObservableCollection<GenshinAccount> LoadAllAccount();
        GenshinAccount? GetFromRegistry();
        bool SetToRegistry(GenshinAccount? account);
    }
}
