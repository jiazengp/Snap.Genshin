using DGP.Genshin.Models.MiHoYo.Gacha;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DGP.Genshin.Services.GachaStatistic
{
    /// <summary>
    /// 本地抽卡记录提供器
    /// </summary>
    public class LocalGachaLogProvider : Observable
    {
        private static readonly string localFolderName = "GachaStatistic";
        public Dictionary<string, GachaData> Data { get; set; } = new Dictionary<string, GachaData>();

        private GachaStatisticService Service { get; set; }

        public LocalGachaLogProvider(GachaStatisticService service)
        {
            Directory.CreateDirectory(localFolderName);
            this.Service = service;
            this.LoadAllLogs();
            if (service.Uids.Count > 0)
            {
                service.HasNoData = false;
                service.SelectedUid = service.Uids.First();
            }
        }

        public void CreateEmptyUser(string uid) => this.Data.Add(uid, new GachaData());

        /// <summary>
        /// 获取最新的时间戳id
        /// </summary>
        /// <returns>default 0</returns>
        public long GetNewestTimeId(ConfigType type, string uid)
        {
            return this.Data.ContainsKey(uid) && this.Data[uid].ContainsKey(type.Key)
                ? this.Data[uid][type.Key]/*.OrderByDescending(i => i.TimeId)*/.First().TimeId
                : 0L;
        }

        #region load&save
        private void LoadAllLogs()
        {
            foreach (string user in Directory.EnumerateDirectories($@"{localFolderName}"))
            {
                string uid = new DirectoryInfo(user).Name;
                this.Service.AddOrIgnore(uid);
                this.LoadLogOf(uid);
            }
        }
        private void LoadLogOf(string uid)
        {
            this.CreateEmptyUser(uid);
            foreach (string p in Directory.EnumerateFiles($@"{localFolderName}\{uid}"))
            {
                FileInfo fileInfo = new FileInfo(p);
                using StreamReader reader = new StreamReader(fileInfo.OpenRead());
                string typeName = fileInfo.Name.Replace(".json", "");
                this.Data[uid].Add(typeName, Json.ToObject<List<GachaLogItem>>(reader.ReadToEnd()));
            }
        }

        public void SaveAllLogs()
        {
            foreach (KeyValuePair<string, GachaData> entry in this.Data)
            {
                this.SaveLogOf(entry.Key);
            }
        }
        public void SaveLogOf(string uid)
        {
            Directory.CreateDirectory($@"{localFolderName}\{uid}");
            foreach (KeyValuePair<string, List<GachaLogItem>> entry in this.Data[uid])
            {
                using StreamWriter writer = new StreamWriter(File.Create($@"{localFolderName}\{uid}\{entry.Key}.json"));
                writer.Write(Json.Stringify(entry.Value));
            }
        }
        #endregion

        #region export
        private readonly object processing = new object();
        public void SaveLocalGachaDataToExcel(string fileName)
        {
            lock (this.processing)
            {
                IWorkbook workbook = new XSSFWorkbook();
                if (this.Data.ContainsKey(this.Service.SelectedUid))
                {
                    foreach (string pool in this.Data[this.Service.SelectedUid].Keys)
                    {
                        ISheet sheet = workbook.CreateSheet(pool);
                        IEnumerable<GachaLogItem> logs = this.Data[this.Service.SelectedUid][pool];
                        IEnumerable<GachaLogItem> rlogs = logs.Reverse();
                        //header
                        IRow header = sheet.CreateRow(0);
                        header.CreateCell(0).SetCellValue("时间");
                        header.CreateCell(1).SetCellValue("名称");
                        header.CreateCell(2).SetCellValue("类别");
                        header.CreateCell(3).SetCellValue("星级");
                        //content
                        int count = rlogs.Count();
                        if (count > 0)
                        {
                            int j = 0;
                            foreach (GachaLogItem item in rlogs)
                            {
                                IRow currentRow = sheet.CreateRow(++j);
                                currentRow.CreateCell(0).SetCellValue(item.Time.ToString("yyyy-MM-dd HH:mm:ss"));
                                currentRow.CreateCell(1).SetCellValue(item.Name);
                                currentRow.CreateCell(2).SetCellValue(item.ItemType);
                                currentRow.CreateCell(3).SetCellValue(Int32.Parse(item.Rank));
                            }
                        }
                    }
                    using FileStream fileStream = File.Create(fileName);
                    workbook.Write(fileStream);
                }
            }
        }
        #endregion
    }
}
