using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtion.WeChat.Struct
{
    public class WxContact
    {
        public BaseResponse BaseResponse { get; set; }
        public int MemberCount { get; set; }
        public User[] MemberList { get; set; }
    }
}
