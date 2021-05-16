using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Snap.GHHW.Services
{
    class WebClientService
    {
        /// <summary>
        /// 获取网页响应
        /// </summary>
        /// <param name="requestUrl">请求的URL</param>
        /// <returns>响应字符串</returns>
        public static async Task<string> GetWebResponse(string requestUrl)
        {
            HttpWebRequest request = WebRequest.CreateHttp(requestUrl);
            //为了能正常的获取GitHub的数据
            //request.Proxy = WebRequest.DefaultWebProxy;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "GET";
            request.ContentType = "application/json;charset=UTF-8";
            request.UserAgent = "Wget/1.9.1";
            request.Timeout = 5000;
            string jsonMetaString;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())//获取响应
            {
                using (StreamReader responseStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    jsonMetaString = responseStreamReader.ReadToEnd();
                }
            }
            return jsonMetaString;
        }
    }
}
