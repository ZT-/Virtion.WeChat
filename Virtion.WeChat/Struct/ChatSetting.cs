using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtion.WeChat.Struct
{
    public class ChatConfig
    {
        public bool IsMonitor;
        public bool IsFilterMsg;
        public int MaxMsgLength;
        public bool IsFilterAdd;
        public bool IsFilterSelf;
        public bool IsHightLight;
        public List<string> WhiteList;
        public ChatConfig()
        {
            this.WhiteList = new List<string>();
            this.IsMonitor = false;
            this.IsFilterMsg = true;
            this.MaxMsgLength = 500;
            this.IsFilterAdd = true;
            this.IsFilterSelf = false;
            this.IsHightLight = true;
        }
    }

}
