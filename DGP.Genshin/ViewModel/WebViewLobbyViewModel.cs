using DGP.Genshin.Control.WebViewLobby;
using DGP.Genshin.DataModel.WebViewLobby;
using DGP.Genshin.Helper;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Data.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Singleton)]
    internal class WebViewLobbyViewModel : ObservableObject2
    {
        private const string entriesFileName = "WebviewEntries.json";

        private ObservableCollection<WebViewEntry>? entries;

        public ObservableCollection<WebViewEntry>? Entries { get => entries; set => SetProperty(ref entries, value); }

        public ICommand AddEntryCommand { get; }

        public WebViewLobbyViewModel()
        {
            LoadEntries();

            AddEntryCommand = new AsyncRelayCommand(AddEntryAsync);
        }

        private async Task AddEntryAsync()
        {
            WebViewEntry? entry = await new WebViewEntryDialog().GetWebViewEntryAsync();
            if(entry is not null)
            {
                entry.ModifyCommand = new AsyncRelayCommand<WebViewEntry>(ModifyEntryAsync);
                entry.RemoveCommand = new RelayCommand<WebViewEntry>(RemoveEntry);
                Entries?.Add(entry);
                SaveEntries();
            }
        }
        private async Task ModifyEntryAsync(WebViewEntry? entry)
        {
            if (entry is not null)
            {
                int index = Entries!.IndexOf(entry);
                WebViewEntry? modified = await new WebViewEntryDialog(entry).GetWebViewEntryAsync();
                if (modified is not null)
                {
                    modified.ModifyCommand = new AsyncRelayCommand<WebViewEntry>(ModifyEntryAsync);
                    modified.RemoveCommand = new RelayCommand<WebViewEntry>(RemoveEntry);
                    Entries.RemoveAt(index);
                    Entries.Insert(index, modified);
                    SaveEntries();
                }
            }
        }
        private void RemoveEntry(WebViewEntry? entry)
        {
            if (entry is not null)
            {
                Entries!.Remove(entry);
                SaveEntries();
            }
        }
        private void LoadEntries()
        {
            if (PathContext.FileExists(entriesFileName))
            {
                List<WebViewEntry>? list = Json.FromFile<List<WebViewEntry>>(PathContext.Locate(entriesFileName));
                if (list is not null)
                {
                    list.ForEach(entry =>
                    {
                        entry.ModifyCommand = new AsyncRelayCommand<WebViewEntry>(ModifyEntryAsync);
                        entry.RemoveCommand = new RelayCommand<WebViewEntry>(RemoveEntry);
                    });
                    Entries = new(list);
                    return;
                }
            }
            Entries = new();
        }
        private void SaveEntries()
        {
            Json.ToFile(PathContext.Locate(entriesFileName), Entries);
        }
    }
}
