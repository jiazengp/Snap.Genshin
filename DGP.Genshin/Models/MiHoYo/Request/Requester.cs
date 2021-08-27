using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

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
                    this.Log($"return code:{resp.ReturnCode}");
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
        public Response<T> Get<T>(string url) =>
            Request<T>(client => client.DownloadString(url));
        public Response<T> Post<T>(string url, dynamic data) =>
            Request<T>(client => client.UploadString(url, Json.Stringify(data)));
    }
}