using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Helper.Extension
{
    public static class ObjectExtesions
    {
        public static async Task<TResult> ExecuteOnUIAsync<TResult>(this object _, Func<Task<TResult>> func)
        {
            return await App.Current.Dispatcher.InvokeAsync(func).Task.Unwrap();
        }
        public static void ExecuteOnUI(this object _, Action action)
        {
            App.Current.Dispatcher.Invoke(action);
        }
    }
}
