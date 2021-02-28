using DGP.Genshin.DataViewer.Controls.Dialogs;
using DGP.Genshin.DataViewer.Helpers;
using DGP.Genshin.DataViewer.Services;
using DGP.Snap.Framework.Extensions.System.Windows.Threading;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DGP.Genshin.DataViewer.Views
{
    public partial class ExcelSplitView : UserControl, INotifyPropertyChanged
    {
        public ExcelSplitView()
        {
            this.DataContext = this;
            this.InitializeComponent();
            this.VisibilityList.DataContext = this.PresentDataGrid;
            this.SetupMemoryUsageTimer();
        }
        private void SetupMemoryUsageTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => { this.MemoryUsageText.Text = $"内存占用: {Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024} MB"; };
            timer.Start();
        }

        private void OpenFolderRequested(object sender, RoutedEventArgs e)
        {
            WorkingFolderService.SelectWorkingFolder();
            string path = WorkingFolderService.WorkingFolderPath;
            if (path == null)
                return;
            if (!Directory.Exists(path + @"\TextMap\") || !Directory.Exists(path + @"\Excel\"))
            {
                new SelectionSuggestDialog().ShowAsync();
            }
            else
            {
                this.TextMapCollection = DirectoryEx.GetFileExs(path + @"\TextMap\");
                this.ExcelConfigDataCollection = DirectoryEx.GetFileExs(path + @"\Excel\");
                if (this.ExcelConfigDataCollection.Count() == 0)
                {
                    new SelectionSuggestDialog().ShowAsync();
                }
                else
                {
                    //npcid
                    MapService.NPCMap = new Lazy<Dictionary<string, string>>(() =>
                    Json.ToObject<JArray>(
                        this.ExcelConfigDataCollection
                        .First(f => f.FileName == "Npc").Read())
                    .ToDictionary(t => t["Id"].ToString(), v => v["NameTextMapHash"].ToString()));
                }
            }
        }
        private void PaneStateChangeRequested(object sender, RoutedEventArgs e) => this.IsPaneOpen = !this.IsPaneOpen;
        private void OnCurrentCellChanged(object sender, EventArgs e)
        {
            if (Keyboard.FocusedElement is DataGridCell cell)
            {
                if (cell.Content is TextBlock block)
                    this.Readable = block.Text.ToString();
                else
                    Debug.WriteLine(cell.Content);
            }
        }
        private void OnSearchExcelList(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == ModernWpf.Controls.AutoSuggestionBoxTextChangeReason.UserInput)
            {
                this.ExcelConfigDataCollection =
                    this.originalExcelConfigDataCollection.Where(i => i.FileName.ToLower().Contains(sender.Text.ToLower()));
            }
        }
        private void OnSearchTextMap(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == ModernWpf.Controls.AutoSuggestionBoxTextChangeReason.UserInput)
                sender.ItemsSource = MapService.TextMap.Where(i => i.Key.Contains(sender.Text) || i.Value.Contains(sender.Text));
        }

        #region Update dataview
        private async void SetPresentDataViewAsync(FileEx value)
        {
            this.BackgroundIndicatorVisibility = Visibility.Visible;
            await Task.Run(() =>
            {
                JArray data = Json.ToObject<JArray>(value.Read());
                DataTable table = new DataTable();
                foreach (JObject o in data)
                {
                    this.SetColumns(table, o);
                }

                foreach (JObject o in data)
                {
                    DataRow row = table.NewRow();
                    this.SetRow(row, o);
                    table.Rows.Add(row);
                }
                this.Invoke(() =>
                {
                    this.PresentDataGrid.ItemsSource = table.AsDataView();
                });
            });
            this.BackgroundIndicatorVisibility = Visibility.Collapsed;
        }
        private void SetRow(DataRow row, JObject o, string inObjString = "")
        {
            foreach (JProperty p in o.Properties())
            {
                string columnName = $"{inObjString}{p.Name}";
                if (p.Value.Type == JTokenType.Object)
                    this.SetRow(row, p.Value as JObject, $"{columnName}:");
                else if(p.Value.Type == JTokenType.Array)
                    this.SetRow(row, p.Value as JArray, $"{columnName}:");
                else if (columnName.Contains("TextMapHash") || columnName.Contains("Tips"))
                    row[columnName] = MapService.GetMappedTextBy(p);
                else if (columnName == "TalkRole:Id" || columnName == "NpcID")
                    row[columnName] = MapService.GetMappedNPCBy(p.Value.ToString());
                else
                    row[columnName] = p.Value;
            }
        }
        private void SetRow(DataRow row,JArray a,string parentColumnName = "")
        {
            for (int i = 0; i < a.Count; i++)
            {
                JToken t = a[i];
                string columnName = $"{parentColumnName}{i}";
                if (t.Type == JTokenType.Object)
                    this.SetRow(row, t as JObject, $"{columnName}:");
                else if (t.Type == JTokenType.Array)
                    this.SetRow(row, t as JArray, $"{columnName}:");
                else if (columnName.Contains("TextMapHash") || columnName.Contains("Tips"))
                    row[columnName] = MapService.GetMappedTextBy((t as JValue).Value.ToString());
                else if (columnName == "TalkRole:Id" || columnName == "NpcID")
                    row[columnName] = MapService.GetMappedNPCBy((t as JValue).Value.ToString());
                else
                    row[columnName] = (t as JValue).Value;
            }
        }
        private void SetColumns(DataTable table, JObject o, string parentColumnName = "")
        {
            if (o != null)
            {
                foreach (JProperty p in o.Properties())
                {
                    string columnName = $"{parentColumnName}{p.Name}";
                    if (p.Value.Type == JTokenType.Object)
                        this.SetColumns(table, p.Value as JObject, $"{columnName}:");
                    else if (p.Value.Type == JTokenType.Array)
                        this.SetColumns(table, p.Value as JArray, $"{columnName}:");
                    else if (!table.Columns.Contains(columnName))
                        table.Columns.Add(columnName);
                }
            }
        }
        private void SetColumns(DataTable table,JArray a,string parentColumnName = "")
        {
            if (a != null)
            {
                for (int i = 0; i < a.Count; i++)
                {
                    JToken t = a[i];
                    string columnName= $"{parentColumnName}{i}";
                    if (t.Type == JTokenType.Object)
                        this.SetColumns(table, t as JObject, $"{columnName}:");
                    else if (t.Type == JTokenType.Array)
                        this.SetColumns(table, t as JArray, $"{columnName}:");
                    else if (!table.Columns.Contains(columnName))
                            table.Columns.Add(columnName);
                }
            }
        }
        #endregion

        #region Property
        //复选框用TextMap枚举
        #region TextMapCollection
        private IEnumerable<FileEx> textMapCollection;
        public IEnumerable<FileEx> TextMapCollection
        {
            get => this.textMapCollection; set => this.Set(ref this.textMapCollection, value);
        }
        #endregion
        //后台用TextMap
        #region SelectedTextMap
        private FileEx selectedTextMap;
        public FileEx SelectedTextMap
        {
            get => this.selectedTextMap; set
            {
                this.Set(ref this.selectedTextMap, value);
                MapService.TextMap = Json.ToObject<Dictionary<string, string>>(this.selectedTextMap.Read());
            }
        }
        #endregion
        //左侧列表用ExcelConfigData枚举
        #region ExcelConfigDataCollection
        private IEnumerable<FileEx> originalExcelConfigDataCollection = new List<FileEx>();

        private IEnumerable<FileEx> excelConfigDataCollection = new List<FileEx>();
        public IEnumerable<FileEx> ExcelConfigDataCollection
        {
            get => this.excelConfigDataCollection; set
            {
                //search support
                if (this.originalExcelConfigDataCollection.Count() <= value.Count())
                    this.originalExcelConfigDataCollection = value;
                this.Set(ref this.excelConfigDataCollection, value);
            }
        }
        #endregion
        //后台用ExcelConfigData
        #region SelectedFile
        private FileEx selectedFile;
        public FileEx SelectedFile
        {
            get => this.selectedFile; set
            {
                if (value != null)
                {
                    this.TitleText.Text = value.FullFileName;
                    this.IsPaneOpen = false;
                    this.SetPresentDataViewAsync(value);
                    this.Set(ref this.selectedFile, value);
                }
                //TO DO:reselect the correct item in list
            }
        }
        #endregion
        //SplitView用pane状态
        #region IsPaneOpen
        private bool isPaneOpen = false;
        public bool IsPaneOpen
        {
            get => this.isPaneOpen; set => this.Set(ref this.isPaneOpen, value);
        }
        #endregion
        //后台任务指示
        #region BackgroundIndicatorVisibility
        private Visibility backgroundIndicatorVisibility = Visibility.Collapsed;
        public Visibility BackgroundIndicatorVisibility { get => this.backgroundIndicatorVisibility; set => this.Set(ref this.backgroundIndicatorVisibility, value); }
        #endregion
        //可读文字
        #region Readable
        private string readable;
        public string Readable { get => this.readable; set => this.Set(ref this.readable, value); }
        #endregion
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
