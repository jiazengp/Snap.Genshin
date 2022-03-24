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
            get => currentRecord;

            set => SetProperty(ref currentRecord, value);
        }
        public string? StateDescription
        {
            get => stateDescription;

            set => SetProperty(ref stateDescription, value);
        }
        public List<UserGameRole> UserGameRoles
        {
            get => userGameRoles;

            set => SetProperty(ref userGameRoles, value);
        }
        public ICommand QueryCommand { get; }
        public ICommand OpenUICommand { get; }

        public RecordViewModel(IRecordService recordService, ICookieService cookieService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger) : base(messenger)
        {
            this.recordService = recordService;
            this.cookieService = cookieService;

            QueryCommand = asyncRelayCommandFactory.Create<string>(UpdateRecordAsync);
            OpenUICommand = asyncRelayCommandFactory.Create(OpenUIAsync);
        }

        private async Task OpenUIAsync()
        {
            try
            {
                UserGameRoles = await new UserGameRoleProvider(cookieService.CurrentCookie).GetUserGameRolesAsync(CancellationToken);
            }
            catch (TaskCanceledException)
            {
                this.Log("Open UI canceled");
            }
        }
        private async Task UpdateRecordAsync(string? uid)
        {
            if (updateRecordTaskPreventer.ShouldExecute)
            {
                try
                {
                    Record record = await recordService.GetRecordAsync(uid);

                    if (record.Success)
                    {
                        CurrentRecord = record;
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
                    updateRecordTaskPreventer.Release();
                }
            }
        }

        public void Receive(RecordProgressChangedMessage message)
        {
            StateDescription = message.Value;
        }
    }
}