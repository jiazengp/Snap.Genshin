using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
