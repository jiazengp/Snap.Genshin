using DGP.Genshin.DataViewer.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private string ReadFile(FileEx file)
        {
            string json;
            using (StreamReader sr = new StreamReader(file.FullPath))
            {
                json = sr.ReadToEnd();
            }
            return json;
        }

        public Action<FileEx> SelectedFileChangedHandler;
        #region Property
        private string workingFolderPath;
        private IEnumerable<FileEx> excelConfigDataCollection;
        private FileEx selectedFile;
        private JArray presentDataTable;
        private bool isPaneOpen;

        public string WorkingFolderPath
        {
            get => this.workingFolderPath; set
            {
                this.ExcelConfigDataCollection = Directory.GetFiles(value + @"\Excel\").Select(f => new FileEx(f));
                this.Set(ref this.workingFolderPath, value);
            }
        }
        public IEnumerable<FileEx> ExcelConfigDataCollection
        {
            get => this.excelConfigDataCollection; set => this.Set(ref this.excelConfigDataCollection, value);
        }
        public FileEx SelectedFile
        {
            get => this.selectedFile; set
            {
                this.PresentDataTable = Json.ToObject<JArray>(this.ReadFile(value));
                foreach (JObject o in this.PresentDataTable)
                {
                    foreach (JProperty p in o.Properties())
                    {
                        if (p.Name.Contains("TextMapHash"))
                            p.Value = MainWindow.GetMapTextBy(p);
                        if (p.Value.Type == JTokenType.Array)
                        {

                        }
                        if (p.Value.Type == JTokenType.Object)
                        {

                        }
                    }
                }
                this.SelectedFileChangedHandler?.Invoke(value);
                this.Set(ref this.selectedFile, value);
            }
        }
        public JArray PresentDataTable
        {
            get => this.presentDataTable; set => this.Set(ref this.presentDataTable, value);
        }
        public bool IsPaneOpen
        {
            get => this.isPaneOpen; set => this.Set(ref this.isPaneOpen, value);
        }
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
