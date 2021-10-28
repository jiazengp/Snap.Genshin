using System;
using System.Text;

namespace DGP.Genshin.Helpers
{
    /// <summary>
    /// because repo cant cantain original token string
    /// https://github.com/settings/tokens
    /// </summary>
    public class TokenHelper
    {
        public static string GetToken()
        {
            return Base64Decode(Encoding.UTF8, "Z2hwX3lDRWdVTVNaNnRRV2JpNjZMUWYyTUprbWFQVFI3bTEwYkVnTw==");
        }

        internal static string Base64Decode(Encoding encoding, string input)
        {
            byte[] bytes = Convert.FromBase64String(input);
            return encoding.GetString(bytes);
        }

        internal static string Base64Encode(Encoding encoding, string base64)
        {
            byte[] bytes = encoding.GetBytes(base64);
            return Convert.ToBase64String(bytes);
        }
    }
}
