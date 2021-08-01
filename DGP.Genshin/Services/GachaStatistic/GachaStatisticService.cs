using DGP.Genshin.Models.MiHoYo.Gacha;
using DGP.Genshin.Models.MiHoYo.Gacha.Statistics;
using ModernWpf.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin.Services.GachaStatistic
{
    public class GachaStatisticService : DependencyObject,INotifyPropertyChanged
    {
        private GachaLogProvider gachaLogProvider;
        private GachaLogProvider GachaLogProvider
        {
            get
            {
                if (gachaLogProvider == null)
                {
                    gachaLogProvider = new GachaLogProvider();
                }
                return gachaLogProvider;
            }
        }

        #region observables
        private Statistic statistic;
        private string selectedUid;
        private bool hasData = true;
        public Statistic Statistic { get => this.statistic; set => this.Set(ref this.statistic, value); }
        public string SelectedUid
        {
            get => selectedUid; set
            {
                Set(ref selectedUid, value);
                LoadLocalData();
            }
        }
        public ObservableCollection<string> Uids { get; set; } = new ObservableCollection<string>();
        public bool HasNoData { get => this.hasData; set => this.Set(ref hasData, value); }
        #endregion

        public async void Refresh()
        {
            if (this.GachaLogProvider.TryFindUrlInLogFile())
            {
                await Task.Run(() =>
                {
                    foreach (ConfigType pool in this.GachaLogProvider.GetGachaConfig().Types)
                    {
                        this.GachaLogProvider.FetchGachaLogIncrement(pool);
                    }
                    this.GachaLogProvider.SaveAll();

                    this.Statistic = StatisticFactory.ToStatistic(this.GachaLogProvider.LocalGachaLogProvider.Data[SelectedUid], SelectedUid);
                    HasNoData = false;
                });
            }
            else
            {
                await new ContentDialog()
                {
                    Title = "获取祈愿记录失败",
                    Content = "请在游戏中打开祈愿历史记录页面后尝试刷新",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
        }
        private async void LoadLocalData()
        {
            await Task.Run(() =>
            {
                var localProvider = this.GachaLogProvider.LocalGachaLogProvider;
                if (localProvider.Data.Count > 0)
                {
                    this.Statistic = StatisticFactory.ToStatistic(localProvider.Data[SelectedUid], SelectedUid);
                    HasNoData = false;
                }
            });
        }
        public void Initialize()
        {
            LoadLocalData();
        }

        #region 单例
        private static GachaStatisticService instance;

        private static readonly object _lock = new();
        private GachaStatisticService()
        {
        }
        public static GachaStatisticService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new GachaStatisticService();
                        }
                    }
                }
                return instance;
            }
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