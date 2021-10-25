using System;

namespace DGP.Genshin.Common.Net.Download
{
    internal class StreamCopyCompleteEventArgs : EventArgs
    {
        public CompletedState CompleteState { get; set; }
        public Exception? Exception { get; set; }
    }
}