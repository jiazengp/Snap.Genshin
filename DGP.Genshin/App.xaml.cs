using DGP.Genshin.Service;
using DGP.Genshin.Service.Update;
using DGP.Snap.Framework.Core.LifeCycle;
using ModernWpf;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace DGP.Genshin
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            static ApplicationTheme? converter(object n) => n == null ? null : (ApplicationTheme?)(ApplicationTheme)Enum.Parse(typeof(ApplicationTheme), n.ToString());
            ThemeManager.Current.ApplicationTheme = SettingService.Instance.GetOrDefault(Setting.AppTheme, null, converter);
            LifeCycleManager.Instance.InitializeAll();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LifeCycleManager.Instance.UnInitializeAll();
            base.OnExit(e);
        }
    }
}
