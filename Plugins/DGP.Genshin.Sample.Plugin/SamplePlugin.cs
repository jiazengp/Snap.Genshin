using DGP.Genshin.Core.Plugins;
using System;

[assembly: SnapGenshinPlugin]

namespace DGP.Genshin.Sample.Plugin
{
    /// <summary>
    /// 插件实例实现
    /// </summary>
    [ImportPage(typeof(SamplePage), "设计图标集", "\uE734")]
    public class SamplePlugin : IPlugin
    {
        public string Name
        {
            get => "设计图标集";
        }

        public string Description
        {
            get => "本插件用于设计人员查看所有的 Segoe Fluent Icons 字符";
        }

        public string Author
        {
            get => "DGP Studio";
        }

        public Version Version
        {
            get => new("0.0.0.2");
        }

        public bool IsEnabled
        {
            get;
            set;
        }
    }
}