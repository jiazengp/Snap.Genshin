using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Snap.Framework.Extensions.System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate,TSource defaultValue)
        {
            TSource result = source.FirstOrDefault(predicate);
            return result == null ? defaultValue : result;
        }
    }
}
