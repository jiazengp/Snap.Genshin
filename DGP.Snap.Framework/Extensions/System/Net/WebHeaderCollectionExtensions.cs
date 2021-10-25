using System.Net;
using System.Net.Mime;

namespace DGP.Genshin.Common.Extensions.System.Net
{
    public static class WebHeaderCollectionExtensions
    {
        public static long GetContentLength(this WebHeaderCollection responseHeaders)
        {
            long contentLength = -1;
            if (responseHeaders != null && !string.IsNullOrEmpty(responseHeaders["Content-Length"]))
            {
                long.TryParse(responseHeaders["Content-Length"], out contentLength);
            }
            return contentLength;
        }

        public static ContentDisposition? GetContentDisposition(this WebHeaderCollection responseHeaders)
        {
            if (responseHeaders != null && !string.IsNullOrEmpty(responseHeaders["Content-Disposition"]))
            {
                string? pos = responseHeaders["Content-Disposition"];
                if (pos is not null)
                {
                    return new ContentDisposition(pos);
                }

            }
            return null;
        }
    }
}
