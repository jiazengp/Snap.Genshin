using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Models.MiHoYo.Request
{
    public class Requester
    {
        public RequestOptions Headers { get; set; }
        public Requester(RequestOptions headers)
        {
            this.Headers = headers;
        }
        private Response<T> Request<T>(Func<WebClient, string> requestMethod)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    foreach (KeyValuePair<string, string> entry in this.Headers)
                    {
                        client.Headers[entry.Key] = entry.Value;
                    }
                    string response = requestMethod(client);
                    Response<T> resp = Json.ToObject<Response<T>>(response);
                    this.Log($"retcode:{resp.ReturnCode} | message:{resp.Message}");
                    return resp;
                }
            }
            catch (Exception ex)
            {
                this.Log($"failed. reason:{ex.Message}");
                return new Response<T>
                {
                    ReturnCode = (int)ReturnCode.Failed,
                    Message = ex.Message
                };
            }
        }
        public Response<T> Get<T>(string url)
        {
            this.Log($"GET {url.Split('?')[0]}");
            return Request<T>(client => client.DownloadString(url));
        }

        public Response<T> Post<T>(string url, dynamic data)
        {
            this.Log($"POST {url.Split('?')[0]}");
            return Request<T>(client => client.UploadString(url, Json.Stringify(data)));
        }

        /// <summary>
        /// 对<see cref="Get{T}(String)"/>方法的异步包装
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<Response<T>> GetAsync<T>(string url) =>
            await Task.Run(() => Get<T>(url));

        /// <summary>
        /// 对<see cref="Post{T}(String, Object)"/>方法的异步包装
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Response<T>> PostAsync<T>(string url, object data) =>
            await Task.Run(() => Post<T>(url, data));
    }
}