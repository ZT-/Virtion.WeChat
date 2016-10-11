using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtion.WeChat.Struct
{
    public class ChatConfig
    {
        public bool IsMonitor;
        public string UserMsg;
        public bool IsFilterUserMsg;
        public bool IsFilterMsg;
        public int MaxMsgLength;
        public bool IsFilterAdd;
        public bool IsFilterSelf;
        public bool IsHightLight;
        public int Delay;
        public List<string> WhiteList;
        public List<string> DefineList;

        public ChatConfig()
        {
            this.WhiteList = new List<string>();
            this.DefineList = new List<string>();
            this.IsMonitor = false;
            this.IsFilterMsg = true;
            this.MaxMsgLength = 500;
            this.IsFilterAdd = false;
            this.IsFilterSelf = false;
            this.IsHightLight = true;
            this.Delay = 0;
        }
    }

}
