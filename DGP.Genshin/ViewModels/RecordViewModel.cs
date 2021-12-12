using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.DataModel.MiHoYo2;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DGP.Genshin.Services.ViewModels
{
    [ViewModel]
    public class RecordViewModel : ObservableObject
    {
        private readonly IRecordService recordService;
        public RecordViewModel(IRecordService recordService)
        {
            this.recordService = recordService;
        }

        private Record? currentRecord;
        public Record? CurrentRecord { get => currentRecord; set => SetProperty(ref currentRecord, value); }

        private bool isQuerying = false;
        public bool IsQuerying { get => isQuerying; set => SetProperty(ref isQuerying, value); }
    }
}