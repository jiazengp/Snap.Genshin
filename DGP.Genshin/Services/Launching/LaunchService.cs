using DGP.Genshin.Services.Settings;
using DGP.Genshin.Common.Data.Behavior;
using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DGP.Genshin.Services.Launching
{
    /// <summary>
    /// 游戏启动服务
    /// </summary>
    public class LaunchService : Observable
    {
        #region Observable
        private List<LaunchScheme> knownSchemes = new()
        {
            new LaunchScheme("官服 | 天空岛", "1", "mihoyo", "1"),
            new LaunchScheme("B服 | 世界树", "14", "bilibili", "0")
        };
        private LaunchScheme currentScheme;
        private bool isBorderless;

        /// <summary>
        /// 已知的启动方案
        /// </summary>
        public List<LaunchScheme> KnownSchemes { get => knownSchemes; set => Set(ref knownSchemes, value); }

        /// <summary>
        /// 当前启动方案
        /// </summary>
        public LaunchScheme CurrentScheme
        {
            get => currentScheme; set
            {
                Set(ref currentScheme, value);
                OnSchemeChanged();
            }
        }

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
        public bool IsBorderless
        {
            get => isBorderless; set
            {
                Set(ref isBorderless, value);
                SettingService.Instance[Setting.IsBorderless] = value;
            }
        }
        #endregion

        private readonly IniData launcherConfig;
        private readonly IniData gameConfig;

        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <param name="scheme">配置方案</param>
        /// <param name="failAction">启动失败调用</param>
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
        /// 获取转义后的原游戏目录
        /// 因为配置中的游戏目录若包含中文会转义为 \xaaaa 形态
        /// </summary>
        /// <returns></returns>
        private string GetUnescapedGameFolder()
        {
            string gameInstallPath = launcherConfig["launcher"]["game_install_path"];
            return Regex.Unescape(gameInstallPath.Replace(@"\x", @"\u"));
        }

        #region 单例
        private static LaunchService? instance;
        private static readonly object _lock = new();
        private LaunchService()
        {
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
            //cause we don't wanna trigger the save func
            OnPropertyChanged(nameof(CurrentScheme));
        }

        public static LaunchService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new LaunchService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
