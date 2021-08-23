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
        private Response<T> Request<T>(Func<WebClient, string> requestMethod, bool isRefererRequired)
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
                    return Json.ToObject<Response<T>>(response);
                }
            }
            catch (Exception ex)
            {
                this.Log("Request data failed");
                return new Response<T>
                {
                    ReturnCode = (int)ReturnCode.Failed,
                    Message = ex.Message
                };
            }
        }
        public Response<T> Get<T>(string url, bool isRefererRequired = false) =>
            Request<T>(x => x.DownloadString(url), isRefererRequired);
        public Response<T> Post<T>(string url, dynamic data, bool isRefererRequired = false) =>
            Request<T>(x => x.UploadString(url, Json.Stringify(data)), isRefererRequired);
        public bool Check<T>(string url) =>
            Get<T>(url).ReturnCode == 0;
    }
}