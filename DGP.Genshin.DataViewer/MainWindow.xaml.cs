using DGP.Genshin.DataViewer.Helper;
using DGP.Genshin.DataViewer.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DGP.Genshin.DataViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            this.DataContext = this;
            this.InitializeComponent();
            this.ExcelDataView.SelectedFileChangedHandler += (f) => { this.TitleText.Text = f.FullFileName; };
        }

        private void OpenFolderRequested(object sender, RoutedEventArgs e)
        {
            this.path = WorkingFolderService.SelectWorkingFolder();
            if (this.path != null)
            {
                this.ExcelDataView.WorkingFolderPath = this.path;
                this.TextMapCollection = Directory.GetFiles(this.path + @"\TextMap\").Select(f => new FileEx(f));
            }
        }

        public static string GetMapTextBy(JProperty p)
        {
            if (TextMap != null)
            {
                if (TextMap.TryGetValue(p.Value.ToString(), out string result))
                    return result.Replace(@"\n", "\n").Replace(@"\r", "\r");
            }

            return "[Not Mapped]" + p.Value.ToString();
        }

        private static Dictionary<string, string> TextMap;
        private string path;

        private IEnumerable<FileEx> textMapCollection;
        private FileEx selectedTextMap;
        public IEnumerable<FileEx> TextMapCollection
        {
            get => this.textMapCollection; set => this.Set(ref this.textMapCollection, value);
        }
        public FileEx SelectedTextMap
        {
            get => this.selectedTextMap; set
            {
                this.Set(ref this.selectedTextMap, value);
                TextMap = Json.ToObject<Dictionary<string, string>>(this.selectedTextMap.ReadFile());
            }
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

        private void PaneStateChangeRequested(object sender, RoutedEventArgs e) => this.ExcelDataView.IsPaneOpen = !this.ExcelDataView.IsPaneOpen;
    }
}
