using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace DGP.Genshin.DataViewer.Services
{
    public class MapService
    {
        public static Dictionary<string, string> TextMap;
        public static Lazy<Dictionary<string, string>> NPCMap;
        public static string GetMappedTextBy(JProperty p) => GetMappedTextBy(p.Value.ToString());
        public static string GetMappedTextBy(string str)
        {
            if (TextMap != null && TextMap.TryGetValue(str, out string result))
                return ProcessStringFormat(result);
            return $"[!Hash:{str}]";
        }
        public static string GetMappedNPCBy(string id)
        {
            if (NPCMap != null && NPCMap.Value.TryGetValue(id, out string result))
                return GetMappedTextBy(result);
            return $"[!Id:{id}]";
        }

        public static string ProcessStringFormat(string s)
        {
            //match the format required string
            if (s.StartsWith("#"))
                s = s.Remove(0, 1);
            //color
            s = new Regex(@"<color=.*?>").Replace(s, "");
            s = s.Replace("</color>", "");
            //important mark
            s = s.Replace("<i>", "").Replace("</i>", "");
            //nickname
            s = s.Replace("{NICKNAME}", "[!:玩家昵称]");
            //apply \n & \r char
            s = s.Replace(@"\n", "\n").Replace(@"\r", "\r");
            return s;
        }

    }
}
