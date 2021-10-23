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

        private static string Base64Decode(Encoding encodeType, string result)
        {
            string decode;
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = encodeType.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }
    }
}
