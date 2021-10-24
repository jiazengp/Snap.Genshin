using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Services.CelestiaDatabase
{
    public static class LINQHelper
    {
        public static IEnumerable<T> TakeWhileAndPreserve<T>(this IEnumerable<T> source, Func<T, bool> predicate, int leastCount)
        {
            IEnumerable<T> result = source.TakeWhile(predicate);
            if (result.Count() < leastCount)
            {
                result = source.Take(10);
            }
            return result;
        }
    }
}
