using DGP.Genshin.DataModels.MiHoYo2;
using DGP.Genshin.Messages;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Threading;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(InjectAs.Transient)]
    public class RecordViewModel : ObservableRecipient, IRecipient<RecordProgressChangedMessage>
    {
        private readonly IRecordService recordService;
        public IRecordService RecordService => recordService;


        private Record? currentRecord;
        private IAsyncRelayCommand<string?> queryCommand;
        private string? stateDescription;

        public Record? CurrentRecord { get => currentRecord; set => SetProperty(ref currentRecord, value); }
        public string? StateDescription { get => stateDescription; set => SetProperty(ref stateDescription, value); }
        public IAsyncRelayCommand<string?> QueryCommand
        {
            get => queryCommand;
            [MemberNotNull(nameof(queryCommand))]
            set => SetProperty(ref queryCommand, value);
        }

        public RecordViewModel(IRecordService recordService, IMessenger messenger) : base(messenger)
        {
            this.recordService = recordService;

            QueryCommand = new AsyncRelayCommand<string?>(UpdateRecordAsync);

            IsActive = true;
        }

        ~RecordViewModel()
        {
            IsActive = false;
        }

        private readonly TaskPreventer updateRecordTaskPreventer = new();
        private async Task UpdateRecordAsync(string? uid)
        {
            if (updateRecordTaskPreventer.ShouldExecute)
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
                updateRecordTaskPreventer.Release();
            }
        }

        public void Receive(RecordProgressChangedMessage message)
        {
            StateDescription = message.Value;
        }
    }
}