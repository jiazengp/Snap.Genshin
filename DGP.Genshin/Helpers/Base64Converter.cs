using System;
using System.Text;

namespace DGP.Genshin.Helpers
{
    public class Base64Converter
    {

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