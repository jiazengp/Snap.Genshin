using DGP.Genshin.Core.Plugins;

namespace DGP.Genshin.Sample.Plugin
{
    public class SamplePlugin : IPlugin
    {
        public string Name => "Smaple plugin";

        public string Description => "Sample plugin description";

        public string Author => "DGP Studio";

        public Version Version => new("0.0.0.1");
    }
}