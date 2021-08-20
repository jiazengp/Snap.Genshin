using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using System;
using System.Net;
using System.Text;

namespace DGP.Genshin.Models.MiHoYo
{
    public class Requester
    {
        private readonly string cookie;
        private const string appVersion = "2.10.1";
        private static readonly string ActivityReferer = $"https://webstatic.mihoyo.com/bbs/event/signin-ys/index.html?bbs_auth_required=true&act_id=e202009291139501&utm_source=bbs&utm_medium=mys&utm_campaign=icon";

        public string referer = null;

        public Requester(string cookie,string referer=null)
        {
            this.cookie = cookie;
            this.referer = referer ?? ActivityReferer;
        }
        private Response<T> Request<T>(Func<WebClient, string> requestMethod, bool refererRequested)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers["Accept"] = "application/json";
                    client.Headers["x-rpc-client_type"] = "5";
                    client.Headers["x-rpc-app_version"] = appVersion;
                    client.Headers["DS"] = DynamicSecretProvider.Create();
                    if (refererRequested)
                    {
                        client.Headers["User-Agent"] = "Mozilla/5.0 (Linux; Android 6.0.1; MuMu Build/V417IR; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/68.0.3440.70 Mobile Safari/537.36 miHoYoBBS/2.10.1";
                        client.Headers["x-rpc-device_id"] = Guid.NewGuid().ToString("D");
                        client.Headers["X-Requested-With"] = "com.mihoyo.hyperion";
                        client.Headers["Referer"] = referer;
                    }
                    client.Headers["Cookie"] = this.cookie;
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
        public Response<T> Get<T>(string url, bool isSignInRequest = false) => Request<T>(x => x.DownloadString(url), isSignInRequest);
        public Response<T> Post<T>(string url, dynamic data, bool isSignInRequest = false) => Request<T>(x => x.UploadString(url, Json.Stringify(data)), isSignInRequest);
        public bool Check<T>(string url) => Get<T>(url).ReturnCode == 0;
    }

}