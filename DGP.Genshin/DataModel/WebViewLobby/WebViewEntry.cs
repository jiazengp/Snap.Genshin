namespace DGP.Genshin.DataModel.WebViewLobby
{
    public class WebViewEntry
    {
        public WebViewEntry(string navigateUrl, string? javaScript = null, int executeDelaySecond = 0)
        {
            NavigateUrl = navigateUrl;
            JavaScript = javaScript;
            ExecutionSecondsDelay = executeDelaySecond;
        }

        public string NavigateUrl { get; set; }
        public string? JavaScript { get; set; }
        public int ExecutionSecondsDelay { get; set; }
    }
}
