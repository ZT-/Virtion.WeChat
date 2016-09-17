using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtion.WeChat.Struct
{
    public class WxSendMsg
    {
        public BaseResponse BaseResponse { get; set; }
        public long MsgID { get; set; }
        public string LocalID { get; set; }
    }
}
