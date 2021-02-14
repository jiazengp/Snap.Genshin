using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DGP.Genshin.DataViewer.Services
{
    public class TextMapHashService
    {
        public static string GetMapTextBy(JProperty p, Dictionary<string, string> TextMap)
        {
            if (TextMap != null && TextMap.TryGetValue(p.Value.ToString(), out string result))
                return result.Replace(@"\n", "\n").Replace(@"\r", "\r");
            return "[Not Mapped]" + p.Value.ToString();
        }
        public string WorkingFolderPath { get; set; }

    }
}
