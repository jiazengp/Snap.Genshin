using System.Collections.Generic;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface ICookiePool : IList<string>
    {
        new void Add(string cookie);
        void AddOrIgnore(string cookie);
        new bool Remove(string cookie);
    }
}