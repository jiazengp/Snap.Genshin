using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;

namespace DGP.Snap.Framework.Data.Json
{
    public static class Json
    {
        /// <summary>	
        /// 将JSON反序列化为指定的.NET类型	
        /// </summary>	
        /// <typeparam name="T">要反序列化的对象的类型</typeparam>	
        /// <param name="value">要反序列化的JSON</param>	
        /// <returns>JSON字符串中的反序列化对象</returns>	
        public static T ToObject<T>(string value) => JsonConvert.DeserializeObject<T>(value);

        /// <summary>	
        /// 将指定的对象序列化为JSON字符串	
        /// </summary>	
        /// <param name="value">要序列化的对象</param>	
        /// <returns>对象的JSON字符串表示形式</returns>	
        public static string Stringify(object value)
        {
            //set date format string to make it compatible to gachaData
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                //兼容原神api格式
                DateFormatString = "yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFFFFFK",
                Formatting = Formatting.Indented,
            };
            return JsonConvert.SerializeObject(value, jsonSerializerSettings);
        }

        /// <summary>	
        /// 向指定 <paramref name="requestUrl"/> 的服务器请求Json数据，并将结果返回为类型为 <typeparamref name="TResponse"/> 的实例	
        /// </summary>	
        /// <typeparam name="TResponse"></typeparam>	
        /// <param name="requestUrl"></param>	
        /// <returns></returns>	
        public static TResponse GetWebResponseObject<TResponse>(string requestUrl)
        {
            string jsonMetaString = GetWebResponse(requestUrl);
            return ToObject<TResponse>(jsonMetaString);
        }

        /// <summary>	
        /// 简单获取网页响应	
        /// </summary>	
        /// <param name="requestUrl">请求的URL</param>	
        /// <returns>响应字符串</returns>	
        private static string GetWebResponse(string requestUrl)
        {
            HttpWebRequest request = WebRequest.CreateHttp(requestUrl);

            request.Proxy = WebRequest.DefaultWebProxy;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.ContentType = "application/json;charset=UTF-8";

            request.Timeout = 5000;
            string jsonMetaString;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using StreamReader responseStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                jsonMetaString = responseStreamReader.ReadToEnd();
            }
            return jsonMetaString;
        }
    }
}
