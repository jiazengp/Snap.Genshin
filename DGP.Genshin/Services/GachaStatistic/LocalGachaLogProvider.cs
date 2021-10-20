using DGP.Genshin.Models.MiHoYo.Gacha;
using DGP.Genshin.Models.MiHoYo.Gacha.Compatibility;
using DGP.Genshin.Services.Settings;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Data.Privacy;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.Extensions.System.Windows.Threading;
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
    public class LocalGachaLogProvider
    {
        public readonly object processing = new object();
        private const string localFolderName = "GachaStatistic";
        public Dictionary<string, GachaData> Data { get; set; } = new Dictionary<string, GachaData>();
        private GachaStatisticService Service { get; set; }

        #region Initialization
        public LocalGachaLogProvider(GachaStatisticService service)
        {
            Directory.CreateDirectory(localFolderName);
            this.Service = service;
            this.LoadAllLogs();
            if (service.Uids.Count > 0)
            {
                service.HasNoData = false;
                service.SetSelectedUidSuppressSyncStatistic(service.Uids.First());
            }
            this.Log("initialized");
        }
        private void LoadAllLogs()
        {
            foreach (string user in Directory.EnumerateDirectories($@"{localFolderName}"))
            {
                string uid = new DirectoryInfo(user).Name;
                this.Service.AddOrIgnore(new PrivateString(uid, PrivateString.DefaultMasker, SettingModel.Instance.ShowFullUID));
                this.LoadLogOf(uid);
            }
        }
        private void LoadLogOf(string uid)
        {
            this.InitializeUser(uid);
            foreach (string p in Directory.EnumerateFiles($@"{localFolderName}\{uid}"))
            {
                FileInfo fileInfo = new FileInfo(p);
                string pool = fileInfo.Name.Replace(".json", "");
                this.Data[uid][pool] = Json.FromFile<List<GachaLogItem>>(fileInfo);
            }
        }
        #endregion

        /// <summary>
        /// 将uid与对应的抽卡数据准备就绪
        /// </summary>
        /// <param name="uid">uid</param>
        public void InitializeUser(string uid) => this.Data.Add(uid, new GachaData());

        /// <summary>
        /// 获取最新的时间戳id
        /// </summary>
        /// <returns>default 0</returns>
        public long GetNewestTimeId(ConfigType type, string uid)
        {
            //有uid有卡池记录就读取最新物品的id,否则返回0
            return this.Data.ContainsKey(uid) && this.Data[uid].ContainsKey(type.Key)
                ? this.Data[uid][type.Key].First().TimeId
                : 0L;
        }

        #region save
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
                Json.ToFile($@"{localFolderName}\{uid}\{entry.Key}.json", entry.Value);
            }
        }
        #endregion

        #region import
        public bool ImportFromGenshinGachaExport(string filePath)
        {
            return this.ImportExternalData<GenshinGachaExportFile>(filePath, file => new ImportableGachaData
            {
                Uid = file.Uid,
                Data = file.Data,
                Types = file.Types
            });
        }

        public bool ImportExternalData<T>(string filePath, Func<T, ImportableGachaData> converter)
        {
            bool successful = true;
            this.Service.CanUserSwitchUid = false;
            lock (this.processing)
            {
                try
                {
                    T file = Json.FromFile<T>(filePath);
                    this.ImportCoreInternal(converter.Invoke(file));
                }
                catch
                {
                    successful = false;
                }
            }
            this.Service.SyncStatisticWithUidAsync();
            this.Service.CanUserSwitchUid = true;
            this.SaveAllLogs();
            return successful;
        }

        private void ImportCoreInternal(ImportableGachaData importable)
        {
            GachaData data = importable.Data;
            //is new uid
            if (App.Current.Invoke(() => this.Service.SwitchUidContext(importable.Uid)))
            {
                this.Data[importable.Uid] = data;
            }
            else//we need to perform merge operation
            {
                foreach (KeyValuePair<string, List<GachaLogItem>> pool in data)
                {
                    List<GachaLogItem> backIncrement = this.PickBackIncrement(importable.Uid, pool.Key, pool.Value);
                    this.MergeBackIncrement(pool.Key, backIncrement);
                }
            }
        }

        private List<GachaLogItem> PickBackIncrement(string uid, string poolType, List<GachaLogItem> importList)
        {
            List<GachaLogItem> currentItems = this.Data[uid][poolType];
            if (currentItems.Count > 0)
            {
                //首个比当前最后的物品id早的物品
                long lastTimeId = currentItems.Last().TimeId;
                int index = importList.FindIndex(i => i.TimeId < lastTimeId);
                if (index < 0)
                {
                    return new List<GachaLogItem>();
                }
                //修改了原先的列表
                importList.RemoveRange(0, index);
            }
            return importList;
        }

        /// <summary>
        /// 合并老数据
        /// </summary>
        /// <param name="type">卡池</param>
        /// <param name="backIncrement">增量</param>
        private void MergeBackIncrement(string type, List<GachaLogItem> backIncrement)
        {
            GachaData dict = this.Data[this.Service.SelectedUid.UnMaskedValue];
            if (dict.ContainsKey(type))
            {
                dict[type].AddRange(backIncrement);
            }
            else
            {
                dict[type] = backIncrement;
            }
        }
        #endregion

        #region export
        public void SaveLocalGachaDataToExcel(string fileName)
        {
            lock (this.processing)
            {
                if (this.Service.SelectedUid == null)
                {
                    return;
                }
                if (this.Data.ContainsKey(this.Service.SelectedUid.UnMaskedValue))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (FileStream fs = File.Create(fileName))
                    {
                        using (ExcelPackage package = new ExcelPackage(fs))
                        {
                            foreach (string pool in this.Data[this.Service.SelectedUid.UnMaskedValue].Keys)
                            {
                                ExcelWorksheet sheet = package.Workbook.Worksheets.Add(pool);
                                IEnumerable<GachaLogItem> logs = this.Data[this.Service.SelectedUid.UnMaskedValue][pool];
                                //fix issue with compatibility
                                logs = logs.Reverse();
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
                                            range.Style.Font.Color.SetColor(this.ToDrawingColor(Int32.Parse(item.Rank)));
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
