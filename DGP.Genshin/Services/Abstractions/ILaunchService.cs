using DGP.Genshin.DataModels.Launching;
using IniParser.Model;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface ILaunchService
    {
        IniData GameConfig { get; }
        IniData LauncherConfig { get; }

        void Launch(LaunchScheme? scheme, Action<Exception> failAction, bool isBorderless, bool isFullScreen);
        bool LoadIniData(string? launcherPath);
        void OpenOfficialLauncher(Action<Exception>? failAction);
        void SaveLaunchScheme(LaunchScheme? scheme);
        string? SelectLaunchDirectoryIfNull(string? launcherPath);
        Task WaitGenshinImpactExitAsync();
    }
}
