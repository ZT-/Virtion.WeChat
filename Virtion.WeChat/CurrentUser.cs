using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Media;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using Virtion.WeChat.Struct;
using Virtion.WeChat.Windows;

namespace Virtion.WeChat
{
    public static class CurrentUser
    {
        public static string Skey;
        public static string WxSid;
        public static string WxUin;
        public static string WebWxDataTicket;
        public static string PassTicket;
        public static string DeviceId 
        {
            get
            {
                Random r= new Random();
                return "e" + 179213476367295 + r.Next(9999999);
            }
        }
        public static string Cookie;
        public static SyncKey Synckey;
        public static BaseRequest BaseRequest;

        //个人信息
        public static User Me;
        //会话列表
        public static Dictionary<string, User> ChatTable = new Dictionary<string, User>();
        //通讯录列表
        public static Dictionary<string, User> ContactTable = new Dictionary<string, User>();
        //消息列表
        public static Dictionary<string, List<Msg>> MessageTable = new Dictionary<string, List<Msg>>();
        //窗口列表
        public static Dictionary<string, ChatDialog> DialogTable = new Dictionary<string, ChatDialog>();


    }
}
