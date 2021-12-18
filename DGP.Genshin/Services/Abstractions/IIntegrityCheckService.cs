using DGP.Genshin.DataModels;
using DGP.Genshin.DataModels.Characters;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface IIntegrityCheckService
    {
        bool IntegrityCheckCompleted { get; }

        Task CheckAllIntegrityAsync(Action<IIntegrityState> progressedCallback);
        Task CheckCharacterIntegrityAsync(ObservableCollection<Character>? collection, int totalCount, IProgress<IIntegrityState> progress);
        Task CheckIntegrityAsync<T>(ObservableCollection<T>? collection, int totalCount, IProgress<IIntegrityState> progress) where T : KeySource;

        public interface IIntegrityState
        {
            int CurrentCount { get; set; }
            int TotalCount { get; set; }
            string? Info { get; set; }
        }
    }
}
