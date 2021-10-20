using System;
using System.Net;
using System.Net.Mime;

namespace DGP.Snap.Framework.Extensions.System.Net
{
    public static class WebHeaderCollectionExtensions
    {
        public static long GetContentLength(this WebHeaderCollection responseHeaders)
        {
            long contentLength = -1;
            if (responseHeaders != null && !String.IsNullOrEmpty(responseHeaders["Content-Length"]))
            {
                Int64.TryParse(responseHeaders["Content-Length"], out contentLength);
            }
            return contentLength;
        }

        public static ContentDisposition? GetContentDisposition(this WebHeaderCollection responseHeaders)
        {
            if (responseHeaders != null && !String.IsNullOrEmpty(responseHeaders["Content-Disposition"]))
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
