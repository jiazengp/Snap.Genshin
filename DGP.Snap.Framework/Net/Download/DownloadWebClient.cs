using DGP.Genshin.Common.Extensions.System.Net;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace DGP.Genshin.Common.Net.Download
{
    [DesignerCategory("Code")]
    internal class DownloadWebClient : WebClient
    {
        private readonly CookieContainer cookieContainer = new CookieContainer();
        private WebResponse? webResponse;
        private long position;

        private readonly TimeSpan timeout = TimeSpan.FromMinutes(2);

        public bool HasResponse => webResponse != null;

        public bool IsPartialResponse => webResponse is HttpWebResponse response && response.StatusCode == HttpStatusCode.PartialContent;

        public void OpenReadAsync(Uri address, long newPosition)
        {
            position = newPosition;
            OpenReadAsync(address);
        }

        public string? GetOriginalFileNameFromDownload()
        {
            if (webResponse == null)
            {
                return null;
            }

            try
            {
                System.Net.Mime.ContentDisposition? contentDisposition = webResponse.Headers.GetContentDisposition();
                if (contentDisposition != null)
                {
                    string? filename = contentDisposition.FileName;
                    if (!string.IsNullOrEmpty(filename))
                    {
                        return Path.GetFileName(filename);
                    }
                }
                return Path.GetFileName(webResponse.ResponseUri.LocalPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            webResponse = response;
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            webResponse = response;

            return response;
        }

#nullable disable
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            if (request != null)
            {
                request.Timeout = (int)timeout.TotalMilliseconds;
            }

            if (request is not HttpWebRequest webRequest)
            {
                return request;
            }

            webRequest.ReadWriteTimeout = (int)timeout.TotalMilliseconds;
            webRequest.Timeout = (int)timeout.TotalMilliseconds;
            if (position != 0)
            {
                webRequest.AddRange((int)position);
                webRequest.Accept = "*/*";
            }
            webRequest.CookieContainer = cookieContainer;
            return request;
        }
#nullable enable
    }
}