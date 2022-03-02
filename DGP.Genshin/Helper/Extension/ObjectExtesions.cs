using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DGP.Genshin.Helper.Extension
{
    internal static class ObjectExtesions
    {
        public static DispatcherOperation<TResult> ExecuteOnUIAsync<TResult>(this Func<TResult> func)
        {
            return App.Current.Dispatcher.InvokeAsync(func);
        }

        public static async Task<TResult> ExecuteOnUIAsync<TResult>(this object obj, Func<Task<TResult>> func)
        {
            return await App.Current.Dispatcher.InvokeAsync(func).Task.Unwrap();
        }
    }
}
