using DGP.Genshin.DataViewer.Helper;
using DGP.Genshin.DataViewer.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private static Dictionary<string, string> currentTextMap;

        private string path;
        private void OpenFolderRequested(object sender, RoutedEventArgs e)
        {
            WorkingFolderService.SelectWorkingFolder();
            this.path = WorkingFolderService.WorkingFolderPath;
            if (this.path != null)
            {
                if (!Directory.Exists(this.path + @"\TextMap\") || !Directory.Exists(this.path + @"\Excel\"))
                    SelectSuggentionDialog.ShowAsync();
                else
                {
                    this.TextMapCollection = Directory.GetFiles(this.path + @"\TextMap\").Select(f => new FileEx(f));
                    this.ExcelConfigDataCollection = Directory.GetFiles(this.path + @"\Excel\").Select(f => new FileEx(f));
                }
            }
        }
        private void PaneStateChangeRequested(object sender, RoutedEventArgs e) => this.IsPaneOpen = !this.IsPaneOpen;
        private void SetPresentDataView(FileEx value)
        {
            this.PresentDataTable = Json.ToObject<JArray>(value.ReadFile());
            foreach (JObject row in this.PresentDataTable)
            {
                foreach (JProperty p in row.Properties())
                {
                    if (p.Name.Contains("TextMapHash"))
                        p.Value = TextMapHashService.GetMapTextBy(p, currentTextMap);
                    if (p.Value.Type == JTokenType.Array)
                    {
                    }
                    if (p.Value.Type == JTokenType.Object)
                    {
                    }
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
                currentTextMap = Json.ToObject<Dictionary<string, string>>(this.selectedTextMap.ReadFile());
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
                SetPresentDataView(value);
                IsPaneOpen = false;
                this.Set(ref this.selectedFile, value);
            }
        }
        #endregion

        #region PresentDataTable
        private JArray presentDataTable;
        public JArray PresentDataTable
        {
            get => this.presentDataTable; set => this.Set(ref this.presentDataTable, value);
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
