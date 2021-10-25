using System;

namespace DGP.Genshin.Common.Exceptions
{
    public class UrlNotFoundException : Exception
    {
        public UrlNotFoundException(string message) : base(message) { }

    }
}
