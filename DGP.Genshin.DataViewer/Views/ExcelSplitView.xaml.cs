using DGP.Genshin.DataViewer.Controls.Dialogs;
using DGP.Genshin.DataViewer.Helpers;
using DGP.Genshin.DataViewer.Services;
using DGP.Snap.Framework.Extensions;
using DGP.Snap.Framework.Extensions.System.Windows;
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
using System.Windows.Documents;
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
            VisibilityList.DataContext = PresentDataGrid;
            SetupMemoryUsageTimer();
        }
        private void SetupMemoryUsageTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => { MemoryUsageText.Text = $"内存占用: {Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024} MB"; };
            timer.Start();
        }

        private void OpenFolderRequested(object sender, RoutedEventArgs e)
        {
            WorkingFolderService.SelectWorkingFolder();
            string path = WorkingFolderService.WorkingFolderPath;
            if (path == null)
                return;
            if (!Directory.Exists(path + @"\TextMap\") || !Directory.Exists(path + @"\Excel\"))
                new SelectionSuggestDialog().ShowAsync();
            else
            {
                this.TextMapCollection = DirectoryEx.GetFileExs(path + @"\TextMap\");
                this.ExcelConfigDataCollection = DirectoryEx.GetFileExs(path + @"\Excel\");
                if (this.ExcelConfigDataCollection.Count() == 0)
                    new SelectionSuggestDialog().ShowAsync();
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
                    Readable = block.Text.ToString();
                else
                    Debug.WriteLine(cell.Content);
            }
        }
        private void OnSearchExcelList(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == ModernWpf.Controls.AutoSuggestionBoxTextChangeReason.UserInput)
                ExcelConfigDataCollection = 
                    originalExcelConfigDataCollection.Where(i => i.FileName.ToLower().Contains(sender.Text.ToLower()));
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
                    SetColumns(table, o);
                }

                foreach (JObject o in data)
                {
                    DataRow row = table.NewRow();
                    SetRow(row, o);
                    table.Rows.Add(row);
                }
                this.Invoke(() =>
                {
                    this.PresentDataGrid.ItemsSource = table.AsDataView();
                });
            });
            this.BackgroundIndicatorVisibility = Visibility.Collapsed;
        }

        private static void SetRow(DataRow row, JObject o, string inObjString = "")
        {
            
            foreach (JProperty p in o.Properties())
            {
                string columnName = $"{inObjString}{p.Name}";
                RemapTextHashByText(p);
                RemapNpcNameByID(p);
                if (p.Value.Type == JTokenType.Object)
                {
                    SetRow(row, p.Value as JObject, $"{columnName}:");
                }
                else if(p.Value.Type == JTokenType.Array)
                {
                    SetRow(row, p.Value as JArray, $"{columnName}:");
                }
                else
                {
                    row[columnName] = p.Value;
                }
            }
        }

        private static void SetRow(DataRow row,JArray a,string parentColumnName = "")
        {
            for (int i = 0; i < a.Count; i++)
            {
                JToken t = a[i];
                string columnName = $"{parentColumnName}{i}";
                if (t.Type == JTokenType.Object)
                {
                    SetRow(row, t as JObject, $"{columnName}:");
                }
                else if (t.Type == JTokenType.Array)
                {
                    SetRow(row, t as JArray, $"{columnName}:");
                }
                else
                {
                    row[columnName] = (t as JValue).Value;
                }
            }
        }

        private static void SetColumns(DataTable table, JObject o, string parentColumnName = "")
        {
            if (o != null)
            {
                foreach (JProperty p in o.Properties())
                {
                    string columnName = $"{parentColumnName}{p.Name}";
                    if (p.Value.Type == JTokenType.Object)
                    {
                        SetColumns(table, p.Value as JObject, $"{columnName}:");
                    }
                    else if (p.Value.Type == JTokenType.Array)
                    {
                        SetColumns(table, p.Value as JArray, $"{columnName}:");
                    }
                    else if (!table.Columns.Contains(columnName))
                    {
                        table.Columns.Add(columnName);
                    }
                }
            }
        }

        private static void SetColumns(DataTable table,JArray a,string parentColumnName = "")
        {
            if (a != null)
            {
                for (int i = 0; i < a.Count; i++)
                {
                    JToken t = a[i];
                    string columnName= $"{parentColumnName}{i}";
                    if (t.Type == JTokenType.Object)
                    {
                        SetColumns(table, t as JObject, $"{columnName}:");
                    }
                    else if (t.Type == JTokenType.Array)
                    {
                        SetColumns(table, t as JArray, $"{columnName}:");
                    }
                    else if (!table.Columns.Contains(columnName))
                            table.Columns.Add(columnName);
                }
            }
        }
        #endregion

        #region Remap
        private static void RemapTextHashByText(JProperty p)
        {
            if (p.Name.Contains("TextMapHash"))
                p.Value = MapService.GetMappedTextBy(p);
        }
        private static void RemapNpcNameByID(JProperty p)
        {
            if (p.Name == "TalkRole"|| p.Name == "NpcID")
            {
                switch (p.Value["Type"]?.ToString())
                {
                    case "TALK_ROLE_NPC":
                        p.Value = MapService.GetMappedNPCBy(p.Value["Id"].ToString());
                        break;
                    case "TALK_ROLE_PLAYER":
                        p.Value = "[!:玩家]";
                        break;
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
                if (originalExcelConfigDataCollection.Count() <= value.Count())
                    originalExcelConfigDataCollection = value;
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
        public string Readable { get => readable; set => Set(ref readable, value); }
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
