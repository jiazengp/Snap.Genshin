using DGP.Genshin.Control.WebViewLobby;
using Newtonsoft.Json;
using System.Windows.Input;

namespace DGP.Genshin.DataModel.WebViewLobby
{
    public class WebViewEntry
    {
        public WebViewEntry(string name, string navigateUrl, string? iconUrl, string? javaScript, bool showInNavView = true)
        {
            this.Name = name;
            this.NavigateUrl = navigateUrl;
            this.IconUrl = iconUrl;
            this.JavaScript = javaScript;
            this.ShowInNavView = showInNavView;
        }

        public WebViewEntry(WebViewEntryDialog dialog)
        {
            this.Name = dialog.Name;
            this.NavigateUrl = dialog.NavigateUrl;
            this.IconUrl = dialog.IconUrl;
            this.JavaScript = dialog.JavaScript;
            this.ShowInNavView = dialog.ShowInNavView;
        }

        public string Name { get; set; }
        public string NavigateUrl { get; set; }
        public string? IconUrl { get; set; }
        public string? JavaScript { get; set; }
        public bool ShowInNavView { get; set; } = true;

        [JsonIgnore] public ICommand? ModifyCommand { get; set; }
        [JsonIgnore] public ICommand? RemoveCommand { get; set; }
        [JsonIgnore] public ICommand? NavigateCommand { get; set; }
    }
}
