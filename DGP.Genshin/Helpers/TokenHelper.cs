using System;
using System.Text;

namespace DGP.Genshin.Helpers
{
    /// <summary>
    /// because repo cant cantain original token string
    /// so we store base64 encoded value here
    /// https://github.com/settings/tokens
    /// </summary>
    public class TokenHelper : Base64Converter
    {
        public static string GetToken()
        {
            return Base64Decode(Encoding.UTF8, "Z2hwX3lDRWdVTVNaNnRRV2JpNjZMUWYyTUprbWFQVFI3bTEwYkVnTw==");
        }
    }
}
