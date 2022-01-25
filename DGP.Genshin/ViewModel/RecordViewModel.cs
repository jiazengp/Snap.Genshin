using DGP.Genshin.DataModel.MiHoYo2;
using DGP.Genshin.Message;
using DGP.Genshin.Service.Abstratcion;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    public class RecordViewModel : ObservableRecipient2, IRecipient<RecordProgressChangedMessage>
    {
        private readonly IRecordService recordService;

        private readonly TaskPreventer updateRecordTaskPreventer = new();

        private Record? currentRecord;
        private string? stateDescription;

        public Record? CurrentRecord { get => currentRecord; set => SetProperty(ref currentRecord, value); }
        public string? StateDescription { get => stateDescription; set => SetProperty(ref stateDescription, value); }
        public ICommand QueryCommand { get; }

        public RecordViewModel(IRecordService recordService, IMessenger messenger) : base(messenger)
        {
            this.recordService = recordService;

            QueryCommand = new AsyncRelayCommand<string?>(UpdateRecordAsync);
        }

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