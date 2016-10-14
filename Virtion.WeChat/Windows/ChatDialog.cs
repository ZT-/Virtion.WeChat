using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using Virtion.WeChat.Struct;

namespace Virtion.WeChat.Windows
{
    public abstract class ChatDialog : MetroWindow
    {
        protected User user;

        public abstract void ReceiveMessage(Msg msg);

        public abstract bool SendMessage(string word);

    }
}
