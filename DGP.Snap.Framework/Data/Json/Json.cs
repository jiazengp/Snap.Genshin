using DGP.Snap.Framework.Core.Logging;
using Newtonsoft.Json;
using System;
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
        public static T ToObject<T>(string value) =>
            JsonConvert.DeserializeObject<T>(value);

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
        /// 从文件中读取后转化为实体类
        /// </summary>
        /// <typeparam name="T">要反序列化的对象的类型</typeparam>
        /// <param name="fileName">存放JSON数据的文件路径</param>
        /// <returns>JSON字符串中的反序列化对象</returns>
        public static T FromFile<T>(string fileName)
        {
            using (StreamReader sr = File.OpenText(fileName))
            {
                return ToObject<T>(sr.ReadToEnd());
            }
        }
        /// <summary>
        /// 从文件中读取后转化为实体类
        /// </summary>
        /// <typeparam name="T">要反序列化的对象的类型</typeparam>
        /// <param name="fileName">存放JSON数据的文件路径</param>
        /// <returns>JSON字符串中的反序列化对象</returns>
        public static T FromFile<T>(FileInfo file)
        {
            using (StreamReader sr = file.OpenText())
            {
                return ToObject<T>(sr.ReadToEnd());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="value"></param>
        public static void ToFile(string fileName, object value)
        {
            using (StreamWriter sw = new StreamWriter(File.OpenWrite(fileName)))
            {
                sw.Write(Stringify(value));
            }
        }

        public static T FromWebsite<T>(string url)
        {
            Logger.LogStatic(typeof(Json), $"GET {url}");
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string response = client.DownloadString(url);
                    return ToObject<T>(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogStatic(typeof(Json), $"web request failed. reason:{ex.Message}");
                return default;
            }
        }
    }
}
