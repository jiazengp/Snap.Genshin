using DGP.Genshin.HutaoAPI;
using DGP.Genshin.Service.Abstratcion;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class HutaoStatisticViewModel
    {
        private readonly ICookieService cookieService;
        public ICommand OpenUICommand { get; }
        public HutaoStatisticViewModel(ICookieService cookieService)
        {
            this.cookieService = cookieService;

            OpenUICommand = new AsyncRelayCommand(TestAsync);
        }

        public async Task TestAsync()
        {
            PlayerRecordClient playerRecordClient = new();
            await playerRecordClient.GetAllRecordsAndPostAsync(cookieService.CurrentCookie);
        }
    }
}
