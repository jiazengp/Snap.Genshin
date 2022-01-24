using Microsoft.Toolkit.Uwp.Notifications;

namespace DGP.Genshin.Helper.Notification
{
    public static class ToastContentBuilderExtensions
    {
        public static ToastContentBuilder AddSignInHeader(this ToastContentBuilder builder, string title)
        {
            return builder.AddHeader("SIGNIN", title, "SIGNIN");
        }
    }
}