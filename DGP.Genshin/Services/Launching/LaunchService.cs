using DGP.Genshin.Common.Data.Behavior;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Services.Settings;
using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Launching
{
    /// <summary>
    /// 游戏启动服务
    /// </summary>
    public class LaunchService : Observable
    {
        private const string AccountsFile = "accounts.json";

        #region Observable
        private List<LaunchScheme> knownSchemes = new()
        {
            new LaunchScheme("官服 | 天空岛", "1", "mihoyo", "1"),
            new LaunchScheme("B服 | 世界树", "14", "bilibili", "0")
        };
        private LaunchScheme currentScheme;
        private bool isBorderless;
        private ObservableCollection<GenshinAccount> accounts;
        private GenshinAccount? selectedAccount;

        /// <summary>
        /// 已知的启动方案
        /// </summary>
        public List<LaunchScheme> KnownSchemes { get => knownSchemes; set => Set(ref knownSchemes, value); }

        /// <summary>
        /// 当前启动方案
        /// </summary>
        public LaunchScheme CurrentScheme { get => currentScheme; set { Set(ref currentScheme, value); OnSchemeChanged(); } }

        /// <summary>
        /// 在此处保存更改后的配置
        /// 以便游戏启动后能读取到
        /// </summary>
        private void OnSchemeChanged()
        {
            gameConfig["General"]["channel"] = currentScheme.Channel;
            gameConfig["General"]["cps"] = currentScheme.CPS;
            gameConfig["General"]["sub_channel"] = currentScheme.SubChannel;

            string unescapedGameFolder = Regex.Unescape(launcherConfig["launcher"]["game_install_path"].Replace("x", "u"));

            new FileIniDataParser().WriteFile($@"{unescapedGameFolder}\config.ini", gameConfig);
        }

        /// <summary>
        /// 是否启用无边框窗口模式
        /// </summary>
        public bool IsBorderless { get => isBorderless; set { Set(ref isBorderless, value); SettingService.Instance[Setting.IsBorderless] = value; } }

        public ObservableCollection<GenshinAccount> Accounts { get => accounts; set => Set(ref accounts, value); }

        public GenshinAccount? SelectedAccount { get => selectedAccount; set { Set(ref selectedAccount, value); RegistryInterop.Set(value); } }
        #endregion

        #region Basic Launch
        private readonly IniData launcherConfig;
        private readonly IniData gameConfig;

        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <param name="scheme">配置方案</param>
        /// <param name="failAction">启动失败回调</param>
        public void Launch(LaunchScheme scheme, Action<Exception> failAction)
        {
            string? launcherPath = SettingService.Instance.GetOrDefault<string?>(Setting.LauncherPath, null);
            if (launcherPath is not null)
            {
                string unescapedGameFolder = GetUnescapedGameFolder();
                string gamePath = $@"{unescapedGameFolder}/{launcherConfig["launcher"]["game_start_name"]}";
                gamePath = Regex.Unescape(gamePath);

                try
                {
                    int fullScreenArg = SettingService.Instance.GetOrDefault(Setting.IsBorderless, false) ? 0 : 1;

                    ProcessStartInfo info = new()
                    {
                        FileName = gamePath,
                        Verb = "runas",
                        UseShellExecute = true,
                        Arguments = $"-popupwindow -screen-fullscreen {fullScreenArg}"
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
        /// 还原转义后的原游戏目录
        /// 目录符号应为/
        /// 因为配置中的游戏目录若包含中文会转义为 \xaaaa 形态
        /// </summary>
        /// <returns></returns>
        private string GetUnescapedGameFolder()
        {
            string gameInstallPath = launcherConfig["launcher"]["game_install_path"];
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
        #endregion

        #region Account Switch
        /// <summary>
        /// 从注册表获取当前的账户信息
        /// </summary>
        public async void MatchAccount()
        {
            //注册表内有账号信息
            if(RegistryInterop.Get() is GenshinAccount current)
            {
                GenshinAccount? matched = Accounts.FirstOrDefault(a => a.GeneralData == current.GeneralData && a.MihoyoSDK == current.MihoyoSDK);
                //账号列表内无匹配项
                if (matched is null)
                {
                    //命名
                    current.Name = await new NameDialog { TargetAccount = current }.GetInputAsync();
                    Accounts.Add(current);
                    selectedAccount = current;
                }
                else
                {
                    selectedAccount = matched;
                }
                OnPropertyChanged(nameof(SelectedAccount));
            }
        }
        public void SaveAllAccounts()
        {
            Json.ToFile(AccountsFile, Accounts);
        }
        #endregion

        #region 单例
        private static volatile LaunchService? instance;
        [SuppressMessage("", "IDE0044")]
        private static object _locker = new();
        private LaunchService()
        {
            //prepare accounts
            if (!File.Exists(AccountsFile))
            {
                File.Create(AccountsFile).Dispose();
            }
            accounts = Json.FromFile<ObservableCollection<GenshinAccount>>(AccountsFile) ?? new();
            selectedAccount = accounts.FirstOrDefault();
            //prepare basic launch
            isBorderless = SettingService.Instance.GetOrDefault(Setting.IsBorderless, false);
            OnPropertyChanged(nameof(IsBorderless));

            string? launcherPath = SettingService.Instance.GetOrDefault<string?>(Setting.LauncherPath, null);
            string configPath = $@"{Path.GetDirectoryName(launcherPath)}\config.ini";

            FileIniDataParser launcherParser = new();
            launcherConfig = launcherParser.ReadFile(configPath);

            string unescapedGameFolder = GetUnescapedGameFolder();

            FileIniDataParser gameParser = new();
            gameConfig = gameParser.ReadFile($@"{unescapedGameFolder}\config.ini");

            currentScheme = KnownSchemes.First(item => item.Channel == gameConfig["General"]["channel"]);
            OnPropertyChanged(nameof(CurrentScheme));
        }
        public static LaunchService Instance
        {
            get
            {
                if (instance is null)
                {
                    lock (_locker)
                    {
                        instance ??= new();
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
