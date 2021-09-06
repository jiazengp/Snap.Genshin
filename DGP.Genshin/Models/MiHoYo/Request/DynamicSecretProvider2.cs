using DGP.Snap.Framework.Attributes;
using DGP.Snap.Framework.Core.Logging;
using DGP.Snap.Framework.Data.Json;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DGP.Genshin.Models.MiHoYo.Request
{
    /// <summary>
    /// 为MiHoYo接口请求器 <see cref="Requester"/> 提供2代动态密钥
    /// </summary>
    internal static class DynamicSecretProvider2
    {
        public const string AppVersion = "2.11.1";

        [Github("https://github.com/Azure99/GenshinPlayerQuery/blob/main/src/Core/GenshinAPI.cs")]
        private static readonly string APISalt = "xV8v4Qu54lUKrEYFZkJhB8cuOh9Asafs"; // @Azure99 respect original author

        public static string Create(string queryUrl, object postBody = null)
        {
            string q = "";
            string[] url = queryUrl.Split('?');
            if (url.Length == 2)
            {
                //dictionary order
                string[] queryParams = url[1].Split('&').OrderBy(x => x).ToArray();
                q = string.Join("&", queryParams);
            }
            //unix timestamp
            int time = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            string random = GetRandomString(6);

            //lulu666lulu https://github.com/Azure99/GenshinPlayerQuery/issues/20
            string target = $"salt={APISalt}&t={time}&r={random}&b={(postBody == null ? "" : Json.Stringify(postBody))}&q={q}";
            string check = GetComputedMd5(target);
            Logger.LogStatic(typeof(DynamicSecretProvider2), $"generated DS:{time},{random},{check}");
            return $"{time},{random},{check}";
        }
        private static string GetRandomString(int length)
        {
            StringBuilder builder = new StringBuilder(length);

            const string randomStringTemplate = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                int pos = random.Next(0, randomStringTemplate.Length);
                builder.Append(randomStringTemplate[pos]);
            }

            return builder.ToString();
        }
        private static string GetComputedMd5(string content)
        {
            using MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(content));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in result)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
