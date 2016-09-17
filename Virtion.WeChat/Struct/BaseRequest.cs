using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtion.WeChat.Struct
{
    public class BaseRequest
    {
        public string Uin { get; set; }
        public string Sid { get; set; }
        public string Skey { get; set; }
        public string DeviceID { get; set; }

        public BaseRequest(string wxuin, string wxsid, string skey, string device_id)
        {
            Uin = CurrentUser.WxUin;
            Sid = CurrentUser.WxSid;
            Skey = CurrentUser.Skey;
            DeviceID = CurrentUser.DeviceId;
        }
    }
}
