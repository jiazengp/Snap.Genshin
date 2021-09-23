using DGP.Snap.Framework.Attributes;
using System;
using System.Text;

namespace DGP.Genshin.Helpers
{
    [Github("https://github.com/settings/tokens")]
    public class TokenHelper
    {
        public static string GetToken() =>
            Base64Decode(Encoding.UTF8, "Z2hwX3lDRWdVTVNaNnRRV2JpNjZMUWYyTUprbWFQVFI3bTEwYkVnTw==");

        public static string Base64Decode(Encoding encodeType, string result)
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
