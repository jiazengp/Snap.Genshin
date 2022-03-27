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

        private readonly IMessenger messenger;

        private ObservableCollection<WebViewEntry>? entries;

        public ObservableCollection<WebViewEntry>? Entries
        {
            get => this.entries;

            set => this.SetProperty(ref this.entries, value);
        }

        public ICommand AddEntryCommand { get; }
        public ICommand CommonScriptCommand { get; }
        public ICommand ModifyCommand { get; }
        public ICommand RemoveEntryCommand { get; }
        public ICommand NavigateCommand { get; }

        public WebViewLobbyViewModel(IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger)
        {
            this.messenger = messenger;

            this.AddEntryCommand = asyncRelayCommandFactory.Create(this.AddEntryAsync);
            this.ModifyCommand = asyncRelayCommandFactory.Create<WebViewEntry>(this.ModifyEntryAsync);
            this.RemoveEntryCommand = new RelayCommand<WebViewEntry>(this.RemoveEntry);
            this.NavigateCommand = new RelayCommand<WebViewEntry>(this.Navigate);
            this.CommonScriptCommand = new RelayCommand(() => Process.Start(new ProcessStartInfo() { FileName = commonScriptLinkUrl, UseShellExecute = true }));

            this.LoadEntries();
        }

        private async Task AddEntryAsync()
        {
            WebViewEntry? entry = await new WebViewEntryDialog().GetWebViewEntryAsync();
            if (entry is not null)
            {
                entry.ModifyCommand = this.ModifyCommand;
                entry.RemoveCommand = this.RemoveEntryCommand;
                entry.NavigateCommand = this.NavigateCommand;
                this.Entries?.Add(entry);
                this.SaveEntries();
            }
        }
        private async Task ModifyEntryAsync(WebViewEntry? entry)
        {
            if (entry is not null)
            {
                int index = this.Entries!.IndexOf(entry);
                WebViewEntry? modified = await new WebViewEntryDialog(entry).GetWebViewEntryAsync();
                if (modified is not null)
                {
                    modified.ModifyCommand = this.ModifyCommand;
                    modified.RemoveCommand = this.RemoveEntryCommand;
                    modified.NavigateCommand = this.NavigateCommand;
                    this.Entries.RemoveAt(index);
                    this.Entries.Insert(index, modified);
                    this.SaveEntries();
                }
            }
        }
        private void RemoveEntry(WebViewEntry? entry)
        {
            if (entry is not null)
            {
                this.Entries!.Remove(entry);
                this.SaveEntries();
            }
        }
        private void Navigate(WebViewEntry? entry)
        {
            this.messenger.Send(new NavigateRequestMessage(typeof(WebViewHostPage), false, entry));
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
                        entry.ModifyCommand = this.ModifyCommand;
                        entry.RemoveCommand = this.RemoveEntryCommand;
                        entry.NavigateCommand = this.NavigateCommand;
                    });
                    this.Entries = new(list);
                    return;
                }
            }
            this.Entries = new();
        }
        private void SaveEntries()
        {
            Json.ToFile(PathContext.Locate(entriesFileName), this.Entries);
        }
    }
}
