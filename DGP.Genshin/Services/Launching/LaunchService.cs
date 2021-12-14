using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.DataModel.Launching;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.Services.Settings;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Launching
{
    [Service(typeof(ILaunchService),ServiceType.Transient)]
    public class LaunchService : ILaunchService
    {
        private const string AccountsFile = "accounts.json";

        private readonly ISettingService settingService;

        private readonly IniData launcherConfig;
        private readonly IniData gameConfig;

        public IniData LauncherConfig => launcherConfig;
        public IniData GameConfig => gameConfig;

        public LaunchService(ISettingService settingService)
        {
            this.settingService = settingService;

            FileStream? accountFile = File.Exists(AccountsFile) ? null : File.Create(AccountsFile);
            accountFile?.Dispose();

            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);

            string configPath = $@"{Path.GetDirectoryName(launcherPath)}\config.ini";
            launcherConfig = GetIniData(configPath);

            string unescapedGameFolder = GetUnescapedGameFolder();
            gameConfig = GetIniData(Path.Combine(unescapedGameFolder, "config.ini"));
        }

        /// <summary>
        /// 读取 ini 文件
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        private IniData GetIniData(string file)
        {
            FileIniDataParser parser = new();
            parser.Parser.Configuration.AssigmentSpacer = "";
            return parser.ReadFile(file);
        }

        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <param name="scheme">配置方案</param>
        /// <param name="failAction">启动失败回调</param>
        public void Launch(LaunchScheme? scheme, Action<Exception> failAction, bool isBorderless, bool isFullScreen)
        {
            if (scheme is null)
            {
                return;
            }
            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);
            if (launcherPath is not null)
            {
                string unescapedGameFolder = GetUnescapedGameFolder();
                string gamePath = $@"{unescapedGameFolder}/{LauncherConfig["launcher"]["game_start_name"]}";
                gamePath = Regex.Unescape(gamePath);

                try
                {
                    ProcessStartInfo info = new()
                    {
                        FileName = gamePath,
                        Verb = "runas",
                        WorkingDirectory = Path.GetDirectoryName(gamePath),
                        UseShellExecute = true,
                        Arguments = $"{(isBorderless ? "-popupwindow " : "")}-screen-fullscreen {(isFullScreen ? 1 : 0)}"
                    };
                    Process? p = Process.Start(info);
                }
                catch (Exception ex)
                {
                    failAction?.Invoke(ex);
                }
            }
        }

        /// <summary>
        /// 启动官方启动器
        /// </summary>
        /// <param name="failAction"></param>
        public void OpenOfficialLauncher(Action<Exception>? failAction)
        {
            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);
            try
            {
                ProcessStartInfo info = new()
                {
                    FileName = launcherPath,
                    Verb = "runas",
                    UseShellExecute = true,
                };
                Process? p = Process.Start(info);
            }
            catch (Exception ex)
            {
                failAction?.Invoke(ex);
            }
        }

        /// <summary>
        /// 还原转义后的原游戏目录
        /// 目录符号应为/
        /// 因为配置中的游戏目录若包含中文会转义为 \xaaaa 形态
        /// </summary>
        /// <returns></returns>
        private string GetUnescapedGameFolder()
        {
            string gameInstallPath = LauncherConfig["launcher"]["game_install_path"];
            return Regex.Unescape(gameInstallPath.Replace(@"\x", @"\u"));
        }

        /// <summary>
        /// 等待原神进程退出，若找不到原神进程会直接返回
        /// </summary>
        /// <returns></returns>
        public Task WaitGenshinImpactExitAsync()
        {
            Process[] procs = Process.GetProcessesByName("YuanShen");
            if (procs.Any())
            {
                return Task.WhenAll(procs.Select(p => p.WaitForExitAsync()));
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 选择原神启动器的目录
        /// </summary>
        /// <param name="launcherPath"></param>
        /// <returns></returns>
        public string? SelectLaunchDirectory(string? launcherPath)
        {
            if (!File.Exists(launcherPath) || Path.GetFileNameWithoutExtension(launcherPath) != "launcher")
            {
                OpenFileDialog openFileDialog = new()
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Filter = "启动器|launcher.exe|快捷方式|*.lnk",
                    Title = "选择启动器文件",
                    CheckPathExists = true,
                    DereferenceLinks = true,
                    FileName = "launcher.exe"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    string fileName = openFileDialog.FileName;
                    if (Path.GetFileNameWithoutExtension(fileName) == "launcher")
                    {
                        launcherPath = openFileDialog.FileName;
                        settingService[Setting.LauncherPath] = launcherPath;
                    }
                }
            }

            return launcherPath;
        }

        /// <summary>
        /// 保存配置,以便游戏启动后能读取到
        /// </summary>
        public void SaveLaunchScheme(LaunchScheme? scheme)
        {
            if (scheme is null)
            {
                return;
            }
            GameConfig["General"]["channel"] = scheme.Channel;
            GameConfig["General"]["cps"] = scheme.CPS;
            GameConfig["General"]["sub_channel"] = scheme.SubChannel;

            string unescapedGameFolder = Regex.Unescape(LauncherConfig["launcher"]["game_install_path"].Replace("x", "u"));
            //compat with https://github.com/DawnFz/GenShin-LauncherDIY
            new FileIniDataParser().WriteFile($@"{unescapedGameFolder}\config.ini", GameConfig, new UTF8Encoding(false));
        }
    }
}
