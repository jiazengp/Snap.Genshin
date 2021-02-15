using DGP.Genshin.DataViewer.Helper;
using DGP.Genshin.DataViewer.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

        private string path;
        private void OpenFolderRequested(object sender, RoutedEventArgs e)
        {
            WorkingFolderService.SelectWorkingFolder();
            this.path = WorkingFolderService.WorkingFolderPath;
            if (this.path != null)
            {
                if (Directory.Exists(this.path + @"\TextMap\") && Directory.Exists(this.path + @"\Excel\"))
                {
                    this.TextMapCollection = DirectoryEx.GetFileExs(this.path + @"\TextMap\");
                    this.ExcelConfigDataCollection = DirectoryEx.GetFileExs(this.path + @"\Excel\");

                    JArray NpcJarray = Json.ToObject<JArray>(ExcelConfigDataCollection.First(f => f.FileName == "Npc").Read());
                    MapService.NPCMap = NpcJarray.ToDictionary(t => t["Id"].ToString(), v => v["NameTextMapHash"].ToString());
                }
                else
                    SelectSuggentionDialog.ShowAsync();
            }
        }
        private void PaneStateChangeRequested(object sender, RoutedEventArgs e) => this.IsPaneOpen = !this.IsPaneOpen;
        private void SetPresentDataView(FileEx value)
        {
            JArray array = Json.ToObject<JArray>(value.Read());
            foreach (JObject row in array)
            {
                foreach (JProperty p in row.Properties())
                {
                    RemapTextByHash(p);
                    ReMapNpcByID(p);
                    if (p.Value.Type == JTokenType.Array)
                    {
                    }
                    if (p.Value.Type == JTokenType.Object)
                    {
                    }
                }
            }
            PresentDataSource=array.AsEnumerable();
            PresentDataGrid.ItemsSource = PresentDataSource;
            foreach (DataGridColumn c in PresentDataGrid.Columns)
            {
                c.CanUserSort = true;
            }
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

        private void PresentDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private void PresentDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            string header = e.Column.Header.ToString();
            ListSortDirection d = (e.Column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
            if (e.Column.SortDirection == null || e.Column.SortDirection == ListSortDirection.Descending)
            {
                PresentDataGrid.ItemsSource = PresentDataSource.OrderBy(r => r[header]).AsEnumerable();
            }
            else
            {
                PresentDataGrid.ItemsSource = PresentDataSource.OrderByDescending(r => r[header]).AsEnumerable();
            }
            foreach (DataGridColumn c in PresentDataGrid.Columns)
            {
                if (c.Header.ToString() == e.Column.Header.ToString())
                    c.SortDirection = d;
                c.CanUserSort = true;
            }
        }
    }
}
