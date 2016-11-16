using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtion.WeChat.Struct
{
    public class ChatConfig
    {
        public bool IsMonitor;
        public bool IsMonitorOnMini;
        public string UserMsg;
        public string UserImage;
        public string MsgUserName;
        public string ImageUserName;
        public bool IsFilterUserMsg;
        public bool IsFilterUserImage;
        public bool IsFilterMsgCount;
        public int MaxMsgLength;
        public bool IsFilterAdd;
        public bool IsFilterSelfDef;
        public bool IsHightLight;
        public int Delay;
        public List<string> WhiteList;
        public List<string> DefineList;

        public ChatConfig()
        {
            this.WhiteList = new List<string>();
            this.DefineList = new List<string>();
            this.IsMonitor = false;
            this.IsMonitorOnMini = false;
            this.IsFilterMsgCount = false;
            this.MaxMsgLength = 500;
            this.IsFilterAdd = false;
            this.IsFilterSelfDef = false;
            this.IsHightLight = true;
            this.Delay = 0;
        }
    }

}
