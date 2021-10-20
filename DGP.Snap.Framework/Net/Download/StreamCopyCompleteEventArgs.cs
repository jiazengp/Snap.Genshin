using System;

namespace DGP.Snap.Framework.Net.Download
{
    internal class StreamCopyCompleteEventArgs : EventArgs
    {
        public CompletedState CompleteState { get; set; }
        public Exception? Exception { get; set; }
    }
}