using CommunityToolkit.Mvvm.Input;
using DGP.Genshin.DataModel.Achievement;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Service.Abstraction.Achievement;
using Microsoft.Win32;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 成就视图模型
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    internal class AchievementViewModel : ObservableObject2
    {
        private readonly MetadataViewModel metadataViewModel;
        private readonly IAchievementService achievementService;
        private ObservableCollection<Achievement>? achievements;
        private AchievementGoal? selectedAchievementGoal;
        private List<AchievementGoal> achievementGoals = null!;

        private string? query;

        /// <summary>
        /// 构造一个新的成就视图模型
        /// </summary>
        /// <param name="achievementService">成就服务</param>
        /// <param name="metadataViewModel">元数据视图模型</param>
        /// <param name="asyncRelayCommandFactory">异步命令工厂</param>
        public AchievementViewModel(IAchievementService achievementService, MetadataViewModel metadataViewModel, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.metadataViewModel = metadataViewModel;
            this.achievementService = achievementService;
            OpenUICommand = new RelayCommand(OpenUI);
            CloseUICommand = new RelayCommand(CloseUI);
            RefreshQueryCommand = new RelayCommand<string>(RefreshQuery);
            ImportFromCocoGoatCommand = asyncRelayCommandFactory.Create(ImportFromCocoGoatAsync);
        }

        /// <summary>
        /// 成就大纲
        /// </summary>
        public List<AchievementGoal> AchievementGoals
        {
            get => achievementGoals;

            set => SetProperty(ref achievementGoals, value);
        }

        /// <summary>
        /// 当前的成就大纲页
        /// </summary>
        public AchievementGoal? SelectedAchievementGoal
        {
            get => selectedAchievementGoal;

            set => SetPropertyAndCallbackOnCompletion(ref selectedAchievementGoal, value, RefreshView);
        }

        /// <summary>
        /// 成就列表
        /// </summary>
        public ObservableCollection<Achievement>? Achievements
        {
            get => achievements;

            set => SetProperty(ref achievements, value);
        }

        /// <summary>
        /// 打开界面时触发的命令
        /// </summary>
        public ICommand OpenUICommand { get; }

        /// <summary>
        /// 关闭界面时触发的命令
        /// </summary>
        public ICommand CloseUICommand { get; }

        /// <summary>
        /// 筛选查询命令
        /// </summary>
        public ICommand RefreshQueryCommand { get; }

        /// <summary>
        /// 从椰羊导入命令
        /// </summary>
        public ICommand ImportFromCocoGoatCommand { get; }

        private void OpenUI()
        {
            AchievementGoals = metadataViewModel.AchievementGoals;
            Achievements = new(metadataViewModel.Achievements);
            List<IdTime> idTimes = achievementService.GetCompletedItems();
            SetAchievementsState(idTimes, Achievements);
            SelectedAchievementGoal = AchievementGoals.First();
            CollectionViewSource.GetDefaultView(Achievements).Filter = OnFilterAchievement;
        }

        private void CloseUI()
        {
            if (Achievements is not null)
            {
                achievementService.SaveCompletedItems(Achievements);
            }
        }

        private async Task ImportFromCocoGoatAsync()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "JS对象简谱文件|*.json",
                Title = "从 Json 文件导入",
                Multiselect = false,
                CheckFileExists = true,
            };
            if (openFileDialog.ShowDialog() is true)
            {
                IEnumerable<IdTime>? data = achievementService.TryGetImportData(ImportAchievementSource.Cocogoat, openFileDialog.FileName);
                if (data != null)
                {
                    Must.NotNull(Achievements!);
                    int totalCount = SetAchievementsState(data, Achievements);
                    await new ContentDialog()
                    {
                        Title = "导入成功",
                        Content = $"共同步了 {totalCount} 个成就",
                        PrimaryButtonText = "确认",
                        DefaultButton = ContentDialogButton.Primary,
                    }.ShowAsync();
                }
                else
                {
                    await new ContentDialog()
                    {
                        Title = "导入失败",
                        Content = $"选择的文件中包含的成就格式不正确\n请尝试 椰羊-设置-本地导出-导出数据\n解压压缩包后选择 \"成就导出.json\" 再导入",
                        PrimaryButtonText = "确认",
                        DefaultButton = ContentDialogButton.Primary,
                    }.ShowAsync();
                }
            }
        }

        private int SetAchievementsState(IEnumerable<IdTime> idTimes, ObservableCollection<Achievement> achievements)
        {
            Dictionary<int, Achievement> mappedAchievements = achievements.ToDictionary(a => a.Id);
            int count = 0;
            foreach (IdTime? item in idTimes)
            {
                Achievement achievement = mappedAchievements[item.Id];
                achievement.CompleteDateTime = item.Time;
                achievement.IsCompleted = true;
                count++;
            }

            return count;
        }

        [PropertyChangedCallback]
        private void RefreshView()
        {
            if (SelectedAchievementGoal != null && Achievements != null)
            {
                CollectionViewSource.GetDefaultView(Achievements).Refresh();
            }
        }

        private void RefreshQuery(string? query = null)
        {
            // prevent duplecate query
            if (this.query != query)
            {
                this.query = query;
                RefreshView();
            }
        }

        private bool OnFilterAchievement(object obj)
        {
            if (obj is Achievement achi)
            {
                bool goalMatch = achi.GoalId == SelectedAchievementGoal!.Id;

                bool queryMatch = true;
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queryMatch = achi.Title!.Contains(query) || achi.Description!.Contains(query);
                }

                return goalMatch && queryMatch;
            }

            return false;
        }
    }
}