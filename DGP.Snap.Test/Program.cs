using DGP.Genshin.Models.MiHoYo;
using DGP.Snap.Framework.Net.Web.QueryString;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DGP.Snap.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var li = GachaStatisticService.Instance.GetGachaConfigInfo().Data.GachaTypeList;
            foreach (GachaConfigType type in li)
            {
                Debug.WriteLine(type.Name + type.Key);
                var newli = GachaStatisticService.Instance.GetGachaLogsOf(type);
                foreach (var item in newli)
                {
                    //Debug.WriteLine(item.Name);
                }
            }
        }
    }

    public class GachaStatisticService
    {
        private string logFilePath;

        private string gachaLogUrl;
        private string configListUrl;

        public GachaConfigInfo GetGachaConfigInfo() => Json.GetWebRequestObject<GachaConfigInfo>(this.configListUrl);

        public IEnumerable<GachaLogItem> GetGachaLogsOf(GachaConfigType type)
        {
            //modify the url
            string baseUrl = this.gachaLogUrl.Substring(0, this.gachaLogUrl.IndexOf('?') + 1);
            string queryUrl = this.gachaLogUrl.Substring(this.gachaLogUrl.IndexOf('?') + 1);
            QueryString queryString = QueryString.Parse(queryUrl);
            queryString.Set("gacha_type", type.Key);
            queryString.Set("size", "20");
            queryString.Set("page", "0");
            queryString.Set("lang", "zh-cn");


            List<GachaLogItem> result = new List<GachaLogItem>();
            GachaLogInfo tmpinfo;
            do
            {
                queryString.Set("page", (Int32.Parse(queryString["page"]) + 1).ToString());
                string finalUrl = baseUrl + queryString;
                Debug.WriteLine(queryString["page"]);
                tmpinfo = Json.GetWebRequestObject<GachaLogInfo>(finalUrl);
                foreach (GachaLogItem item in tmpinfo.Data.List)
                    yield return item;
            } while (tmpinfo.Data.List.Count == 20);
        }

        public void GetUrlInLocalLowLogFile()
        {

        }

        #region 单例
        private static GachaStatisticService instance;
        private static readonly object _lock = new object();
        private GachaStatisticService()
        {
            this.logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\miHoYo\原神\output_log.txt";
            using (StreamReader sr = new StreamReader(this.logFilePath))
            {
                string str;

                //we need to check till the log file end to make sure the url authentication is correct.
                while (sr.Peek() >= 0)
                {
                    str = sr.ReadLine();
                    if (str.StartsWith("OnGetWebViewPageFinish") && str.EndsWith("#/log"))
                    {
                        str = str.Replace("OnGetWebViewPageFinish:", "");
                        string[] splitedUrl = str.Split('?');
                        splitedUrl[0] = "https://hk4e-api.mihoyo.com/event/gacha_info/api/getGachaLog";
                        this.gachaLogUrl = String.Join("?", splitedUrl).Replace("#/log", "");
                    }
                }
            }
            if (this.gachaLogUrl != null)
                this.configListUrl = this.gachaLogUrl.Replace("getGachaLog?", "getConfigList?");
            else
                throw new Exception("日志中没有url");
        }
        public static GachaStatisticService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new GachaStatisticService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
