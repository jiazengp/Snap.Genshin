using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DGP.Genshin.DataViewer.Services
{
    public class MapService
    {
        private const string MapFailedString = "[映射失败]:";

        public static Dictionary<string, string> TextMap;
        public static Lazy<Dictionary<string, string>> NPCMap;
        public static string GetMappedTextBy(JProperty p) => GetMappedTextBy(p.Value.ToString());
        public static string GetMappedTextBy(string str)
        {
            if (TextMap != null && TextMap.TryGetValue(str, out string result))
                return result.Replace(@"\n", "\n").Replace(@"\r", "\r");
            return MapFailedString + str;
        }
        public static string GetMappedNPC(string id)
        {
            if (NPCMap != null && NPCMap.Value.TryGetValue(id, out string result))
                return GetMappedTextBy(result);
            return MapFailedString + id;
        }

    }
}
