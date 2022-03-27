using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.DataModel.Reccording;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Messaging;
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
    [ViewModel(InjectAs.Transient)]
    internal class RecordViewModel : ObservableRecipient2, ISupportCancellation, IRecipient<RecordProgressChangedMessage>
    {
        private readonly IRecordService recordService;
        private readonly ICookieService cookieService;

        private readonly TaskPreventer updateRecordTaskPreventer = new();
        public CancellationToken CancellationToken { get; set; }

        private Record? currentRecord;
        private string? stateDescription;
        private List<UserGameRole> userGameRoles = new();

        public Record? CurrentRecord
        {
            get => this.currentRecord;

            set => this.SetProperty(ref this.currentRecord, value);
        }
        public string? StateDescription
        {
            get => this.stateDescription;

            set => this.SetProperty(ref this.stateDescription, value);
        }
        public List<UserGameRole> UserGameRoles
        {
            get => this.userGameRoles;

            set => this.SetProperty(ref this.userGameRoles, value);
        }
        public ICommand QueryCommand { get; }
        public ICommand OpenUICommand { get; }

        public RecordViewModel(IRecordService recordService, ICookieService cookieService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger) : base(messenger)
        {
            this.recordService = recordService;
            this.cookieService = cookieService;

            this.QueryCommand = asyncRelayCommandFactory.Create<string>(this.UpdateRecordAsync);
            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
        }

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
                try
                {
                    Record record = await this.recordService.GetRecordAsync(uid);

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
                                DefaultButton = ContentDialogButton.Primary
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
                                DefaultButton = ContentDialogButton.Primary
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

        public void Receive(RecordProgressChangedMessage message)
        {
            this.StateDescription = message.Value;
        }
    }
}