using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.Factory.Abstraction;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Net.Afdian;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class SponsorViewModel : ObservableObject, ISupportCancellation
    {
        private const string UserId = "8f9ed3e87f4911ebacb652540025c377";
        private const string Token = "Th98JamKvc5FHYyErgM4d6spAXGVwbPD";

        public CancellationToken CancellationToken { get; set; }

        private List<Sponsor>? sponsors;

        public List<Sponsor>? Sponsors
        {
            get => this.sponsors;

            set => this.SetProperty(ref this.sponsors, value);
        }

        public ICommand OpenUICommand { get; }

        public SponsorViewModel(IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
        }

        private async Task OpenUIAsync()
        {
            try
            {
                int currentPage = 1;
                List<Sponsor> result = new();
                Response<ListWrapper<Sponsor>>? response;
                do
                {
                    response = await new AfdianProvider(UserId, Token).QuerySponsorAsync(currentPage++, this.CancellationToken);
                    if (response?.Data?.List is List<Sponsor> part)
                    {
                        result.AddRange(part);
                    }
                }
                while (response?.Data?.TotalPage >= currentPage);

                this.Sponsors = result;
            }
            catch (TaskCanceledException) { this.Log("Open UI canceled"); }
        }
    }
}
