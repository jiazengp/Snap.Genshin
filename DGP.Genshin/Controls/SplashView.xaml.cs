using DGP.Genshin.Cookie;
using DGP.Genshin.Services;
using System;
using System.ComponentModel;
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
        public MetadataViewModel MetaDataService { get; set; } = MetadataViewModel.Instance;
        private bool integrityCheckCompleted;
        private bool isCookieVisible = true;
        private bool hasCheckCompleted;
        private string currentStateDescription = "校验缓存完整性...";

        public bool IsCookieVisible
        {
            get => isCookieVisible; set
            {
                Set(ref isCookieVisible, value);
                OnInitializeStateChanged();
            }
        }

        public bool HasCheckCompleted { get => hasCheckCompleted; set => Set(ref hasCheckCompleted, value); }
        public string CurrentStateDescription { get => currentStateDescription; set => Set(ref currentStateDescription, value); }
        public SplashView()
        {
            DataContext = this;
            MetadataViewModel.Instance.CompleteStateChanged += isCompleted =>
            {
                if (isCompleted)
                {
                    integrityCheckCompleted = true;
                    OnInitializeStateChanged();
                }
            };
            IsCookieVisible = !CookieManager.IsCookieAvailable;
            InitializeComponent();
        }

        private async void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            await MetadataViewModel.Instance.CheckAllIntegrityAsync();
        }

        /// <summary>
        /// you need to set <see cref="HasCheckCompleted"/> to true to collaspe the view
        /// </summary>
        public event Action<SplashView>? PostInitializationAction;

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
            await CookieManager.SetCookieAsync();
            IsCookieVisible = !CookieManager.IsCookieAvailable;
        }

        private void OnInitializeStateChanged()
        {
            if (IsCookieVisible == false && integrityCheckCompleted)
            {
                PostInitializationAction?.Invoke(this);
            }
        }
    }
}
