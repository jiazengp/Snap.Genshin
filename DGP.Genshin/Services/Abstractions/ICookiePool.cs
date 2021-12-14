using System;
using System.Collections.Generic;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface ICookiePool:IList<string>
    {
        event Action<string>? CookieAdded;
        event Action<string>? CookieRemoved;

        new void Add(string cookie);
        void AddOrIgnore(string cookie);
        new bool Remove(string cookie);
    }
}