using DGP.Genshin.DataModel.Launching;
using DGP.Genshin.FPSUnlocking;
using DGP.Genshin.Helper;
using DGP.Genshin.Service.Abstratcion;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using Microsoft.AppCenter.Crashes;
using Microsoft.Win32;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Data.Json;
using Snap.Data.Utility;
using Snap.Exception;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DGP.Genshin.Service
{
    /// <summary>
    /// 启动服务的默认实现
    /// </summary>
    [Service(typeof(ILaunchService), InjectAs.Transient)]
    internal class LaunchService : ILaunchService
    {
        private const string AccountsFileName = "accounts.json";
        private const string LauncherSection = "launcher";
        private const string GameName = "game_start_name";
        private const string GeneralSection = "General";
        private const string Channel = "channel";
        private const string CPS = "cps";
        private const string SubChannel = "sub_channel";
        private const string GameInstallPath = "game_install_path";
        private const string ConfigFileName = "config.ini";
        private const string LauncherExecutable = "launcher.exe";

        private readonly ISettingService settingService;

        /// <summary>
        /// 定义了对注册表的操作
        /// </summary>
        private class GenshinRegistry
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
                object? sdk = Registry.GetValue(GenshinKey, SdkKey, string.Empty);
                object? data = Registry.GetValue(GenshinKey, DataKey, "");

                if (sdk is null || data is null)
                {
                    return null;
                }

                string sdkString = Encoding.UTF8.GetString((byte[])sdk);
                string dataString = Encoding.UTF8.GetString((byte[])data);

                return new GenshinAccount { MihoyoSDK = sdkString, GeneralData = dataString };
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
            PathContext.CreateOrIgnore(AccountsFileName);

            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);

            TryLoadIniData(launcherPath);
        }

        [MemberNotNullWhen(true, nameof(gameConfig)), MemberNotNullWhen(true, nameof(launcherConfig))]
        public bool TryLoadIniData(string? launcherPath)
        {
            if (launcherPath != null)
            {
                try
                {
                    string configPath = Path.Combine(Path.GetDirectoryName(launcherPath)!, "config.ini");
                    launcherConfig = GetIniData(configPath);

                    string unescapedGameFolder = GetUnescapedGameFolderFromLauncherConfig();
                    gameConfig = GetIniData(Path.Combine(unescapedGameFolder, ConfigFileName));
                }
                catch (ParsingException) { return false; }
                catch (ArgumentNullException) { return false; }
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
            FileIniDataParser parser = new();
            parser.Parser.Configuration.AssigmentSpacer = "";
            return parser.ReadFile(file);
        }

        public async Task LaunchAsync(LaunchScheme? scheme, Action<Exception> failAction, LaunchOption option)
        {
            if (scheme is null)
            {
                return;
            }
            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);
            if (launcherPath is not null)
            {
                string unescapedGameFolder = GetUnescapedGameFolderFromLauncherConfig();
                string gamePath = Path.Combine(unescapedGameFolder, LauncherConfig[LauncherSection][GameName]);

                try
                {
                    //https://docs.unity.cn/cn/current/Manual/CommandLineArguments.html
                    string commandLine = new CommandLineBuilder()
                        .AppendIf(option.IsBorderless, "-popupwindow")
                        .Append("-screen-fullscreen", option.IsFullScreen ? 1 : 0)
                        .Build();

                    Process? game = new()
                    {
                        StartInfo = new()
                        {
                            FileName = gamePath,
                            Verb = "runas",
                            WorkingDirectory = Path.GetDirectoryName(gamePath),
                            UseShellExecute = true,
                            Arguments = commandLine
                        }
                    };

                    if (option.UnlockFPS)
                    {
                        Unlocker unlocker = new(game, option.TargetFPS);
                        UnlockResult result = await unlocker.StartProcessAndUnlockAsync();
                        this.Log(result);
                    }
                    else
                    {
                        if (game.Start())
                        {
                            await game.WaitForExitAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    failAction.Invoke(ex);
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
        /// https://stackoverflow.com/questions/70639344/replace-captured-item-in-regular-expression-replace-in-c-sharp
        /// </summary>
        /// <returns></returns>
        private string GetUnescapedGameFolderFromLauncherConfig()
        {
            string gameInstallPath = LauncherConfig[LauncherSection][GameInstallPath];
            string? hex4Result = Regex.Replace(gameInstallPath, @"\\x([0-9a-f]{4})", @"\u$1");
            return Regex.Unescape(hex4Result);
        }

        public string? SelectLaunchDirectoryIfNull(string? launcherPath)
        {
            if (!File.Exists(launcherPath) || Path.GetFileName(launcherPath) != LauncherExecutable)
            {
                OpenFileDialog openFileDialog = new()
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Filter = "启动器或快捷方式|launcher.exe;*.lnk",
                    Title = "选择启动器文件",
                    CheckPathExists = true,
                    DereferenceLinks = true,
                    FileName = LauncherExecutable
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string fileName = openFileDialog.FileName;
                    if (Path.GetFileNameWithoutExtension(fileName) == LauncherSection)
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
            GameConfig[GeneralSection][Channel] = scheme.Channel;
            GameConfig[GeneralSection][CPS] = scheme.CPS;
            GameConfig[GeneralSection][SubChannel] = scheme.SubChannel;

            string unescapedGameFolder = GetUnescapedGameFolderFromLauncherConfig();
            string configFilePath = Path.Combine(unescapedGameFolder, ConfigFileName);
            //new UTF8Encoding(false) compat with https://github.com/DawnFz/GenShin-LauncherDIY
            new FileIniDataParser().WriteFile(configFilePath, GameConfig, new UTF8Encoding(false));
        }

        public ObservableCollection<GenshinAccount> LoadAllAccount()
        {
            return Json.FromFileOrNew<ObservableCollection<GenshinAccount>>(AccountsFileName);
        }

        public void SaveAllAccounts(IEnumerable<GenshinAccount> accounts)
        {
            Json.ToFile(AccountsFileName, accounts);
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
