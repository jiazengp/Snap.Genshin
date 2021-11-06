using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Services.GachaStatistics.Compatibility;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DGP.Genshin.Services.GachaStatistics
{
    /// <summary>
    /// 本地抽卡记录提供器
    /// </summary>
    public class LocalGachaLogWorker
    {
        private readonly object exporting = new();
        private const string localFolder = "GachaStatistic";
        public GachaDataCollection Data { get; set; } = new();

        #region Initialization
        /// <summary>
        /// 初始化 <see cref="LocalGachaLogWorker"/>的一个实例，并初始化本地数据
        /// 本地数据在初始化完成后随即可用
        /// </summary>
        /// <param name="data"></param>
        public LocalGachaLogWorker(GachaDataCollection data)
        {
            Data = data;
            Directory.CreateDirectory(localFolder);
            LoadAllLogs();
            this.Log("initialized");
        }
        private void LoadAllLogs()
        {
            foreach (string uidFolder in Directory.EnumerateDirectories($@"{localFolder}"))
            {
                string uid = new DirectoryInfo(uidFolder).Name;
                LoadLogOf(uid);
            }
        }
        private void LoadLogOf(string uid)
        {
            Data.Add(uid, new GachaData());
            foreach (string p in Directory.EnumerateFiles($@"{localFolder}\{uid}"))
            {
                FileInfo fileInfo = new(p);
                string pool = fileInfo.Name.Replace(".json", "");
                if (Data is not null)
                {
                    GachaData? one = Data[uid];
                    if (one is not null)
                    {
                        one[pool] = Json.FromFile<List<GachaLogItem>>(fileInfo);
                    }
                }
            }
        }
        #endregion

        #region save
        public void SaveAllLogs()
        {
            foreach (KeyValuePair<string, GachaData> entry in Data)
            {
                SaveLogOf(entry.Key);
            }
        }
        private void SaveLogOf(string uid)
        {
            Directory.CreateDirectory($@"{localFolder}\{uid}");
            foreach (KeyValuePair<string, List<GachaLogItem>?> entry in Data[uid])
            {
                Json.ToFile($@"{localFolder}\{uid}\{entry.Key}.json", entry.Value);
            }
        }
        #endregion

        #region import
        public bool ImportFromGenshinGachaExport(string filePath)
        {
            return ImportFromExternalData<GenshinGachaExportFile>(filePath, file =>
            {
                return file is null
                    ? throw new InvalidOperationException("不正确的祈愿记录文件格式")
                    : new ImportableGachaData { Uid = file.Uid, Data = file.Data, Types = file.Types };
            });
        }

        public bool ImportFromExternalData<T>(string filePath, Func<T?, ImportableGachaData> converter)
        {
            bool successful = true;
            lock (exporting)
            {
                try
                {
                    T? file = Json.FromFile<T>(filePath);
                    ImportImportableGachaData(converter.Invoke(file));
                }
                catch (Exception ex)
                {
                    this.Log(ex);
                    successful = false;
                }
            }
            SaveAllLogs();
            return successful;
        }

        private void ImportImportableGachaData(ImportableGachaData importable)
        {
            if (importable.Uid is null)
            {
                throw new InvalidOperationException("未提供Uid");
            }

            if (importable.Data is GachaData data)
            {
                //is new uid
                if (Data.ContainsKey(importable.Uid))
                {
                    //we need to perform merge operation
                    foreach (KeyValuePair<string, List<GachaLogItem>?> pool in data)
                    {
                        TrimToBackIncrement(importable.Uid, pool.Key, pool.Value);
                        MergeBackIncrement(importable.Uid, pool.Key, pool.Value ?? new());
                    }
                }
                else
                {
                    Data[importable.Uid] = data;
                }
            }
            else
            {
                throw new InvalidOperationException("提供的数据不包含抽卡记录");
            }

        }

        /// <summary>
        /// 从数据尾部选出需要合并的部分
        /// 并裁剪列表
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="poolType"></param>
        /// <param name="importList"></param>
        /// <returns>经过修改的原列表</returns>
        private void TrimToBackIncrement(string uid, string poolType, List<GachaLogItem>? importList)
        {
            GachaData one = Data[uid];
            List<GachaLogItem>? currentItems = one[poolType];

            if (currentItems?.Count > 0)
            {
                //首个比当前最后的物品id早的物品
                long lastTimeId = currentItems.Last().TimeId;
                int? index = importList?.FindIndex(i => i.TimeId < lastTimeId);
                if (index < 0)
                {
                    return;
                }
                //修改了原先的列表
                if (index is not null)
                {
                    importList?.RemoveRange(0, index.Value);
                }
            }
        }

        /// <summary>
        /// 合并老数据
        /// </summary>
        /// <param name="type">卡池</param>
        /// <param name="backIncrement">增量</param>
        private void MergeBackIncrement(string uid, string type, List<GachaLogItem>? backIncrement)
        {
            GachaData dict = Data[uid];

            if (dict.ContainsKey(type) && backIncrement is not null)
            {
                dict[type]?.AddRange(backIncrement);
            }
            else
            {
                dict[type] = backIncrement;
            }
        }
        #endregion

        #region export
        public void SaveLocalGachaDataToExcel(string uid, string fileName)
        {
            lock (exporting)
            {
                if (Data.ContainsKey(uid))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    //fs cant be disposed by excelpackage,so we need to dispose ourselves
                    using (FileStream fs = File.Create(fileName))
                    {
                        using (ExcelPackage package = new(fs))
                        {
                            foreach (string pool in Data[uid].Keys)
                            {
                                ExcelWorksheet sheet = package.Workbook.Worksheets.Add(pool);
                                IEnumerable<GachaLogItem>? logs = Data[uid][pool];
                                //fix issue with compatibility
                                logs = logs?.Reverse();
                                //header
                                sheet.Cells[1, 1].Value = "时间";
                                sheet.Cells[1, 2].Value = "名称";
                                sheet.Cells[1, 3].Value = "类别";
                                sheet.Cells[1, 4].Value = "星级";
                                //content
                                int? count = logs?.Count();
                                int j = 1;
                                if (count > 0 && logs is not null)
                                {
                                    foreach (GachaLogItem item in logs)
                                    {
                                        j++;
                                        sheet.Cells[j, 1].Value = item.Time.ToString("yyyy-MM-dd HH:mm:ss");
                                        sheet.Cells[j, 2].Value = item.Name;
                                        sheet.Cells[j, 3].Value = item.ItemType;
                                        if (item.Rank is not null)
                                        {
                                            sheet.Cells[j, 4].Value = int.Parse(item.Rank);
                                        }
                                        using (ExcelRange range = sheet.Cells[j, 1, j, 4])
                                        {
                                            if (item.Rank is not null)
                                            {
                                                range.Style.Font.Color.SetColor(ToDrawingColor(int.Parse(item.Rank)));
                                            }
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
