using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Gacha;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.Extensions.System.Windows.Threading;
using DGP.Snap.Framework.Net.Web.QueryString;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DGP.Genshin.Services.GachaStatistic
{
    public class GachaLogProvider
    {
        private const string gachaLogBaseUrl = "https://hk4e-api.mihoyo.com/event/gacha_info/api/getGachaLog";
        private string logFilePath;

        private string gachaLogUrl;
        private string configListUrl;

        private Config gachaConfig;
        public Config GachaConfig
        {
            get
            {
                if (this.gachaConfig == null)
                    this.gachaConfig = this.GetGachaConfig();
                return this.gachaConfig;
            }
        }

        private LocalGachaLogProvider localGachaLogProvider;
        public LocalGachaLogProvider LocalGachaLogProvider { get => this.localGachaLogProvider; private set => this.localGachaLogProvider = value; }

        public GachaStatisticService Service { get; set; }

        public GachaLogProvider(GachaStatisticService service)
        {
            this.Service = service;
            this.LocalGachaLogProvider = new LocalGachaLogProvider(service);
        }
        public bool TryFindUrlInLogFile()
        {
            string LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            this.logFilePath = LocalPath + @"Low\miHoYo\原神\output_log.txt";
            //share the file to make genshin access it
            //so it doesn't crash when game is running
            using (StreamReader sr = new StreamReader(File.Open(this.logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                string str;
                //check till the log file end to make sure
                while (sr.Peek() >= 0)
                {
                    str = sr.ReadLine();
                    if (str.StartsWith("OnGetWebViewPageFinish:") && str.EndsWith("#/log"))
                    {
                        str = str.Replace("OnGetWebViewPageFinish:", "").Replace("#/log", "");
                        string[] splitedUrl = str.Split('?');
                        splitedUrl[0] = gachaLogBaseUrl;
                        this.gachaLogUrl = String.Join("?", splitedUrl);
                    }
                }
            }
            if (this.gachaLogUrl == null)
            {
                return false;
            }
            else
            {
                this.configListUrl = this.gachaLogUrl.Replace("getGachaLog?", "getConfigList?");
                return true;
            }
        }

        /// <summary>
        /// 获取祈愿池信息
        /// </summary>
        /// <returns></returns>
        private Config GetGachaConfig()
        {
            Response<Config> resp = Json.GetWebResponseObject<Response<Config>>(this.configListUrl);
            return resp.ReturnCode == 0 ? resp.Data : null;
        }

        /// <summary>
        /// 获取祈愿记录增量信息，能改变当前uid
        /// </summary>
        /// <param name="type"></param>
        public void FetchGachaLogIncrement(ConfigType type)
        {
            List<GachaLogItem> result = new List<GachaLogItem>();
            int currentPage = 0;
            long endId = 0;
            do
            {
                if (this.TryGetBatch(out GachaLog gachaLog, type, endId, ++currentPage))
                {
                    foreach (GachaLogItem item in gachaLog.List)
                    {
                        //first time fetch ,no local data
                        if (!this.LocalGachaLogProvider.Data.ContainsKey(item.Uid))
                        {
                            this.LocalGachaLogProvider.CreateEmptyUser(item.Uid);
                            this.Service.Invoke(() => this.Service.Uids.Add(item.Uid));
                            this.Service.SelectedUid = item.Uid;
                        }
                        long time = this.LocalGachaLogProvider.GetNewestTimeId(type, item.Uid);
                        if (item.TimeId > time)
                        {
                            result.Add(item);
                        }
                        else//already done the new item
                        {
                            //works bad with break.
                            this.MergeIncrement(type, result);
                            return;
                        }
                    }

                    if (gachaLog.List.Count < 20)
                    {
                        break;
                    }
                    endId = gachaLog.List.Last().TimeId;
                }
                else
                {
                    //notify user to re open log page
                    break;
                }
            } while (true);
            this.MergeIncrement(type, result);
        }

        private void MergeIncrement(ConfigType type, List<GachaLogItem> increment)
        {
            Dictionary<string, List<GachaLogItem>> dict = this.LocalGachaLogProvider.Data[this.Service.SelectedUid].GachaLogs;
            if (dict.ContainsKey(type.Key))
            {
                List<GachaLogItem> local = dict[type.Key];
                increment.AddRange(local);
            }
            dict[type.Key] = increment;
        }

        public void SaveAllLogs() => this.LocalGachaLogProvider.SaveAllLogs();

        public event Action<FetchProgress> OnFetchProgressed;

        /// <summary>
        /// try to get a segment contains 20 log items
        /// </summary>
        /// <param name="result"></param>
        /// <param name="type"></param>
        /// <param name="endId"></param>
        /// <returns></returns>
        private bool TryGetBatch(out GachaLog result, ConfigType type, long endId, int currentPage)
        {
            this.Log($"try to get batch of {type.Name}with end_id:{endId}");
            OnFetchProgressed?.Invoke(new FetchProgress { Type = type.Name, Page = currentPage });
            //modify the url
            string[] splitedUrl = this.gachaLogUrl.Split('?');
            string baseUrl = splitedUrl[0];

            QueryString query = QueryString.Parse(splitedUrl[1]);
            query.Set("gacha_type", type.Key);
            //20 is the max size the api can return
            query.Set("size", "20");
            query.Set("lang", "zh-cn");
            //page no longer matters.
            query.Set("end_id", endId.ToString());
            string finalUrl = $"{baseUrl}?{query}";

            Response<GachaLog> resp = Json.GetWebResponseObject<Response<GachaLog>>(finalUrl);

            if (resp.ReturnCode == 0)
            {
                if (resp.Data.List.Count > 0)
                {
                    string tmpUid = resp.Data.List.First().Uid;
                    if (this.Service.SelectedUid != tmpUid)
                    {
                        this.Service.SelectedUid = tmpUid;
                    }
                }
                result = resp.Data;
                return true;
            }
            else
            {
                result = null;
                return false;
            }

        }
    }
}
