using DGP.Genshin.Cookie;
using DGP.Genshin.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// SplashView.xaml 的交互逻辑
    /// </summary>
    public partial class SplashView : UserControl
    {
        public MetadataViewModel metadataViewModel;

        public SplashView()
        {
            DataContext = this;
            metadataViewModel.CompleteStateChanged += isCompleted =>
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

        private async void CookieButtonClick(object sender, RoutedEventArgs e)
        {
            await CookieManager.SetCookieAsync();
            IsCookieVisible = !CookieManager.IsCookieAvailable;
        }
    }

    public class SplashViewModel : ObservableObject
    {
        private MetadataViewModel metadataViewModel;
        private bool integrityCheckCompleted;

        private bool isCookieVisible = true;
        private bool hasCheckCompleted;
        private string currentStateDescription = "校验缓存完整性...";
        private IAsyncRelayCommand initializeCommand;

        /// <summary>
        /// you need to set <see cref="HasCheckCompleted"/> to true to collaspe the view
        /// </summary>
        public event Action<SplashView>? PostInitializationAction;

        public bool IsCookieVisible
        {
            get => isCookieVisible; set
            {
                SetProperty(ref isCookieVisible, value);
                OnInitializeStateChanged();
            }
        }

        private void OnInitializeStateChanged()
        {
            if (IsCookieVisible == false && integrityCheckCompleted)
            {
                PostInitializationAction?.Invoke(this);
            }
        }
        public bool HasCheckCompleted { get => hasCheckCompleted; set => SetProperty(ref hasCheckCompleted, value); }
        public string CurrentStateDescription { get => currentStateDescription; set => SetProperty(ref currentStateDescription, value); }
        public IAsyncRelayCommand InitializeCommand 
        { 
            get => initializeCommand; 
            [MemberNotNull(nameof(initializeCommand))]
            set => SetProperty(ref initializeCommand, value); }

        public SplashViewModel(MetadataViewModel metadataViewModel)
        {
            this.metadataViewModel = metadataViewModel;
            InitializeCommand = new AsyncRelayCommand(async () => { await this.metadataViewModel.CheckAllIntegrityAsync(); });
        }
    }
}
