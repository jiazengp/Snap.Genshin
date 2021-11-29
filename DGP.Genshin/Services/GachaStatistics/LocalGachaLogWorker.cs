using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Services.GachaStatistics.Compatibility;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DGP.Genshin.Services.GachaStatistics
{
    /// <summary>
    /// 本地抽卡记录工作器
    /// </summary>
    public class LocalGachaLogWorker
    {
        /// <summary>
        /// 导出操作锁
        /// </summary>
        private readonly object exporting = new();
        private const string localFolder = "GachaStatistic";
        private const string metadataSheetName = "原始数据";

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
                if (Data[uid] is GachaData user)
                {
                    user[pool] = Json.FromFile<List<GachaLogItem>>(fileInfo);
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


        private object savingGachaLog = new();
        private void SaveLogOf(string uid)
        {
            Directory.CreateDirectory($@"{localFolder}\{uid}");
            foreach (KeyValuePair<string, List<GachaLogItem>?> entry in Data[uid])
            {
                lock (savingGachaLog)
                {
                    Json.ToFile($@"{localFolder}\{uid}\{entry.Key}.json", entry.Value);
                }
            }
        }
        #endregion

        #region import
        public bool ImportFromUIGF(string filePath)
        {
            bool successful = true;
            lock (exporting)
            {
                try
                {
                    using (FileStream fs = File.OpenRead(filePath))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (ExcelPackage package = new(fs))
                        {
                            ExcelWorksheets? sheets = package.Workbook.Worksheets;
                            if (sheets.Any(s => s.Name == metadataSheetName))
                            {
                                ExcelWorksheet metadataSheet = sheets[metadataSheetName]!;
                                
                                int columnIndex = 1;
                                Dictionary<string, int> PropertyColumn = new()
                                {
                                    { "uid", 0 },
                                    { "gacha_type", 0 },
                                    { "item_id", 0 },
                                    { "count", 0 },
                                    { "time", 0 },
                                    { "name", 0 },
                                    { "lang", 0 },
                                    { "item_type", 0 },
                                    { "rank_type", 0 },
                                    { "id", 0 }
                                };
                                //detect column property name
                                while (true)
                                {
                                    string header = metadataSheet.Cells[1,columnIndex].GetValue<string>();
                                    if (header is null)
                                    {
                                        break;
                                    }
                                    PropertyColumn[header] = columnIndex;
                                    columnIndex++;
                                }
                                int row = 2;
                                List<GachaLogItem> gachaLogs = new();
                                //read data
                                while (true)
                                {
                                    this.Log(row);
                                    if (metadataSheet.Cells[row, 1].Value == null)
                                    {
                                        break;
                                    }
                                    GachaLogItem item = new();
                                    //reflection magic here.
                                    foreach (var itemProperty in item.GetType().GetProperties())
                                    {
                                        if (itemProperty.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName is string jsonPropertyName)
                                        {
                                            var targetColumn = PropertyColumn[jsonPropertyName];
                                            if (columnIndex != 0)
                                            {
                                                if (itemProperty.Name == nameof(item.Time))
                                                {
                                                    DateTime value = Convert.ToDateTime(metadataSheet.Cells[row, PropertyColumn[jsonPropertyName]].GetValue<string>());
                                                    itemProperty.SetValue(item, value);
                                                }
                                                else
                                                {
                                                    itemProperty.SetValue(item, metadataSheet.Cells[row, PropertyColumn[jsonPropertyName]].Value);
                                                }
                                            }
                                        }
                                    }
                                    gachaLogs.Add(item);
                                    row++;
                                }

                                ImportableGachaData importData = new();
                                importData.Data = new();
                                foreach (GachaLogItem item in gachaLogs)
                                {
                                    importData.Uid ??= item.Uid;
                                    string? type = item.GachaType;
                                    //refactor 400 type here to redirect list addition
                                    type = type == "400" ? "301" : type;
                                    _ = type ?? throw new UnexceptedNullException("卡池类型不应为 null");
                                    if (!importData.Data.ContainsKey(type))
                                    {
                                        importData.Data.Add(type, new());
                                    }
                                    importData.Data[type]!.Add(item);
                                }
                                foreach(var pair in importData.Data)
                                {
                                    importData.Data[pair.Key] = pair.Value?.OrderByDescending(x => x.Id).ToList();
                                }
                                ImportImportableGachaData(importData);
                            }
                            else
                            {
                                successful = false;
                            }
                        }
                    } 
                    SaveAllLogs();
                }
                catch (Exception ex)
                {
                    this.Log(ex);
                    successful = false;
                }
            }
            return successful;
        }

        public bool ImportFromGenshinGachaExport(string filePath)
        {
            return ImportFromExternalData<GenshinGachaExportFile>(filePath, file =>
            {
                _ = file ?? throw new SnapGenshinInternalException("不正确的祈愿记录文件格式");
                return new ImportableGachaData { Uid = file.Uid, Data = file.Data };
            });
        }

        public bool ImportFromKeqingNiuza(string filePath)
        {
            return ImportFromExternalData<KeqingNiuzaFile>(filePath, file =>
            {
                _ = file ?? throw new SnapGenshinInternalException("不正确的祈愿记录文件格式");
                ImportableGachaData importData = new();
                importData.Data = new();
                foreach (GachaLogItem item in file)
                {
                    importData.Uid ??= item.Uid;
                    string? type = item.GachaType;
                    //refactor 400 type here to prevent 400 list creation
                    type = type == "400" ? "301" : type;
                    _ = type ?? throw new UnexceptedNullException("卡池类型不应为 null");
                    if (!importData.Data.ContainsKey(type))
                    {
                        importData.Data.Add(type, new());
                    }
                    importData.Data[type]!.Insert(0, item);
                }
                return importData;
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
                    SaveAllLogs();
                }
                catch (Exception ex)
                {
                    this.Log(ex);
                    successful = false;
                }
            }
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
                    //is new uid
                    Data.Add(importable.Uid, data);
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
                    //FileStream can't be disposed by ExcelPackage,so we need to dispose it ourselves
                    using (FileStream fs = File.Create(fileName))
                    {
                        using (ExcelPackage package = new(fs))
                        {
                            foreach (string pool in Data[uid].Keys)
                            {
                                ExcelWorksheet sheet = package.Workbook.Worksheets.Add(ConfigType.Known[pool]);
                                IEnumerable<GachaLogItem>? logs = Data[uid][pool];
                                //fix issue with compatibility
                                logs = logs?.Reverse();
                                InitializeGachaLogSheetHeader(sheet);
                                FillSheetWithGachaData(sheet, logs);
                                //freeze the title
                                sheet.View.FreezePanes(2, 1);
                            }

                            AddInterchangeableSheet(package,Data[uid]);

                            package.Workbook.Properties.Title = "祈愿记录";
                            package.Workbook.Properties.Author = "Snap Genshin";
                            package.Save();
                        }
                    }
                }
            }
        }
        private void AddInterchangeableSheet(ExcelPackage package,GachaData data)
        {
            ExcelWorksheet interchangeSheet = package.Workbook.Worksheets.Add(metadataSheetName);
            //header
            InitializeInterchangeSheetHeader(interchangeSheet);
            IOrderedEnumerable<GachaLogItem> combinedLogs = data.SelectMany(x => x.Value ?? new()).OrderBy(x => x.Id);
            FillInterChangeSheet(interchangeSheet, combinedLogs);
        }

        private void InitializeInterchangeSheetHeader(ExcelWorksheet sheet)
        {
            sheet.Cells[1, 1].Value = "uid";
            sheet.Cells[1, 2].Value = "gacha_type";
            sheet.Cells[1, 3].Value = "item_id";
            sheet.Cells[1, 4].Value = "count";
            sheet.Cells[1, 5].Value = "time";
            sheet.Cells[1, 6].Value = "name";
            sheet.Cells[1, 7].Value = "lang";
            sheet.Cells[1, 8].Value = "item_type";
            sheet.Cells[1, 9].Value = "rank_type";
            sheet.Cells[1, 10].Value = "id";
        }

        private void InitializeGachaLogSheetHeader(ExcelWorksheet sheet)
        {
            //header
            sheet.Cells[1, 1].Value = "时间";
            sheet.Cells[1, 2].Value = "名称";
            sheet.Cells[1, 3].Value = "物品类型";
            sheet.Cells[1, 4].Value = "星级";
            sheet.Cells[1, 5].Value = "祈愿类型";
        }

        /// <summary>
        /// 填充单个sheet
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="logs">包含单个 概率共享卡池 的物品的列表</param>
        private void FillSheetWithGachaData(ExcelWorksheet sheet, IEnumerable<GachaLogItem>? logs)
        {
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
                    sheet.Cells[j, 4].Value = int.Parse(item.Rank!);
                    sheet.Cells[j, 5].Value = item.GachaType?.ToString();

                    using (ExcelRange range = sheet.Cells[j, 1, j, 5])
                    {
                        range.Style.Font.Color.SetColor(ToDrawingColor(int.Parse(item.Rank!)));
                    }
                }
            }
            //自适应
            sheet.Cells[1, 1, j, 6].AutoFitColumns(0);
        }

        /// <summary>
        /// 填充单个sheet
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="logs">包含单个 概率共享卡池 的物品的列表</param>
        private void FillInterChangeSheet(ExcelWorksheet sheet, IEnumerable<GachaLogItem>? logs)
        {
            //content
            int? count = logs?.Count();
            int j = 1;
            if (count > 0 && logs is not null)
            {
                foreach (GachaLogItem item in logs)
                {
                    j++;
                    sheet.Cells[j, 1].Value = item.Uid;
                    sheet.Cells[j, 2].Value = item.GachaType;
                    sheet.Cells[j, 3].Value = string.Empty;
                    sheet.Cells[j, 4].Value = item.Count;
                    sheet.Cells[j, 5].Value = item.Time.ToString("yyyy-MM-dd HH:mm:ss");
                    sheet.Cells[j, 6].Value = item.Name;
                    sheet.Cells[j, 7].Value = item.Language;
                    sheet.Cells[j, 8].Value = item.ItemType;
                    sheet.Cells[j, 9].Value = item.Rank;
                    sheet.Cells[j, 10].Value = item.Id;

                    using (ExcelRange range = sheet.Cells[j, 1, j, 10])
                    {
                        range.Style.Font.Color.SetColor(ToDrawingColor(int.Parse(item.Rank!)));
                    }
                }
            }
            //自适应
            sheet.Cells[1, 1, j, 10].AutoFitColumns(0);
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
