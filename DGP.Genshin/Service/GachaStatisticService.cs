using DGP.Genshin.Helper;
using DGP.Genshin.Models.MiHoYo;
using DGP.Snap.Framework.Net.Web.QueryString;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DGP.Genshin.Service
{
    public class GachaStatisticService
    {
        private string logFilePath;
        private const string historyDataFileName = "gachaData.json";
        private readonly string historyDataFile = AppDomain.CurrentDomain.BaseDirectory + historyDataFileName;

        private string gachaLogUrl;
        private string configListUrl;

        private GachaData data;
        public GachaData Data 
        { 
            get 
            {
                if (this.data == null)
                    this.RequestAllGachaLogsMergeSave();
                return this.data; 
            } set => this.data = value; 
        }
        private GachaData RequestedData;

        #region network
        private GachaConfigInfo RequestGachaConfigInfo() => Json.GetWebRequestObject<GachaConfigInfo>(this.configListUrl);

        public void RequestAllGachaLogsMergeSave()
        {
            System.Diagnostics.Debug.WriteLine("gacha web request start");
            this.RequestedData = new GachaData
            {
                Types = this.RequestGachaConfigInfo().Data.GachaTypeList,
                Url = gachaLogUrl,
            };

            Dictionary<string, IEnumerable<GachaLogItem>> dict = new Dictionary<string, IEnumerable<GachaLogItem>>();
            foreach (GachaConfigType type in this.RequestedData.Types)
            {
                dict.Add(type.Key, this.RequestGachaLogsOf(type));
            }
            this.RequestedData.GachaLogs = dict;
            //just get uid casually
            this.RequestedData.Uid = dict.First().Value.First().Uid;
            this.MergeData();
        }

        private IEnumerable<GachaLogItem> RequestGachaLogsOf(GachaConfigType type)
        {
            //modify the url ,actually the fragment #/log is not necessary
            string[] splitedUrl = this.gachaLogUrl.Split('?');
            string baseUrl = splitedUrl[0];

            QueryString queryString = QueryString.Parse(splitedUrl[1]);
            queryString.Set("gacha_type", type.Key);
            queryString.Set("size", "20");
            queryString.Set("page", "0");
            queryString.Set("lang", "zh-cn");

            GachaLogInfo tmpinfo;
            do
            {
                queryString.Set("page", (Int32.Parse(queryString["page"]) + 1).ToString());
                string finalUrl = baseUrl + "?" + queryString;

                tmpinfo = Json.GetWebRequestObject<GachaLogInfo>(finalUrl);
                foreach (GachaLogItem item in tmpinfo.Data.List)
                    yield return item;
            } while (tmpinfo.Data.List.Count == 20);
        }
        #endregion

        /// <summary>
        /// invoke after request
        /// </summary>
        private void MergeData()
        {
            if (this.data == null)
            {
                this.data = this.RequestedData;
                this.SaveToLocalData();
                return;
            }

            this.Data.Uid = this.RequestedData.Uid;
            this.Data.Types = this.RequestedData.Types;
            //遍历每个池
            foreach (KeyValuePair<string, IEnumerable<GachaLogItem>> banner in this.RequestedData.GachaLogs)
            {
                //遍历 请求得到的池内物品
                int i = 0;
                GachaLogItem item = this.RequestedData.GachaLogs[banner.Key].ElementAt(i);
                while (!this.RequestedData.GachaLogs[banner.Key].Contains(item))
                {
                    item = this.RequestedData.GachaLogs[banner.Key].ElementAt(i++);
                }
                //前 i 个 item 即为新增项
                this.Data.GachaLogs[banner.Key] = this.RequestedData.GachaLogs[banner.Key].Take(i).Concat(this.Data.GachaLogs[banner.Key]);
            }
            this.SaveToLocalData();
        }

        #region local storage
        public GachaData RetriveLocalData()
        {
            if (!File.Exists(this.historyDataFile))
                return null;
            string json;
            using (StreamReader sr = new StreamReader(this.historyDataFile))
            {
                json = sr.ReadToEnd();
            }
            return Json.ToObject<GachaData>(json);
        }
        public void SaveToLocalData()
        {
            if (!File.Exists(this.historyDataFile))
                File.Create(this.historyDataFile).Dispose();

            string json = Json.Stringify(this.data);
            using StreamWriter sw = new StreamWriter(this.historyDataFile);
            sw.Write(json);
        }
        #endregion

        #region 单例
        private static GachaStatisticService instance;
        private static readonly object _lock = new object();
        private GachaStatisticService()
        {
            this.Data = this.RetriveLocalData();
            this.InitializeUrls();
        }
        private void InitializeUrls()
        {
            this.logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\miHoYo\原神\output_log.txt";
            using (StreamReader sr = new StreamReader(this.logFilePath))
            {
                string str;

                //we need to check till the log file end to make sure the url authentication is correct.
                while (sr.Peek() >= 0)
                {
                    str = sr.ReadLine();
                    if (str.StartsWith("OnGetWebViewPageFinish:") && str.EndsWith("#/log"))
                    {
                        str = str.Replace("OnGetWebViewPageFinish:", "").Replace("#/log", "");
                        string[] splitedUrl = str.Split('?');
                        splitedUrl[0] = "https://hk4e-api.mihoyo.com/event/gacha_info/api/getGachaLog";
                        this.gachaLogUrl = String.Join("?", splitedUrl);
                    }
                }
            }
            if (this.gachaLogUrl != null)
                this.configListUrl = this.gachaLogUrl.Replace("getGachaLog?", "getConfigList?");
            else if (this.data != null)
                this.gachaLogUrl = data.Url;
            else
                throw new Exception("日志与记录文件中没有可用的url");
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
