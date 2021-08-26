using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DGP.Snap.Framework.Extensions.System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue)
        {
            TSource result = source.FirstOrDefault(predicate);
            return result == null ? defaultValue : result;
        }

        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> asyncAction, int maxThreadCount)
        {
            SemaphoreSlim throttler = new SemaphoreSlim(initialCount: maxThreadCount);
            IEnumerable<Task> tasks = source.Select(async item =>
            {
                await throttler.WaitAsync();
                try
                {
                    await asyncAction(item).ConfigureAwait(false);
                }
                finally
                {
                    throttler.Release();
                }
            });
            await Task.WhenAll(tasks);
        }
    }
}
