using DGP.Genshin.Models.MiHoYo;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.NativeMethods;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace DGP.Genshin.Services
{
    public class LoginService
    {
        protected static readonly string CookieFile = "cookie.dat";
        protected static readonly string CookieUrl = "https://user.mihoyo.com/";
        public string Cookie { get; set; }
        public LoginWindow LoginWindow { get; set; }
        public void Login() => new LoginWindow(this).ShowDialog();
        public void OnLogin(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                this.Cookie = this.GetCookie();
                File.WriteAllText(CookieFile, this.Cookie);
                this.LoginWindow.Close();
            }
            AfterLogin?.Invoke(isLoggedIn);
        }
        public string GetCookie()
        {
            StringBuilder cookieBuilder = new StringBuilder(1024);
            uint size = Convert.ToUInt32(cookieBuilder.Capacity + 1);
            WinInet.InternetGetCookieEx(CookieUrl, null, cookieBuilder, ref size, WinInet.COOKIE_HTTP_ONLY, IntPtr.Zero);
            return cookieBuilder.ToString();
        }

        public bool GetLoginStatus()
        {
            //use my own uid here
            Response<dynamic> response =
                this.Get<dynamic>("https://api-takumi.mihoyo.com/game_record/genshin/api/index?role_id=109719094&server=cn_gf01");
            return response.ReturnCode == 0;
        }
        public event Action<bool> AfterLogin;

        #region API
        private static readonly string APISalt = "w5k9n3aqhoaovgw25l373ee18nsazydo"; // @Azure99    //respect original author
        private static readonly string RandomStringTemplate = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        #region Request Methods
        protected Response<T> Request<T>(Func<WebClient, string> requestMethod, string referer = null)
        {
            try
            {
                using WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                client.Headers["x-rpc-client_type"] = "5";
                client.Headers["x-rpc-app_version"] = "2.9.0";
                client.Headers["DS"] = this.CreateDynamicSecret();
                client.Headers["Cookie"] = this.Cookie;
                string response = requestMethod(client);
                return Json.ToObject<Response<T>>(response);
            }
            catch (Exception ex)
            {
                this.Log("Request data failed");
                return new Response<T>
                {
                    ReturnCode = -1,
                    Message = ex.Message
                };
            }
        }
        public Response<T> Get<T>(string url, string referer = null) => this.Request<T>(x => x.DownloadString(url), referer);
        public Response<T> Post<T>(string url, string data) => this.Request<T>(x => x.UploadString(url, data));
        #endregion

        #region Encryption
        private string CreateDynamicSecret()
        {
            //unix timestamp
            long time = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            string random = this.CreateRandomString(6);
            string check = this.ComputeMd5($"salt={APISalt}&t={time}&r={random}");

            return $"{time},{random},{check}";
        }
        private string CreateRandomString(int length)
        {

            StringBuilder sb = new StringBuilder(length);
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                int pos = random.Next(0, RandomStringTemplate.Length);
                sb.Append(RandomStringTemplate[pos]);
            }

            return sb.ToString();
        }
        private string ComputeMd5(string content)
        {
            using MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(content));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in result)
                builder.Append(b.ToString("x2"));

            return builder.ToString();
        }
        #endregion

        #endregion
    }
}
