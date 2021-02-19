using DGP.Genshin.DataViewer.Helper;
using DGP.Genshin.DataViewer.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.DataViewer.Views
{
    /// <summary>
    /// ExcelSplitView.xaml 的交互逻辑
    /// </summary>
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
                SelectSuggentionDialog.ShowAsync();
            else
            {
                this.TextMapCollection = DirectoryEx.GetFileExs(path + @"\TextMap\");
                this.ExcelConfigDataCollection = DirectoryEx.GetFileExs(path + @"\Excel\");
                if (ExcelConfigDataCollection.Count() == 0)
                    SelectSuggentionDialog.ShowAsync();
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
        private void SetPresentDataView(FileEx value)
        {
            
            JArray dataArray = Json.ToObject<JArray>(value.Read());
            DataTable table = new DataTable();
            foreach (JProperty p in from JObject o in dataArray from p in
                                  from JProperty p in o.Properties()
                                  where !table.Columns.Contains(p.Name)
                                  select p select p)
            {
                table.Columns.Add(p.Name);
            }
            foreach (JObject o in dataArray)
            {
                DataRow row = table.NewRow();
                
                foreach (JProperty p in o.Properties())
                {
                    RemapTextByHash(p);
                    ReMapNpcByID(p);
                    row[p.Name] = p.Value;
                    
                    //if (p.Value.Type == JTokenType.Array)
                    //{
                    //}
                    //if (p.Value.Type == JTokenType.Object)
                    //{
                    //}
                }
                table.Rows.Add(row);
            }
            //PresentDataSource=array.AsEnumerable();
            //PresentDataGrid.ItemsSource = PresentDataSource;
            PresentDataGrid.ItemsSource = table.AsDataView();
        }

        private static void RemapTextByHash(JProperty p)
        {
            if (p.Name.Contains("TextMapHash"))
                p.Value = MapService.GetMappedTextBy(p);
        }
        private static void ReMapNpcByID(JProperty p)
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

        #region TextMapCollection
        private IEnumerable<FileEx> textMapCollection;
        public IEnumerable<FileEx> TextMapCollection
        {
            get => this.textMapCollection; set => this.Set(ref this.textMapCollection, value);
        }
        #endregion

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

        #region ExcelConfigDataCollection
        private IEnumerable<FileEx> excelConfigDataCollection;
        public IEnumerable<FileEx> ExcelConfigDataCollection
        {
            get => this.excelConfigDataCollection; set => this.Set(ref this.excelConfigDataCollection, value);
        }
        #endregion

        #region SelectedFile
        private FileEx selectedFile;
        public FileEx SelectedFile
        {
            get => this.selectedFile; set
            {
                TitleText.Text = value.FullFileName;
                SetPresentDataView(value);
                IsPaneOpen = false;
                this.Set(ref this.selectedFile, value);
            }
        }
        #endregion

        #region PresentDataTable
        private IEnumerable<JToken> presentDataSource;
        public IEnumerable<JToken> PresentDataSource
        {
            get => this.presentDataSource; set
            {
                this.Set(ref this.presentDataSource, value);
                StaticText.Text = presentDataSource.Count() + " 项";
            }
        }
        #endregion

        #region IsPaneOpen
        private bool isPaneOpen;
        public bool IsPaneOpen
        {
            get => this.isPaneOpen; set => this.Set(ref this.isPaneOpen, value);
        }
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
