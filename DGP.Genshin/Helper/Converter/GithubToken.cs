using System.Text;

namespace DGP.Genshin.Helper.Converter
{
    /// <summary>
    /// because repo cant cantain original token string
    /// so we store base64 encoded value here
    /// https://github.com/settings/tokens
    /// </summary>
    internal class GithubToken : Base64Converter
    {
        private GithubToken() { }
        public static string GetToken()
        {
            return Base64Decode(Encoding.UTF8, "Z2hwX3lDRWdVTVNaNnRRV2JpNjZMUWYyTUprbWFQVFI3bTEwYkVnTw==");
        }
    }
}
