using DGP.Genshin.Models.MiHoYo;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Exceptions;
using DGP.Snap.Framework.Net.Web.QueryString;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DGP.Genshin.Services
{
    public class GachaStatisticService
    {
        private string logFilePath;
        private const string historyDataFileName = "gachaData.json";
        private readonly string historyDataFile = AppDomain.CurrentDomain.BaseDirectory + historyDataFileName;

        private string gachaLogUrl;
        private string configListUrl;

        private GachaData data = null;
        public GachaData Data 
        { 
            get 
            {
                if (this.data == null)
                    this.RequestAllGachaLogsMergeSave();
                return this.data; 
            } set => this.data = value; 
        }
        private GachaData requestedData;
        private readonly object processing = new object();
        #region network
        private GachaConfigInfo RequestGachaConfigInfo() => Json.GetWebRequestObject<GachaConfigInfo>(this.configListUrl);

        public void RequestAllGachaLogsMergeSave(Action onErrorcallback=null)
        {
            Debug.WriteLine("gacha web request start");
            List<GachaConfigType> pools = this.RequestGachaConfigInfo().Data?.GachaTypeList;
            if (pools != null)
            {
                this.requestedData = new GachaData
                {
                    Types = pools,
                    Url = this.gachaLogUrl,
                };

                Dictionary<string, IEnumerable<GachaLogItem>> dict = new Dictionary<string, IEnumerable<GachaLogItem>>();
                foreach (GachaConfigType type in this.requestedData.Types)
                {
                    dict.Add(type.Key, this.RequestGachaLogsOf(type));
                }
                this.requestedData.GachaLogs = dict;
                //just get uid casually
                this.requestedData.Uid = dict.First().Value.First().Uid;
                this.MergeDataAndSave();
            }
            else
            {
                onErrorcallback.Invoke();
            }

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

        #region local storage
        /// <summary>
        /// merge the requested data to local data
        /// </summary>
        private void MergeDataAndSave()
        {
            lock (this.processing)
            {
                if (this.data == null)
                    this.data = this.requestedData;
                if (this.data == this.requestedData)
                {
                    this.SaveToLocalData();
                    return;
                }

                this.Data.Uid = this.requestedData.Uid;
                this.Data.Types = this.requestedData.Types;
                //遍历每个请求得到的池
                foreach (KeyValuePair<string, IEnumerable<GachaLogItem>> requestedPoolEntry in this.requestedData.GachaLogs)
                {
                    DateTime localTime = this.Data.GachaLogs[requestedPoolEntry.Key].First().Time;
                    IEnumerable<GachaLogItem> newItems = requestedPoolEntry.Value.TakeWhile(item => item.Time > localTime);
                    this.Data.GachaLogs[requestedPoolEntry.Key] = newItems.Concat(this.Data.GachaLogs[requestedPoolEntry.Key]);
                }
                this.SaveToLocalData();
            }
        }
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

        #region export
        public void SaveGachaDataToExcel(string fileName)
        {
            IWorkbook workbook = new XSSFWorkbook();
            
            if (this.data != null)
            {
                lock (this.processing)
                {
                    foreach (GachaConfigType pool in this.data.Types)
                    {
                        ISheet sheet = workbook.CreateSheet(pool.Name);
                        IEnumerable<GachaLogItem> logs = this.data.GachaLogs[pool.Key];
                        //header
                        IRow header = sheet.CreateRow(0);
                        header.CreateCell(0).SetCellValue("时间");
                        header.CreateCell(1).SetCellValue("名称");
                        header.CreateCell(2).SetCellValue("类别");
                        header.CreateCell(3).SetCellValue("星级");
                        //content
                        if (logs.Count() >= 1)
                        {
                            for (int j = 1; j < logs.Count(); j++)
                            {
                                //倒序
                                GachaLogItem item = logs.ElementAt(logs.Count() - j);
                                IRow currentRow = sheet.CreateRow(j);
                                currentRow.CreateCell(0).SetCellValue(item.Time.ToString("yyyy-MM-dd HH:mm"));
                                currentRow.CreateCell(1).SetCellValue(item.Name);
                                currentRow.CreateCell(2).SetCellValue(item.ItemType);
                                currentRow.CreateCell(3).SetCellValue(Int32.Parse(item.Rank));
                            }
                        }
                    }
                }
            }
            if (File.Exists(fileName))
                File.Delete(fileName);
            using FileStream fileStream = File.Create(fileName);
            workbook.Write(fileStream);
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

                //check till the log file end to make sure the url authentication is newest.
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
            if (this.gachaLogUrl == null)
            {
                if (this.data == null || this.data.Url == null) 
                    throw new UrlNotFoundException("日志与记录文件中没有可用的url");
                this.gachaLogUrl = this.data.Url;
            }
            this.configListUrl = this.gachaLogUrl.Replace("getGachaLog?", "getConfigList?");
            this.data.Url = this.gachaLogUrl;
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
