using System;
using Virtion.WeChat.Util;

namespace Virtion.WeChat.Server.Wx
{
    public class User
    {
        public long Uin;
        public string UserName;
        public string NickName;
        public string HeadImgUrl;
        public int ContactFlag;
        public int MemberCount;
        public User[] MemberList;
        public string RemarkName;
        public int Sex;
        public string Signature;
        public int VerifyFlag;
        public long OwnerUin;
        public string PYInitial;
        public string PYQuanPin;
        public string RemarkPYInitial;
        public string RemarkPYQuanPin;
        public int StarFriend;
        public int AppAccountFlag;
        public int Statues;
        public int AttrStatus;
        public string Province;
        public string City;
        public string Alias;
        public int SnsFlag;
        public int UniFriend;
        public int ChatRoomId;
        public string KeyWord;
        public string EncryChatRoomId;

        private string displayName;
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.displayName) == true)
                {
                    if (string.IsNullOrEmpty(this.RemarkName) == true)
                    {
                        if (string.IsNullOrEmpty(this.NickName) == true)
                        {
                            this.displayName = this.UserName;
                        }
                        else
                        {
                            this.displayName = this.NickName;
                        }
                    }
                    else
                    {
                        this.displayName = this.RemarkName;
                    }

                    var startPos = this.displayName.IndexOf("<span", StringComparison.Ordinal);
                    while (startPos > -1)
                    {
                        var endPos = this.displayName.IndexOf("</span>", StringComparison.Ordinal);
                        string emjio = this.displayName.Substring(startPos, endPos - startPos + 7);
                        this.displayName = this.displayName.Replace(emjio, "[]");
                        startPos = this.displayName.IndexOf("<span", StringComparison.Ordinal);
                    }
                }

                return this.displayName;
            }
        }
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


    }
}
