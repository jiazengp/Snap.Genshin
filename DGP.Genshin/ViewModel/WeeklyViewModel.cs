using CommunityToolkit.Mvvm.ComponentModel;
using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.DataModel.Character;
using DGP.Genshin.DataModel.Material;
using DGP.Genshin.Factory.Abstraction;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Data.Primitive;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WeeklyDefinition = DGP.Genshin.DataModel.Material.Weeklies;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 周本视图模型
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    internal class WeeklyViewModel : ObservableObject, ISupportCancellation
    {
        private readonly MetadataViewModel metadata;
        private IList<Weekly>? weeklies;

        /// <summary>
        /// 构造一个心的周本视图模型
        /// </summary>
        /// <param name="metadata">元数据模型</param>
        /// <param name="asyncRelayCommandFactory">异步命令工厂</param>
        public WeeklyViewModel(MetadataViewModel metadata, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.metadata = metadata;

            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
        }

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// 周本列表
        /// </summary>
        public IList<Weekly>? Weeklies { get => this.weeklies; set => this.SetProperty(ref this.weeklies, value); }

        /// <summary>
        /// 打开界面触发的命令
        /// </summary>
        public ICommand OpenUICommand { get; }

        private async Task OpenUIAsync()
        {
            try
            {
                await Task.Delay(500, this.CancellationToken);
                this.BuildWeeklies();
            }
            catch (TaskCanceledException)
            {
                this.Log("Open UI cancelled");
            }
        }

        private void BuildWeeklies()
        {
            this.Weeklies = new List<Weekly>()
            {
                new Weekly(
                    "裂空的魔龙",
                    this.IndexedFromWeeklyName(WeeklyDefinition.DvalinsPlume),
                    this.IndexedFromWeeklyName(WeeklyDefinition.DvalinsClaw),
                    this.IndexedFromWeeklyName(WeeklyDefinition.DvalinsSigh)),
                new Weekly(
                    "北风的王狼",
                    this.IndexedFromWeeklyName(WeeklyDefinition.TailofBoreas),
                    this.IndexedFromWeeklyName(WeeklyDefinition.RingofBoreas),
                    this.IndexedFromWeeklyName(WeeklyDefinition.SpiritLocketofBoreas)),

                new Weekly(
                    "「公子」",
                    this.IndexedFromWeeklyName(WeeklyDefinition.TuskofMonocerosCaeli),
                    this.IndexedFromWeeklyName(WeeklyDefinition.ShardofaFoulLegacy),
                    this.IndexedFromWeeklyName(WeeklyDefinition.ShadowoftheWarrior)),
                new Weekly(
                    "若陀龙王",
                    this.IndexedFromWeeklyName(WeeklyDefinition.DragonLordsCrown),
                    this.IndexedFromWeeklyName(WeeklyDefinition.BloodjadeBranch),
                    this.IndexedFromWeeklyName(WeeklyDefinition.GildedScale)),

                new Weekly(
                    "「女士」",
                    this.IndexedFromWeeklyName(WeeklyDefinition.MoltenMoment),
                    this.IndexedFromWeeklyName(WeeklyDefinition.HellfireButterfly),
                    this.IndexedFromWeeklyName(WeeklyDefinition.AshenHeart)),
                new Weekly(
                    "祸津御建鸣神命",
                    this.IndexedFromWeeklyName(WeeklyDefinition.MudraoftheMaleficGeneral),
                    this.IndexedFromWeeklyName(WeeklyDefinition.TearsoftheCalamitousGod),
                    this.IndexedFromWeeklyName(WeeklyDefinition.TheMeaningofAeons)),
            };
        }

        private Indexed<Material?, Character> IndexedFromWeeklyName(string weeklyName)
        {
            List<Character> list = this.metadata.Characters
                .Where(c => c.Weekly!.Source == weeklyName)
                .ToList();
            return new(list.FirstOrDefault()?.Weekly, list);
        }

        /// <summary>
        /// 表示一个周本材料展示
        /// </summary>
        internal record Weekly
        {
            /// <summary>
            /// 构造一个新的周本材料展示
            /// </summary>
            /// <param name="name">名称</param>
            /// <param name="first">材料1列表</param>
            /// <param name="second">材料2列表</param>
            /// <param name="third">材料3列表</param>
            public Weekly(string name, Indexed<Material?, Character> first, Indexed<Material?, Character> second, Indexed<Material?, Character> third)
            {
                this.Name = name;
                this.First = first;
                this.Second = second;
                this.Third = third;
            }

            /// <summary>
            /// 名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 材料1
            /// </summary>
            public Indexed<Material?, Character> First { get; set; }

            /// <summary>
            /// 材料2
            /// </summary>
            public Indexed<Material?, Character> Second { get; set; }

            /// <summary>
            /// 材料3
            /// </summary>
            public Indexed<Material?, Character> Third { get; set; }
        }
    }
}