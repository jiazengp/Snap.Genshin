using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Messages
{
    public class CookieChangedMessage : ValueChangedMessage<string>
    {
        public CookieChangedMessage(string cookie) : base(cookie)
        {
        }
    }
}
