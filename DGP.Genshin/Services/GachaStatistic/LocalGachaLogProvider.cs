using DGP.Genshin.Models.MiHoYo.Gacha;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using OfficeOpenXml;
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
            LoadAllLogs();
            if (service.Uids.Count > 0)
            {
                service.HasNoData = false;
                service.SelectedUid = service.Uids.First();
            }
        }

        public void CreateUser(string uid) => this.Data.Add(uid, new GachaData());

        /// <summary>
        /// 获取最新的时间戳id
        /// </summary>
        /// <returns>default 0</returns>
        public long GetNewestTimeId(ConfigType type, string uid)
        {
            return this.Data.ContainsKey(uid) && this.Data[uid].ContainsKey(type.Key)
                ? this.Data[uid][type.Key].First().TimeId
                : 0L;
        }

        #region load&save
        private void LoadAllLogs()
        {
            foreach (string user in Directory.EnumerateDirectories($@"{localFolderName}"))
            {
                string uid = new DirectoryInfo(user).Name;
                this.Service.AddOrIgnore(uid);
                LoadLogOf(uid);
            }
        }
        private void LoadLogOf(string uid)
        {
            CreateUser(uid);
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
                SaveLogOf(entry.Key);
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
                if (this.Service.SelectedUid == null)
                {
                    return;
                }
                if (this.Data.ContainsKey(this.Service.SelectedUid))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (FileStream fs = File.Create(fileName))
                    {
                        using (ExcelPackage package = new ExcelPackage(fs))
                        {
                            foreach (string pool in this.Data[this.Service.SelectedUid].Keys)
                            {
                                ExcelWorksheet sheet = package.Workbook.Worksheets.Add(pool);
                                IEnumerable<GachaLogItem> logs = this.Data[this.Service.SelectedUid][pool];
                                //header
                                sheet.Cells[1, 1].Value = "时间";
                                sheet.Cells[1, 2].Value = "名称";
                                sheet.Cells[1, 3].Value = "类别";
                                sheet.Cells[1, 4].Value = "星级";
                                //content
                                int count = logs.Count();
                                int j = 1;
                                if (count > 0)
                                {
                                    foreach (GachaLogItem item in logs)
                                    {
                                        j++;
                                        sheet.Cells[j, 1].Value = item.Time.ToString("yyyy-MM-dd HH:mm:ss");
                                        sheet.Cells[j, 2].Value = item.Name;
                                        sheet.Cells[j, 3].Value = item.ItemType;
                                        sheet.Cells[j, 4].Value = Int32.Parse(item.Rank);
                                        using (ExcelRange range = sheet.Cells[j, 1, j, 4])
                                        {
                                            range.Style.Font.Color.SetColor(ToDrawingColor(Int32.Parse(item.Rank)));
                                        }
                                    }
                                }
                                sheet.Cells[1, 1, j, 4].AutoFitColumns(0);

                                package.Workbook.Properties.Title = "祈愿记录";
                                package.Workbook.Properties.Author = "Snap Genshin";
                            }
                            package.Save();
                        }
                    }
                }
            }
        }

        public System.Drawing.Color ToDrawingColor(int rank)
        {
            return rank switch
            {
                1 => System.Drawing.Color.FromArgb(255, 114, 119, 139),
                2 => System.Drawing.Color.FromArgb(255, 42, 143, 114),
                3 => System.Drawing.Color.FromArgb(255, 81, 128, 203),
                4 => System.Drawing.Color.FromArgb(255, 161, 86, 224),
                5 => System.Drawing.Color.FromArgb(255, 188, 105, 50),
                _ => throw new ArgumentOutOfRangeException(nameof(rank)),
            };
        }
        #endregion
    }
}
