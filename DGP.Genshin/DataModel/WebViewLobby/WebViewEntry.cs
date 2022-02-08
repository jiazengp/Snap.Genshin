using Newtonsoft.Json;
using System.Windows.Input;

namespace DGP.Genshin.DataModel.WebViewLobby
{
    public class WebViewEntry
    {
        public WebViewEntry(string name, string navigateUrl,string? iconUrl, string? javaScript)
        {
            Name = name;
            NavigateUrl = navigateUrl;
            IconUrl = iconUrl;
            JavaScript = javaScript;
        }

        public string Name { get; set; }
        public string NavigateUrl { get; set; }
        public string? IconUrl { get; set; }
        public string? JavaScript { get; set; }

        [JsonIgnore] public ICommand? ModifyCommand { get; set; }
        [JsonIgnore] public ICommand? RemoveCommand { get; set; }
    }
}
