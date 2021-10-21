using DGP.Snap.Framework.Attributes;
using DGP.Snap.Framework.Core.Logging;
using DGP.Snap.Framework.Data.Json;
using System;
using System.Linq;

namespace DGP.Genshin.Models.MiHoYo.Request
{
    /// <summary>
    /// 为MiHoYo接口请求器 <see cref="Requester"/> 提供2代动态密钥
    /// </summary>
    [Github("https://github.com/Azure99/GenshinPlayerQuery/issues/20")]
    internal class DynamicSecretProvider2 : Md5DynamicSecretProviderBase
    {
        /// <summary>
        /// 防止从外部创建 <see cref="DynamicSecretProvider2"/> 的实例
        /// </summary>
        private DynamicSecretProvider2() { }
        public const string AppVersion = "2.11.1";

        [Github("https://github.com/Azure99/GenshinPlayerQuery/blob/main/src/Core/GenshinAPI.cs")]
        private static readonly string APISalt = "xV8v4Qu54lUKrEYFZkJhB8cuOh9Asafs"; // @Azure99 respect original author

        public static string Create(string queryUrl, object? postBody = null)
        {
            //unix timestamp
            int t = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            //random
            string r = GetRandomString();
            //body
            string b = postBody is null ? "" : Json.Stringify(postBody);
            //query
            string q = "";
            string[] url = queryUrl.Split('?');
            if (url.Length == 2)
            {
                string[] queryParams = url[1].Split('&').OrderBy(x => x).ToArray();
                q = String.Join("&", queryParams);
            }
            //check
            string check = GetComputedMd5($"salt={APISalt}&t={t}&r={r}&b={b}&q={q}");
            string result = $"{t},{r},{check}";
            Logger.LogStatic(typeof(DynamicSecretProvider2), $"generated DS : {result}");
            return result;
        }
        private static readonly Random random = new Random();
        private static string GetRandomString() =>
            random.Next(100000, 200000).ToString();
    }
}
