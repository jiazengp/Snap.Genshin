using CommunityToolkit.Mvvm.Messaging;
using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.DataModel.Reccording;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.Service.Abstraction;
using Microsoft.VisualStudio.Threading;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 玩家查询视图模型
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    internal class RecordViewModel : ObservableRecipient2, ISupportCancellation
    {
        private readonly IRecordService recordService;
        private readonly ICookieService cookieService;
        private readonly TaskPreventer updateRecordTaskPreventer = new();

        private Record? currentRecord;
        private string? stateDescription;
        private List<UserGameRole> userGameRoles = new();

        /// <summary>
        /// 构造一个新的玩家查询视图模型
        /// </summary>
        /// <param name="cookieService">cookie服务</param>
        /// <param name="recordService">玩家查询服务</param>
        /// <param name="asyncRelayCommandFactory">异步命令工厂</param>
        /// <param name="messenger">消息器</param>
        public RecordViewModel(
            ICookieService cookieService,
            IRecordService recordService,
            IAsyncRelayCommandFactory asyncRelayCommandFactory,
            IMessenger messenger)
            : base(messenger)
        {
            this.recordService = recordService;
            this.cookieService = cookieService;

            this.QueryCommand = asyncRelayCommandFactory.Create<string>(this.UpdateRecordAsync);
            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
        }

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// 当前的记录
        /// </summary>
        public Record? CurrentRecord
        {
            get => this.currentRecord;

            set => this.SetProperty(ref this.currentRecord, value);
        }

        /// <summary>
        /// 状态提示
        /// </summary>
        public string? StateDescription
        {
            get => this.stateDescription;

            set => this.SetProperty(ref this.stateDescription, value);
        }

        /// <summary>
        /// 可选的用户角色
        /// </summary>
        public List<UserGameRole> UserGameRoles
        {
            get => this.userGameRoles;

            set => this.SetProperty(ref this.userGameRoles, value);
        }

        /// <summary>
        /// 查询命令
        /// </summary>
        public ICommand QueryCommand { get; }

        /// <summary>
        /// 打开界面命令
        /// </summary>
        public ICommand OpenUICommand { get; }

        private async Task OpenUIAsync()
        {
            try
            {
                this.UserGameRoles = await new UserGameRoleProvider(this.cookieService.CurrentCookie).GetUserGameRolesAsync(this.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                this.Log("Open UI canceled");
            }
        }

        private async Task UpdateRecordAsync(string? uid)
        {
            if (this.updateRecordTaskPreventer.ShouldExecute)
            {
                IProgress<string?> progress = new Progress<string?>(this.OnProgressChanged);

                try
                {
                    Record record = await this.recordService.GetRecordAsync(uid, progress);

                    if (record.Success)
                    {
                        this.CurrentRecord = record;
                    }
                    else
                    {
                        if (record.Message?.Length == 0)
                        {
                            ContentDialogResult result = await new ContentDialog()
                            {
                                Title = "查询失败",
                                Content = "米游社用户信息不完整，请在米游社完善个人信息。",
                                PrimaryButtonText = "确认",
                                DefaultButton = ContentDialogButton.Primary,
                            }.ShowAsync();

                            if (result is ContentDialogResult.Primary)
                            {
                                Process.Start("https://bbs.mihoyo.com/ys/");
                            }
                        }
                        else
                        {
                            await new ContentDialog()
                            {
                                Title = "查询失败",
                                Content = $"UID:{uid}\n{record.Message}\n",
                                PrimaryButtonText = "确认",
                                DefaultButton = ContentDialogButton.Primary,
                            }.ShowAsync();
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    this.Log("UpdateRecordAsync canceled by user switch page");
                }
                finally
                {
                    this.updateRecordTaskPreventer.Release();
                }
            }
        }

        private void OnProgressChanged(string? message)
        {
            this.StateDescription = message;
        }
    }
}