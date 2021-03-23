using DGP.Genshin.DataViewer.Controls.Dialogs;
using DGP.Genshin.DataViewer.Helpers;
using DGP.Genshin.DataViewer.Services;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.Extensions.System.Windows;
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
    /// DirectorySelectView.xaml 的交互逻辑
    /// </summary>
    public partial class DirectorySelectView : UserControl, INotifyPropertyChanged
    {
        public DirectorySelectView()
        {
            this.DataContext = this;
            this.InitializeComponent();
            this.Container.GoToElementState("PickingFolder", true);
        }

        private ExcelSplitView excelSplitView;
        public ExcelSplitView ExcelSplitView { get => this.excelSplitView; set => this.Set(ref this.excelSplitView, value); }

        private void OpenFolderRequested(object sender, RoutedEventArgs e)
        {
            WorkingFolderService.SelectWorkingFolder();
            string path = WorkingFolderService.WorkingFolderPath;
            InitializeMaps(path);
        }
        private void OnFolderDrop(object sender, DragEventArgs e)
        {
            string folder = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            this.Log($"dropped file path:{folder}");
            this.Log(Directory.Exists(folder));
            if (Directory.Exists(folder))
            {
                InitializeMaps(folder);
            }
        }
        private void InitializeMaps(string path)
        {
            if (path == null)
                return;
            string mapPath = null;
            string excelPath = null;
            if (Directory.Exists(path + @"\TextMap\"))
                mapPath = path + @"\TextMap\";
            if (Directory.Exists(path + @"\Excel\"))
                excelPath = path + @"\Excel\";
            if (Directory.Exists(path + @"\ExcelBinOutput\"))
                excelPath = path + @"\ExcelBinOutput\";
            if (mapPath == null || excelPath == null)
            {
                new SelectionSuggestDialog().ShowAsync();
            }
            else
            {
                this.ExcelSplitView.TextMapCollection = DirectoryEx.GetFileExs(mapPath);
                this.ExcelSplitView.ExcelConfigDataCollection = DirectoryEx.GetFileExs(excelPath);
                if (this.ExcelSplitView.ExcelConfigDataCollection.Count() == 0)
                {
                    new SelectionSuggestDialog().ShowAsync();
                }
                else
                {
                    this.Container.GoToElementState("SelectingMap", true);
                    MapService.NPCMap = new Lazy<Dictionary<string, string>>(() =>
                    Json.ToObject<JArray>(
                        this.ExcelSplitView.ExcelConfigDataCollection
                        .First(f => f.FileName == "Npc").Read())
                    .ToDictionary(t => t["Id"].ToString(), v => v["NameTextMapHash"].ToString()));
                }
            }
        }

        private void OnMapSelectionChanged(object sender, SelectionChangedEventArgs e) => this.Container.GoToElementState("Confirming", true);

        private void OnConfirmed(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.ExcelSplitView.IsPaneOpen = true;
        }

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
