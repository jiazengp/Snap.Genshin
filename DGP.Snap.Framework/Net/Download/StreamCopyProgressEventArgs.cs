using System;

namespace DGP.Genshin.Common.Net.Download
{
    internal class StreamCopyProgressEventArgs : EventArgs
    {
        public long BytesReceived { get; set; }
    }
}