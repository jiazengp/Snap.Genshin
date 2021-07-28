using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DGP.Genshin.Models.MiHoYo.Record.Avatar
{
    internal class Constellation
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
        [JsonProperty("effect")] public string Effect { get; set; }
        public string ProcessedEffect => ProcessStringFormat(this.Effect);
        [JsonProperty("is_actived")] public bool IsActived { get; set; }
        [JsonProperty("pos")] public int Position { get; set; }

        public static string ProcessStringFormat(string s)
        {
            //color
            s = new Regex(@"<color=.*?>").Replace(s, "");
            s = s.Replace("</color>", "");
            //important mark
            s = s.Replace("<i>", "").Replace("</i>", "");
            //apply \n & \r char
            s = s.Replace(@"\n", "\n").Replace(@"\r", "\r");
            return s;
        }
    }
}
