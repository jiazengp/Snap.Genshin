using DGP.Genshin.Controls;
using DGP.Genshin.Data.Character;
using DGP.Genshin.Data.Weapon;
using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Service;
using DGP.Snap.Framework.Exceptions;
using DGP.Snap.Framework.Extensions.System.Collections.Generic;
using ModernWpf.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    public partial class GachaStatisticPage : System.Windows.Controls.Page, INotifyPropertyChanged
    {
        public GachaStatisticPage()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e) => await this.SyncDataFromService();

        private async Task SyncDataFromService()
        {

            IDictionary<string, IEnumerable<GachaLogItem>> dict = new Dictionary<string, IEnumerable<GachaLogItem>>();
            await Task.Run(() =>
            {
                Thread.Sleep(1000);
                try
                {
                    dict = GachaStatisticService.Instance.Data.GachaLogs;
                }
                catch (UrlNotFoundException)
                {
                    dict = null;
                    this.Dispatcher.InvokeAsync(() =>
                    {
                        new ContentDialog()
                        {
                            Title = "获取数据失败",
                            Content = "请打开原神\n进入祈愿界面查看历史记录\n退出原神后再次尝试",
                            PrimaryButtonText = "确认",
                            DefaultButton = ContentDialogButton.Primary,

                        }.ShowAsync();
                        Service.NavigationService.Current.GoBack();
                    });
                }
                if (dict != null)
                {
                    this.Dispatcher.InvokeAsync(() =>
                    {
                        this.NormalPool = this.ConvertToIcon(dict["200"]);
                        this.NormalPoolSource = dict["200"];
                        this.NormalPoolView = this.BuildView(this.NormalPoolSource);

                        this.NovicePool = this.ConvertToIcon(dict["100"]);
                        this.NovicePoolSource = dict["100"];
                        this.NovicePoolView = this.BuildView(this.NovicePoolSource);

                        this.CharacterPool = this.ConvertToIcon(dict["301"]);
                        this.CharacterPoolSource = dict["301"];
                        this.CharacterPoolView = this.BuildView(this.CharacterPoolSource);

                        this.WeaponPool = this.ConvertToIcon(dict["302"]);
                        this.WeaponPoolSource = dict["302"];
                        this.WeaponPoolView = this.BuildView(this.WeaponPoolSource);
                    });
                }
            });
            Debug.WriteLine("sync completed");
            
        }

        private IEnumerable<GachaLogNode> BuildView(IEnumerable<GachaLogItem> pool)
        {
            IEnumerable<GachaLogItem> rank5 = pool.Where(i => i.Rank == "5");
            IEnumerable<GachaLogItem> rank4 = pool.Where(i => i.Rank == "4");
            IEnumerable<GachaLogItem> rank3 = pool.Where(i => i.Rank == "3");

            Dictionary<string, int> countOfRank5 = new Dictionary<string, int>();
            foreach(GachaLogItem item in rank5)
            {
                countOfRank5.AddOrSet(item.Name, v => ++v);
            }
            Dictionary<string, int> countOfRank4 = new Dictionary<string, int>();
            foreach (GachaLogItem item in rank4)
            {
                countOfRank4.AddOrSet(item.Name, v => ++v);
            }
            Dictionary<string, int> countOfRank3 = new Dictionary<string, int>();
            foreach (GachaLogItem item in rank3)
            {
                countOfRank3.AddOrSet(item.Name, v => ++v);
            }

            IEnumerable<GachaLogNode> gachaLogNodes = new List<GachaLogNode>()
            {
                new GachaLogNode("五星", rank5.Count(), countOfRank5.Select(i => new GachaLogNode(i.Key,i.Value,null)).OrderByDescending(i=>i.Count)),
                new GachaLogNode("四星", rank4.Count(), countOfRank4.Select(i => new GachaLogNode(i.Key,i.Value,null)).OrderByDescending(i=>i.Count)),
                new GachaLogNode("三星", rank3.Count(), countOfRank3.Select(i => new GachaLogNode(i.Key,i.Value,null)).OrderByDescending(i=>i.Count))
            };
            return gachaLogNodes;
        }

        public IEnumerable<UserControl> ConvertToIcon(IEnumerable<GachaLogItem> items)
        {
            foreach (var item in items)
            {
                switch (item.ItemType)
                {
                    case "武器":
                        yield return new WeaponIcon() { Weapon = WeaponManager.Instance.Weapons.First(i => i.WeaponName == item.Name) };
                        break;
                    case "角色":
                        yield return new CharacterIcon() { Character = CharacterManager.Instance.Characters.First(i => i.CharacterName == item.Name) };
                        break;
                }
            }
        }

        #region Property

        #region NormalPool
        private IEnumerable<UserControl> normalPool;
        public IEnumerable<UserControl> NormalPool { get => this.normalPool; set => this.Set(ref this.normalPool, value); }
        #endregion

        #region NormalPoolSource
        private IEnumerable<GachaLogItem> normalPoolSource;
        public IEnumerable<GachaLogItem> NormalPoolSource { get => this.normalPoolSource; set => this.Set(ref this.normalPoolSource, value); }
        #endregion

        #region NormalPoolView
        private IEnumerable<GachaLogNode> normalPoolView;
        public IEnumerable<GachaLogNode> NormalPoolView { get => this.normalPoolView; set => this.Set(ref this.normalPoolView, value); }
        #endregion


        #region NovicePool
        private IEnumerable<UserControl> novicePool;
        public IEnumerable<UserControl> NovicePool
        {
            get => this.novicePool;
            set => this.Set(ref this.novicePool, value);
        }
        #endregion

        #region NovicePoolSource
        private IEnumerable<GachaLogItem> novicePoolSource;
        public IEnumerable<GachaLogItem> NovicePoolSource { get => this.novicePoolSource; set => this.Set(ref this.novicePoolSource, value); }
        #endregion

        #region NovicePoolView
        private IEnumerable<GachaLogNode> novicePoolView;
        public IEnumerable<GachaLogNode> NovicePoolView { get => this.novicePoolView; set => this.Set(ref this.novicePoolView, value); }
        #endregion


        #region CharacterPool
        private IEnumerable<UserControl> characterPool;
        public IEnumerable<UserControl> CharacterPool
        {
            get => this.characterPool;
            set => this.Set(ref this.characterPool, value);
        }
        #endregion

        #region CharacterPoolSource
        private IEnumerable<GachaLogItem> characterPoolSource;
        public IEnumerable<GachaLogItem> CharacterPoolSource { get => this.characterPoolSource; set => this.Set(ref this.characterPoolSource, value); }
        #endregion

        #region CharacterPoolView
        private IEnumerable<GachaLogNode> characterPoolView;
        public IEnumerable<GachaLogNode> CharacterPoolView { get => this.characterPoolView; set => this.Set(ref this.characterPoolView, value); }
        #endregion


        #region WeaponPool
        private IEnumerable<UserControl> weaponPool;

        public IEnumerable<UserControl> WeaponPool
        {
            get => this.weaponPool;
            set => this.Set(ref this.weaponPool, value);
        }
        #endregion

        #region NovicePoolSource
        private IEnumerable<GachaLogItem> weaponPoolSource;
        public IEnumerable<GachaLogItem> WeaponPoolSource { get => this.weaponPoolSource; set => this.Set(ref this.weaponPoolSource, value); }
        #endregion

        #region WeaponPoolView
        private IEnumerable<GachaLogNode> weaponPoolView;
        public IEnumerable<GachaLogNode> WeaponPoolView { get => this.weaponPoolView; set => this.Set(ref this.weaponPoolView, value); }
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

        private async void RefreahRequested(object sender, RoutedEventArgs e)
        {
            (sender as AppBarButton).IsEnabled = false;
            await Task.Run(() =>
            {
                GachaStatisticService.Instance.RequestAllGachaLogsMergeSave();
            }).ContinueWith((task) => this.SyncDataFromService());
            (sender as AppBarButton).IsEnabled = true;
        }
    }
}
