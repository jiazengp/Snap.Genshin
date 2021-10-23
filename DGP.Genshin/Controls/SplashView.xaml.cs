using DGP.Genshin.Cookie;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Settings;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// SplashView.xaml 的交互逻辑
    /// </summary>
    public partial class SplashView : UserControl, INotifyPropertyChanged
    {
        public MetaDataService MetaDataService { get; set; } = MetaDataService.Instance;
        private bool integrityCheckCompleted = false;
        private bool isCookieVisible = true;
        private bool isLauncherPathVisible = true;
        private bool hasCheckCompleted;
        private string currentStateDescription = "校验图片资源完整性...";

        public bool IsCookieVisible
        {
            get => isCookieVisible; set
            {
                Set(ref isCookieVisible, value);
                OnInitializeStateChanged();
            }
        }
        public bool IsLauncherPathVisible
        {
            get => isLauncherPathVisible; set
            {
                Set(ref isLauncherPathVisible, value);
                OnInitializeStateChanged();
            }
        }

        public bool HasCheckCompleted { get => hasCheckCompleted; set => Set(ref hasCheckCompleted, value); }
        public string CurrentStateDescription { get => currentStateDescription; set => Set(ref currentStateDescription, value); }
        public SplashView()
        {
            DataContext = this;
            MetaDataService.Instance.CompleteStateChanged += isCompleted =>
            {
                if (isCompleted)
                {
                    integrityCheckCompleted = true;
                    OnInitializeStateChanged();
                }
            };
            IsCookieVisible = !CookieManager.IsCookieAvailable;
            IsLauncherPathVisible = SettingService.Instance.Equals<string>(Setting.LauncherPath, null, null) ?? true;
            InitializeComponent();
        }

        private async void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            await MetaDataService.Instance.CheckAllIntegrityAsync();
        }

        /// <summary>
        /// you need to set <see cref="HasCheckCompleted"/> to true to collaspe the view
        /// </summary>
        public event Action<SplashView>? InitializationPostAction;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private async void CookieButtonClick(object sender, RoutedEventArgs e)
        {
            await CookieManager.AddCookieAsync();
            IsCookieVisible = !CookieManager.IsCookieAvailable;
        }

        private void LauncherPathButtonClick(object sender, RoutedEventArgs e)
        {
            string? launcherPath = SettingService.Instance.GetOrDefault<string?>(Setting.LauncherPath, null);
            if (!File.Exists(launcherPath) || Path.GetFileNameWithoutExtension(launcherPath) != "launcher")
            {
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Filter = "启动器|launcher.exe",
                    Title = "选择启动器文件",
                    CheckPathExists = true,
                    FileName = $"launcher.exe"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    launcherPath = openFileDialog.FileName;
                    SettingService.Instance[Setting.LauncherPath] = launcherPath;
                    IsLauncherPathVisible = SettingService.Instance.Equals<string>(Setting.LauncherPath, null, null) ?? true;
                }
            }
        }

        private void OnInitializeStateChanged()
        {
            if (IsCookieVisible == false && IsLauncherPathVisible == false && integrityCheckCompleted)
            {
                InitializationPostAction?.Invoke(this);
            }
        }
    }
}
