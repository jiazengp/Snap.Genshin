using Microsoft;

namespace DGP.Genshin.DataModel.Launching
{
    /// <summary>
    /// 启动方案
    /// </summary>
    public record LaunchScheme
    {
        public LaunchScheme(string name, string channel, string cps, string subChannel)
        {
            this.Name = name;
            this.Channel = channel;
            this.CPS = cps;
            this.SubChannel = subChannel;
        }

        public string Name { get; set; }
        public string Channel { get; set; }
        public string CPS { get; set; }
        public string SubChannel { get; set; }

        public SchemeType GetSchemeType()
        {
            return (this.Channel, this.SubChannel) switch
            {
                ("1", "1") => SchemeType.Officical,
                ("14", "0") => SchemeType.Bilibili,
                ("1", "0") => SchemeType.Mihoyo,
                (_, _) => throw Assumes.NotReachable()
            };
        }
    }
}
