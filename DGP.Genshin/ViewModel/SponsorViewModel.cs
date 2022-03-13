using DGP.Genshin.Factory.Abstraction;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Snap.Core.DependencyInjection;
using Snap.Net.Afdian;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    public class SponsorViewModel : ObservableObject
    {
        private const string UserId = "8f9ed3e87f4911ebacb652540025c377";
        private const string Token = "Th98JamKvc5FHYyErgM4d6spAXGVwbPD";

        private List<Sponsor>? sponsors;

        public List<Sponsor>? Sponsors
        {
            get => sponsors;

            set => SetProperty(ref sponsors, value);
        }

        public ICommand OpenUICommand { get; }

        public SponsorViewModel(IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            OpenUICommand = asyncRelayCommandFactory.Create(OpenUIAsync);
        }

        private async Task OpenUIAsync()
        {
            int currentPage = 1;
            List<Sponsor> result = new();
            Response<ListWrapper<Sponsor>>? response;
            do
            {
                response = await new AfdianProvider(UserId, Token).QuerySponsorAsync(currentPage++);
                if (response?.Data?.List is List<Sponsor> part)
                {
                    result.AddRange(part);
                }
            }
            while (response?.Data?.TotalPage >= currentPage);

            Sponsors = result;
        }
    }
}
