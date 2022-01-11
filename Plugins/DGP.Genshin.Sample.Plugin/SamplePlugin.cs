using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Services.Abstratcions;

namespace DGP.Genshin.Sample.Plugin
{
    /// <summary>
    /// 插件实例实现
    /// </summary>
    [ImportPage(typeof(SamplePage), "Sample Page", "\uE734")]
    public class SamplePlugin : IPlugin
    {
        private const string IsSamplePluginEnabled = "IsSamplePluginEnabled";

        public string Name => "Sample plugin";

        public string Description =>
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " +
            "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
            "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. " +
            "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        public string Author => "DGP Studio";

        public Version Version => new("0.0.0.1");

        /// <summary>
        /// 可以使用Snap Genshin 内置的设置服务储存
        /// 也可以自己实现储存逻辑
        /// </summary>
        public bool IsEnabled
        {
            get => App.GetService<ISettingService>().GetOrDefault(IsSamplePluginEnabled, false);
            set => App.GetService<ISettingService>()[IsSamplePluginEnabled] = value;
        }
    }
}