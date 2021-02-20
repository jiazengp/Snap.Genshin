using DGP.Genshin.DataViewer.Helper;
using DGP.Genshin.DataViewer.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.DataViewer.Views
{
    public partial class ExcelSplitView : UserControl, INotifyPropertyChanged
    {
        public ExcelSplitView()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        private void OpenFolderRequested(object sender, RoutedEventArgs e)
        {
            WorkingFolderService.SelectWorkingFolder();
            string path = WorkingFolderService.WorkingFolderPath;
            if (path == null)
                return;
            if (!Directory.Exists(path + @"\TextMap\") || !Directory.Exists(path + @"\Excel\"))
                this.SelectSuggentionDialog.ShowAsync();
            else
            {
                this.TextMapCollection = DirectoryEx.GetFileExs(path + @"\TextMap\");
                this.ExcelConfigDataCollection = DirectoryEx.GetFileExs(path + @"\Excel\");
                if (this.ExcelConfigDataCollection.Count() == 0)
                    this.SelectSuggentionDialog.ShowAsync();
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

        private async void SetPresentDataViewAsync(FileEx value)
        {
            this.BackgroundIndicatorVisibility = Visibility.Visible;
            await Task.Run(() =>
            {
                JArray dataArray = Json.ToObject<JArray>(value.Read());
                DataTable table = new DataTable();
                foreach (JProperty p in from JObject o in dataArray
                                        from p in
                                            from JProperty p in o.Properties()
                                            where !table.Columns.Contains(p.Name)
                                            select p
                                        select p)
                    table.Columns.Add(p.Name);
                foreach (JObject o in dataArray)
                {
                    DataRow row = table.NewRow();
                    foreach (JProperty p in o.Properties())
                    {
                        RemapTextHashByText(p);
                        RemapNpcIDByName(p);

                        row[p.Name] = p.Value;
                    }
                    table.Rows.Add(row);
                }
                this.Dispatcher.Invoke(() => { this.PresentDataGrid.ItemsSource = table.AsDataView(); });
            });
            this.BackgroundIndicatorVisibility = Visibility.Collapsed;
        }
        private static void RemapTextHashByText(JProperty p)
        {
            if (p.Name.Contains("TextMapHash"))
                p.Value = MapService.GetMappedTextBy(p);
        }
        private static void RemapNpcIDByName(JProperty p)
        {
            if (p.Name == "TalkRole")
            {
                switch (p.Value["Type"]?.ToString())
                {
                    case "TALK_ROLE_NPC":
                        p.Value = MapService.GetMappedNPC(p.Value["Id"].ToString());
                        break;
                    case "TALK_ROLE_PLAYER":
                        p.Value = "[玩家]";
                        break;
                }
            }
        }

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
        private IEnumerable<FileEx> excelConfigDataCollection;
        public IEnumerable<FileEx> ExcelConfigDataCollection
        {
            get => this.excelConfigDataCollection; set => this.Set(ref this.excelConfigDataCollection, value);
        }
        #endregion
        //后台用ExcelConfigData
        #region SelectedFile
        private FileEx selectedFile;
        public FileEx SelectedFile
        {
            get => this.selectedFile; set
            {
                this.TitleText.Text = value.FullFileName;
                this.IsPaneOpen = false;
                this.SetPresentDataViewAsync(value);
                this.Set(ref this.selectedFile, value);
            }
        }
        #endregion
        //SplitView用pane状态
        #region IsPaneOpen
        private bool isPaneOpen;
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
