using Microsoft.Toolkit.Uwp.Notifications;

namespace DGP.Genshin.Helpers
{
    public class NotificationHelper
    {
        public static void SendNotification()
        {
            new ToastContentBuilder()
                .AddText("测试")
                .AddButton(new ToastButton().SetContent("确定"))
                .AddButton(new ToastButtonDismiss())
                .Show();
        }
    }
}
