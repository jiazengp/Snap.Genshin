namespace DGP.Genshin.Services.Launching
{
    /// <summary>
    /// 启动方案
    /// 官服
    /// [General] channel = 1 cps = mihoyo sub_channel = 1
    /// B服
    /// [General] channel = 14 cps = bilibili sub_channel = 0
    /// </summary>
    public record LaunchScheme
    {
        public LaunchScheme(string name,string channel,string cps,string subChannel)
        {
            Name = name;
            Channel = channel;
            CPS = cps;
            SubChannel = subChannel;
        }

        public string Name { get; set; }
        public string Channel { get; set; }
        public string CPS { get; set; }
        public string SubChannel { get; set; }
    }
}
