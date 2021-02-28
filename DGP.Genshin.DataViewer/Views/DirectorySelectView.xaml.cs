using DGP.Genshin.DataViewer.Controls.Dialogs;
using DGP.Genshin.DataViewer.Helpers;
using DGP.Genshin.DataViewer.Services;
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
            if (path == null)
                return;
            if (!Directory.Exists(path + @"\TextMap\") || !Directory.Exists(path + @"\Excel\"))
            {
                new SelectionSuggestDialog().ShowAsync();
            }
            else
            {
                this.ExcelSplitView.TextMapCollection = DirectoryEx.GetFileExs(path + @"\TextMap\");
                this.ExcelSplitView.ExcelConfigDataCollection = DirectoryEx.GetFileExs(path + @"\Excel\");
                if (this.ExcelSplitView.ExcelConfigDataCollection.Count() == 0)
                {
                    new SelectionSuggestDialog().ShowAsync();
                }
                else
                {
                    this.Container.GoToElementState("SelectingMap", true);
                    //npcid
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
