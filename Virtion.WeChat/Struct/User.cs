using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Virtion.Util;

namespace Virtion.WeChat.Struct
{
    public class User
    {
        public long Uin { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string HeadImgUrl { get; set; }
        public string RemarkName { get; set; }
        public string PYInitial { get; set; }
        public string PYQuanPin { get; set; }
        public string RemarkPYInitial { get; set; }
        public string RemarkPYQuanPin { get; set; }
        public int HideInputBarFlag { get; set; }
        public int StarFriend { get; set; }
        public int Sex { get; set; }
        public string Signature { get; set; }
        public int AppAccountFlag { get; set; }
        public int VerifyFlag { get; set; }
        public int ContactFlag { get; set; }
        public int SnsFlag { get; set; }

        //me
        public int WebWxPluginSwitch { get; set; }
        public int HeadImgFlag { get; set; }

        //friend
        public int MemberCount { get; set; }
        public User[] MemberList { get; set; }
        public long OwnerUin { get; set; }
        public int Statues { get; set; }
        public long AttrStatus { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Alias { get; set; }
        public int UniFriend { get; set; }
        public string DisplayName { get; set; }
        public int ChatRoomId { get; set; }

        public int MemberStatus { get; set; }

        public string EncryChatRoomId;

        public string PseudoUID
        {
            get
            {
                return MD5Helper.StringToMD5(
                    this.NickName
                + this.RemarkName
                + this.Province
                + this.City
                + this.Signature);
            }
        }

        public void SetDisplayName()
        {
            if (string.IsNullOrEmpty(RemarkName) == true)
            {
                if (string.IsNullOrEmpty(NickName) == true)
                {
                    if (string.IsNullOrEmpty(DisplayName) == true)
                    {
                        DisplayName = UserName;
                    }
                }
                else
                {
                    DisplayName = NickName;
                }
            }
            else
            {
                DisplayName = RemarkName;
            }

            var startPos = DisplayName.IndexOf("<span", StringComparison.Ordinal);
            while (startPos > -1)
            {
                var endPos = DisplayName.IndexOf("</span>", StringComparison.Ordinal);
                string emjio = DisplayName.Substring(startPos, endPos - startPos + 7);
                DisplayName = DisplayName.Replace(emjio, "[]");
                startPos = DisplayName.IndexOf("<span", StringComparison.Ordinal);
            }
        }


    }
}
