using DGP.Genshin.Models.MiHoYo.Record.Avatar;
using DGP.Genshin.Services;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Models.MiHoYo.Record
{
    internal class RecordAPI
    {
        private static readonly string APISalt = "w5k9n3aqhoaovgw25l373ee18nsazydo"; // @Azure99    //respect original author
        private static readonly string APIAppVersion = "2.9.0";
        private static readonly string APIClientType = "5";
        private static readonly string RandomStringTemplate = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string APIBaseUrl = $@"https://api-takumi.mihoyo.com/game_record/genshin/api";
        public bool GetLoginStatus()
        {
            Response<JObject> response =
                this.Get<JObject>(
                    "https://api-takumi.mihoyo.com/game_record/genshin/api/index?role_id=100010001&server=cn_gf01");
            return response.ReturnCode == 0;
        }
        public async Task<Record> GetRecordAsync(string uid)
        {

            string server = null;
            try
            {
                server = new Dictionary<char, string>()
                {
                    { '1', "cn_gf01" },
                    { '5', "cn_qd01" },
                    //{ '6', "os_usa" },
                    //{ '7', "os_euro" },
                    //{ '8', "os_asia" },
                    //{ '9', "os_cht" }
                }[uid[0]];
            }
            catch
            {
                return new Record("不支持查询此UID");
            }

            Response<PlayerInfo> playerInfo = await Task.Run(() =>
            {
                return this.Get<PlayerInfo>(
                    $@"{APIBaseUrl}/index?role_id={uid}&server={server}");
            });
            if (playerInfo.ReturnCode != 0)
                return new Record($"获取玩家基本信息失败：\n{playerInfo.Message}");

            Response<SpiralAbyss.SpiralAbyss> spiralAbyss = await Task.Run(() =>
            {
                return this.Get<SpiralAbyss.SpiralAbyss>(
                    $@"{APIBaseUrl}/spiralAbyss?schedule_type=1&server={server}&role_id={uid}");
            });
            if (spiralAbyss.ReturnCode != 0)
                return new Record($"获取本期深渊螺旋信息失败：\n{spiralAbyss.Message}");

            Response<SpiralAbyss.SpiralAbyss> lastSpiralAbyss = await Task.Run(() =>
            {
                return this.Get<SpiralAbyss.SpiralAbyss>(
                   $@"{APIBaseUrl}/spiralAbyss?schedule_type=2&server={server}&role_id={uid}");
            });
            if (lastSpiralAbyss.ReturnCode != 0)
                return new Record($"获取上期深渊螺旋信息失败：\n{lastSpiralAbyss.Message}");

            Response<DetailedAvatarInfo> roles = await Task.Run(() =>
            {
                return this.Post<DetailedAvatarInfo>(
                   $@"{APIBaseUrl}/character",
                   Json.Stringify(new CharacterQueryPostData
                   {
                       CharacterIds = playerInfo.Data.Avatars.Select(x => x.Id).ToList(),
                       RoleId = uid,
                       Server = server
                   }));
            });

            return roles.ReturnCode != 0 ? new Record(roles.Message) : new Record
            {
                Success = true,
                UserId = uid,
                Server = server,
                PlayerInfo = playerInfo.Data,
                SpiralAbyss = spiralAbyss.Data,
                LastSpiralAbyss = lastSpiralAbyss.Data,
                DetailedAvatars = roles.Data.Avatars
            };
        }
        #region Request Methods
        private Response<T> Request<T>(Func<WebClient, string> requestFunc)
        {
            try
            {
                using WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                client.Headers["x-rpc-client_type"] = APIClientType;
                client.Headers["x-rpc-app_version"] = APIAppVersion;
                client.Headers["DS"] = this.CreateDynamicSecret();
                client.Headers["Cookie"] = RecordService.Instance.LoginTicket;
                string response = requestFunc(client);
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
        private Response<T> Get<T>(string url) => this.Request<T>(x => x.DownloadString(url));
        private Response<T> Post<T>(string url, string data) => this.Request<T>(x => x.UploadString(url, data));
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
            using (MD5 md5 = MD5.Create())
            {
                byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(content ?? ""));

                StringBuilder builder = new StringBuilder();
                foreach (byte b in result)
                    builder.Append(b.ToString("x2"));

                return builder.ToString();
            }
        }
        #endregion
        #region 单例
        private static RecordAPI instance;
        private static readonly object _lock = new();
        private RecordAPI()
        {
        }
        public static RecordAPI Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new RecordAPI();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
