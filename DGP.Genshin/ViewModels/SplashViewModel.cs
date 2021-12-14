using DGP.Genshin.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DGP.Genshin.ViewModels
{
    public class SplashViewModel : ObservableObject
    {
        private MetadataViewModel metadataViewModel;
        private bool integrityCheckCompleted;

        private bool isCookieVisible = true;
        private bool hasCheckCompleted;
        private string currentStateDescription = "校验缓存完整性...";
        private IAsyncRelayCommand initializeCommand;
        private IAsyncRelayCommand setCookieCommand;

        /// <summary>
        /// you need to set <see cref="HasCheckCompleted"/> to true to collaspe the view
        /// </summary>
        public event Action? PostInitializationAction;

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
                PostInitializationAction?.Invoke();
            }
        }
        public bool HasCheckCompleted { get => hasCheckCompleted; set => SetProperty(ref hasCheckCompleted, value); }
        public string CurrentStateDescription { get => currentStateDescription; set => SetProperty(ref currentStateDescription, value); }
        public IAsyncRelayCommand InitializeCommand
        {
            get => initializeCommand;
            [MemberNotNull(nameof(initializeCommand))]
            set => SetProperty(ref initializeCommand, value);
        }
        public IAsyncRelayCommand SetCookieCommand
        {
            get => setCookieCommand;
            [MemberNotNull(nameof(setCookieCommand))]
            set => SetProperty(ref setCookieCommand, value);
        }

        public SplashViewModel(MetadataViewModel metadataViewModel)
        {
            this.metadataViewModel = metadataViewModel;
            IsCookieVisible = !CookieService.IsCookieAvailable;
            metadataViewModel.CompleteStateChanged += isCompleted =>
            {
                if (isCompleted)
                {
                    integrityCheckCompleted = true;
                    OnInitializeStateChanged();
                }
            };
            InitializeCommand = new AsyncRelayCommand(async () => { await this.metadataViewModel.CheckAllIntegrityAsync(); });
            SetCookieCommand = new AsyncRelayCommand(async () => {
                await CookieService.SetCookieAsync();
                IsCookieVisible = !CookieService.IsCookieAvailable;
            });
        }
    }
}
