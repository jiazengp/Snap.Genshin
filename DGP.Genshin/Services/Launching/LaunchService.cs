using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.DataModels.Launching;
using DGP.Genshin.Services.Abstratcions;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Launching
{
    /// <summary>
    /// 启动服务的默认实现
    /// </summary>
    [Service(typeof(ILaunchService), ServiceType.Transient)]
    public class LaunchService : ILaunchService
    {
        private const string AccountsFile = "accounts.json";

        private readonly ISettingService settingService;

        private IniData? launcherConfig;
        private IniData? gameConfig;

        public IniData LauncherConfig
        {
            get
            {
                return launcherConfig ?? throw new SnapGenshinInternalException("启动器路径不能为 null");
            }
        }

        public IniData GameConfig
        {
            get
            {
                return gameConfig ?? throw new SnapGenshinInternalException("启动器路径不能为 null");
            }
        }

        public LaunchService(ISettingService settingService)
        {
            this.settingService = settingService;

            FileStream? accountFile = File.Exists(AccountsFile) ? null : File.Create(AccountsFile);
            accountFile?.Dispose();

            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);

            LoadIniData(launcherPath);
        }

        [MemberNotNullWhen(true, nameof(gameConfig))]
        [MemberNotNullWhen(true, nameof(launcherConfig))]
        public bool LoadIniData(string? launcherPath)
        {
            if (launcherPath != null)
            {
                string configPath = $@"{Path.GetDirectoryName(launcherPath)}\config.ini";
                launcherConfig = GetIniData(configPath);

                string unescapedGameFolder = GetUnescapedGameFolderFromLauncherConfig();
                gameConfig = GetIniData(Path.Combine(unescapedGameFolder, "config.ini"));

                return true;
            }
            else
            {
                return false;
            }
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

        public async Task LaunchAsync(LaunchScheme? scheme, Action<Exception> failAction, bool isBorderless, bool isFullScreen, bool waitForExit = false)
        {
            if (scheme is null)
            {
                return;
            }
            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);
            if (launcherPath is not null)
            {
                string unescapedGameFolder = GetUnescapedGameFolderFromLauncherConfig();
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
                    if (waitForExit && p is not null)
                    {
                        await p.WaitForExitAsync();
                    }
                }
                catch (Exception ex)
                {
                    failAction?.Invoke(ex);
                }
            }
        }

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
        private string GetUnescapedGameFolderFromLauncherConfig()
        {
            string gameInstallPath = LauncherConfig["launcher"]["game_install_path"];
            return Regex.Unescape(gameInstallPath.Replace(@"\x", @"\u"));
        }

        public Task WaitGenshinImpactExitAsync()
        {
            Process[] procs = Process.GetProcessesByName("YuanShen");
            if (procs.Any())
            {
                return Task.WhenAll(procs.Select(p => p.WaitForExitAsync()));
            }
            return Task.CompletedTask;
        }

        public string? SelectLaunchDirectoryIfNull(string? launcherPath)
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
