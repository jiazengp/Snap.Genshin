using DGP.Genshin.Services.Settings;
using DGP.Snap.Framework.Data.Behavior;
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
    public class LaunchService : Observable
    {
        private List<LaunchScheme> knownSchemes = new List<LaunchScheme>
        {
            new LaunchScheme { Name = "官服|天空岛", Channel = "1", CPS = "mihoyo", SubChannel = "1" },
            new LaunchScheme { Name = "B服|世界树", Channel = "14", CPS = "bilibili", SubChannel = "0" }
        };
        private LaunchScheme currentScheme;
        private bool isBorderless;

        public List<LaunchScheme> KnownSchemes { get => this.knownSchemes; set => Set(ref this.knownSchemes, value); }
        public LaunchScheme CurrentScheme
        {
            get => this.currentScheme; set
            {
                Set(ref this.currentScheme, value);
                OnSchemeChanged();
            }
        }
        public bool IsBorderless
        {
            get => this.isBorderless; set
            {
                Set(ref this.isBorderless, value);
                SettingService.Instance[Setting.IsBorderless] = value;
            }
        }

        private void OnSchemeChanged()
        {
            this.gameConfig["General"]["channel"] = this.currentScheme.Channel;
            this.gameConfig["General"]["cps"] = this.currentScheme.CPS;
            this.gameConfig["General"]["sub_channel"] = this.currentScheme.SubChannel;

            string unescapedGameFolder = Regex.Unescape(this.launcherConfig["launcher"]["game_install_path"].Replace("x", "u"));

            new FileIniDataParser().WriteFile($@"{unescapedGameFolder}\config.ini", this.gameConfig);
        }

        private IniData launcherConfig;
        private IniData gameConfig;
        public void Launch(LaunchScheme scheme, Action<Exception> failAction)
        {
            string launcherPath = SettingService.Instance.GetOrDefault<string>(Setting.LauncherPath, null);
            if (launcherPath != null)
            {
                string unescapedGameFolder = this.launcherConfig["launcher"]["game_install_path"].Replace("x", "u");
                string gamePath = $@"{unescapedGameFolder}/{this.launcherConfig["launcher"]["game_start_name"]}";
                gamePath = Regex.Unescape(gamePath);

                try
                {
                    int fullScreenArg = SettingService.Instance.GetOrDefault(Setting.IsBorderless, false) ? 0 : 1;

                    ProcessStartInfo info = new ProcessStartInfo()
                    {
                        FileName = gamePath,
                        Arguments = $"-popupwindow -screen-fullscreen {fullScreenArg}"
                    };
                    Process p = Process.Start(info);
                }
                catch (Exception ex)
                {
                    failAction?.Invoke(ex);
                }
            }
        }

        public void Initialize()
        {
            this.isBorderless = SettingService.Instance.GetOrDefault(Setting.IsBorderless, false);
            OnPropertyChanged(nameof(this.IsBorderless));

            string launcherPath = SettingService.Instance.GetOrDefault<string>(Setting.LauncherPath, null);
            string configPath = $@"{Path.GetDirectoryName(launcherPath)}\config.ini";

            FileIniDataParser launcherParser = new FileIniDataParser();
            this.launcherConfig = launcherParser.ReadFile(configPath);

            string unescapedGameFolder = Regex.Unescape(this.launcherConfig["launcher"]["game_install_path"].Replace("x", "u"));

            FileIniDataParser gameParser = new FileIniDataParser();
            this.gameConfig = gameParser.ReadFile($@"{unescapedGameFolder}\config.ini");

            this.currentScheme = this.KnownSchemes.First(item => item.Channel == this.gameConfig["General"]["channel"]);
            //cause we don't wanna trigger the save func
            OnPropertyChanged(nameof(this.CurrentScheme));
        }

        #region 单例
        private static LaunchService instance;
        private static readonly object _lock = new();
        private LaunchService()
        {
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
