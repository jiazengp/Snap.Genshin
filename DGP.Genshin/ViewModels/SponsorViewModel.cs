using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Net.Afdian;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    internal class SponsorViewModel : ObservableObject
    {
        private const string UserId = "8f9ed3e87f4911ebacb652540025c377";
        private const string Token = "Th98JamKvc5FHYyErgM4d6spAXGVwbPD";

        private List<Sponsor>? sponsors;
        private ICommand? openUICommand;

        public List<Sponsor>? Sponsors
        {
            get => sponsors;
            set => SetProperty(ref sponsors, value);
        }

        public ICommand OpenUICommand
        {
            get
            {
                openUICommand ??= new AsyncRelayCommand(OpenUIAsync);
                return openUICommand;
            }
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
