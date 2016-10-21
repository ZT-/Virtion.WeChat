using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virtion.WeChat.Server.Wx;

namespace Wechat.API.RPC
{
    public class GetContactResponse
    {
        public BaseResponse BaseResponse;
        public int MemberCount;
        public User[] MemberList;
        public int Seq;
    }
}
