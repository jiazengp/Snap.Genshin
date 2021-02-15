using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DGP.Genshin.DataViewer.Services
{
    public class MapService
    {
        public string WorkingFolderPath { get; set; }

        public static Dictionary<string, string> TextMap;
        public static Dictionary<string, string> NPCMap;
        public static string GetMappedTextBy(JProperty p)
        {
            return GetMappedTextBy(p.Value.ToString());
        }
        public static string GetMappedTextBy(string str)
        {
            if (TextMap != null && TextMap.TryGetValue(str, out string result))
                return result.Replace(@"\n", "\n").Replace(@"\r", "\r");
            return "[映射失败]" + str;
        }
        public static string GetMappedNPC(string id)
        {
            if (NPCMap != null && NPCMap.TryGetValue(id, out string result))
                return GetMappedTextBy(result);
            return "[映射失败]" + id;
        }

    }
}
