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
    public class LocalGachaLogProvider : Observable
    {
        private static readonly string localFolderName = "GachaStatistic";
        public Dictionary<string, GachaData> Data { get; set; } = new Dictionary<string, GachaData>();

        public LocalGachaLogProvider()
        {
            Directory.CreateDirectory(localFolderName);
            var service = GachaStatisticService.Instance;
            foreach (string d in Directory.EnumerateDirectories($@"{localFolderName}"))
            {
                string uid = new DirectoryInfo(d).Name;
                service.Dispatcher.Invoke(() =>
                {
                    if(!service.Uids.Contains(uid))
                        service.Uids.Add(uid);
                });
                this.LoadLogsOf(uid);
            }
            if (service.Uids.Count > 0)
            {
                service.SelectedUid = service.Uids.First();
            }
        }

        private void LoadLogsOf(string uid)
        {
            foreach (string p in Directory.EnumerateFiles($@"{localFolderName}\{uid}"))
            {
                FileInfo fileInfo = new FileInfo(p);
                using StreamReader reader = new StreamReader(fileInfo.OpenRead());
                if(!this.Data.ContainsKey(uid))
                    this.Data.Add(uid, new GachaData());
                this.Data[uid].GachaLogs.Add(fileInfo.Name.Replace(".json", ""), Json.ToObject<List<GachaLogItem>>(reader.ReadToEnd()));
            }
        }
        public void SaveAll()
        {
            foreach (KeyValuePair<string, GachaData> entry in this.Data)
            {
                this.Save(entry.Key);
            }
        }
        public void Save(string uid)
        {
            Directory.CreateDirectory($@"{localFolderName}\{uid}");
            foreach (KeyValuePair<string, List<GachaLogItem>> entry in this.Data[uid].GachaLogs)
            {
                using StreamWriter writer = new StreamWriter(File.Create($@"{localFolderName}\{uid}\{entry.Key}.json"));
                writer.Write(Json.Stringify(entry.Value));
            }
        }
        /// <summary>
        /// 获取最新的时间戳id
        /// </summary>
        /// <returns>default 0</returns>
        public long GetNewestTimeId(ConfigType type, string uid)
        {
            return this.Data.ContainsKey(uid) && this.Data[uid].GachaLogs.ContainsKey(type.Key)
                ? this.Data[uid].GachaLogs[type.Key].OrderByDescending(i => i.TimeId).First().TimeId
                : 0L;
        }

        #region export
        private readonly object processing = new object();
        public void SaveGachaDataToExcel(string fileName)
        {
            IWorkbook workbook = new XSSFWorkbook();

            if (this.Data != null)
            {
                lock (this.processing)
                {
                    foreach (string pool in this.Data[GachaStatisticService.Instance.SelectedUid].GachaLogs.Keys)
                    {
                        ISheet sheet = workbook.CreateSheet(pool);
                        IEnumerable<GachaLogItem> logs = this.Data[GachaStatisticService.Instance.SelectedUid].GachaLogs[pool];
                        //header
                        IRow header = sheet.CreateRow(0);
                        header.CreateCell(0).SetCellValue("时间");
                        header.CreateCell(1).SetCellValue("名称");
                        header.CreateCell(2).SetCellValue("类别");
                        header.CreateCell(3).SetCellValue("星级");
                        //content
                        if (logs.Count() > 0)
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
    }
}
