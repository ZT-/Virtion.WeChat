using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virtion.WeChat.Server.Wx;

namespace Wechat.API.RPC
{
    public class BatchGetContactResponse
    {
        public BaseResponse BaseResponse;
        public int Count;
        public User[] ContactList;
    }
}
