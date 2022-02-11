using DGP.Genshin.Control.WebViewLobby;
using DGP.Genshin.DataModel.WebViewLobby;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstratcion;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Data.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Singleton)]
    internal class WebViewLobbyViewModel : ObservableObject2
    {
        private const string entriesFileName = "WebviewEntries.json";
        private const string commonScriptLinkUrl = 
            "https://www.snapgenshin.com/features/#%E9%83%A8%E5%88%86%E9%9C%80%E8%A6%81%E6%89%A7%E8%A1%8C%E8%84%9A%E6%9C%AC%E7%9A%84%E7%BD%91%E9%A1%B5";
        private ObservableCollection<WebViewEntry>? entries;

        public ObservableCollection<WebViewEntry>? Entries { get => entries; set => SetProperty(ref entries, value); }

        public ICommand AddEntryCommand { get; }
        public ICommand CommonScriptCommand { get; }

        public WebViewLobbyViewModel()
        {
            LoadEntries();

            AddEntryCommand = new AsyncRelayCommand(AddEntryAsync);
            CommonScriptCommand = new RelayCommand(() => Process.Start(new ProcessStartInfo() { FileName = commonScriptLinkUrl, UseShellExecute = true }));
        }

        private async Task AddEntryAsync()
        {
            WebViewEntry? entry = await new WebViewEntryDialog().GetWebViewEntryAsync();
            if (entry is not null)
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
        private void Navigate(WebViewEntry? entry)
        {
            App.Messenger.Send(new NavigateRequestMessage(typeof(WebViewHostPage), false, entry));
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
                        entry.NavigateCommand = new RelayCommand<WebViewEntry>(Navigate);
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
