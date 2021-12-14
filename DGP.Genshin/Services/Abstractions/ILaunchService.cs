using DGP.Genshin.DataModel.Launching;
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
        void OpenOfficialLauncher(Action<Exception>? failAction);
        void SaveLaunchScheme(LaunchScheme? scheme);
        string? SelectLaunchDirectory(string? launcherPath);
        Task WaitGenshinImpactExitAsync();
    }
}
