using DGP.Genshin.Control.WebViewLobby;
using DGP.Genshin.DataModel.WebViewLobby;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
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
        private const string commonScriptLinkUrl = "https://www.snapgenshin.com/documents/features/customize-webpage.html";

        private readonly IAsyncRelayCommandFactory asyncRelayCommandFactory;
        private readonly IMessenger messenger;

        private ObservableCollection<WebViewEntry>? entries;

        public ObservableCollection<WebViewEntry>? Entries
        {
            get => entries;

            set => SetProperty(ref entries, value);
        }

        public ICommand AddEntryCommand { get; }
        public ICommand CommonScriptCommand { get; }
        public ICommand ModifyCommand { get; }
        public ICommand RemoveEntryCommand { get; }
        public ICommand NavigateCommand { get; }

        public WebViewLobbyViewModel(IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger)
        {
            this.asyncRelayCommandFactory = asyncRelayCommandFactory;
            this.messenger = messenger;

            AddEntryCommand = asyncRelayCommandFactory.Create(AddEntryAsync);
            ModifyCommand = asyncRelayCommandFactory.Create<WebViewEntry>(ModifyEntryAsync);
            RemoveEntryCommand = new RelayCommand<WebViewEntry>(RemoveEntry);
            NavigateCommand = new RelayCommand<WebViewEntry>(Navigate);
            CommonScriptCommand = new RelayCommand(() => Process.Start(new ProcessStartInfo() { FileName = commonScriptLinkUrl, UseShellExecute = true }));
            
            LoadEntries();
        }

        private async Task AddEntryAsync()
        {
            WebViewEntry? entry = await new WebViewEntryDialog().GetWebViewEntryAsync();
            if (entry is not null)
            {
                entry.ModifyCommand = ModifyCommand;
                entry.RemoveCommand = RemoveEntryCommand;
                entry.NavigateCommand = NavigateCommand;
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
                    modified.ModifyCommand = ModifyCommand;
                    modified.RemoveCommand = RemoveEntryCommand;
                    modified.NavigateCommand = NavigateCommand;
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
            messenger.Send(new NavigateRequestMessage(typeof(WebViewHostPage), false, entry));
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
                        entry.ModifyCommand = ModifyCommand;
                        entry.RemoveCommand = RemoveEntryCommand;
                        entry.NavigateCommand = NavigateCommand;
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
