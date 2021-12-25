using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.DataModels.Launching;
using DGP.Genshin.Services.Abstratcions;
using IniParser;
using IniParser.Model;
using Microsoft.AppCenter.Crashes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// 启动服务的默认实现
    /// </summary>
    [Service(typeof(ILaunchService), ServiceType.Transient)]
    internal class LaunchService : ILaunchService
    {
        private const string AccountsFile = "accounts.json";

        private readonly ISettingService settingService;

        /// <summary>
        /// 定义了对注册表的操作
        /// </summary>
        internal class GenshinRegistry
        {
            private const string GenshinKey = @"HKEY_CURRENT_USER\Software\miHoYo\原神";
            private const string SdkKey = "MIHOYOSDK_ADL_PROD_CN_h3123967166";
            private const string DataKey = "GENERAL_DATA_h2389025596";

            public static bool Set(GenshinAccount? account)
            {
                if (account?.MihoyoSDK is not null && account.GeneralData is not null)
                {
                    Registry.SetValue(GenshinKey, SdkKey, Encoding.UTF8.GetBytes(account.MihoyoSDK));
                    Registry.SetValue(GenshinKey, DataKey, Encoding.UTF8.GetBytes(account.GeneralData));
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 在注册表中获取账号信息
            /// 若不提供命名，则返回的账号仅用于比较，不应存入列表中
            /// </summary>
            /// <param name="accountNamer"></param>
            /// <returns></returns>
            public static GenshinAccount? Get()
            {
                object? sdk = Registry.GetValue(GenshinKey, SdkKey, "");
                object? data = Registry.GetValue(GenshinKey, DataKey, "");

                if (sdk is null || data is null)
                {
                    return null;
                }

                string sdkString = Encoding.UTF8.GetString((byte[])sdk);
                string dataString = Encoding.UTF8.GetString((byte[])data);

                return new GenshinAccount { MihoyoSDK = sdkString, GeneralData = dataString };
            }

            /// <summary>
            /// 在注册表中获取账号信息
            /// 若不提供命名，则返回的账号仅用于比较，不应存入列表中
            /// </summary>
            /// <param name="accountNamer"></param>
            /// <returns></returns>
            public static async Task<GenshinAccount?> GetAsync(Func<GenshinAccount, Task<string?>> asyncAccountNamer)
            {
                object? sdk = Registry.GetValue(GenshinKey, SdkKey, null);
                object? data = Registry.GetValue(GenshinKey, DataKey, null);

                if (sdk is null || data is null)
                {
                    return null;
                }

                string sdkString = Encoding.UTF8.GetString((byte[])sdk);
                string dataString = Encoding.UTF8.GetString((byte[])data);

                GenshinAccount account = new() { MihoyoSDK = sdkString, GeneralData = dataString };
                account.Name = await asyncAccountNamer.Invoke(account);
                return account;
            }
        }

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

            TryLoadIniData(launcherPath);
        }

        [MemberNotNullWhen(true, nameof(gameConfig))]
        [MemberNotNullWhen(true, nameof(launcherConfig))]
        public bool TryLoadIniData(string? launcherPath)
        {
            if (launcherPath != null)
            {
                try
                {
                    string configPath = $@"{Path.GetDirectoryName(launcherPath)}\config.ini";
                    launcherConfig = GetIniData(configPath);

                    string unescapedGameFolder = GetUnescapedGameFolderFromLauncherConfig();
                    gameConfig = GetIniData(Path.Combine(unescapedGameFolder, "config.ini"));
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                    return false;
                }
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
            //this method cause tons of problems

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

        public ObservableCollection<GenshinAccount> LoadAllAccount()
        {
            return Json.FromFile<ObservableCollection<GenshinAccount>>(AccountsFile) ?? new();
        }

        public void SaveAllAccounts(IEnumerable<GenshinAccount> accounts)
        {
            Json.ToFile(AccountsFile, accounts);
        }

        public GenshinAccount? GetFromRegistry()
        {
            return GenshinRegistry.Get();
        }
        public bool SetToRegistry(GenshinAccount? account)
        {
            return GenshinRegistry.Set(account);
        }
    }
}
