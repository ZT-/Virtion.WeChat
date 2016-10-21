using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Virtion.WeChat.Server.Wx;
using Virtion.WeChat.Util;
using Wechat.API;
using Wechat.API.RPC;

namespace Virtion.WeChat.Server
{
    public class WechatApiService
    {
        HttpClient http;
        public static string Server = "wx";//wx, wx2

        public static string GetAvatorUrl
        {
            get
            {
                return "http://" + Server + ".qq.com";
            }
        }

        public WechatApiService(HttpClient httpClient)
        {
            http = httpClient;
        }

        /// <summary>
        /// 获得二维码登录SessionID,使用此ID可以获得登录二维码
        /// </summary>
        /// <returns>Session</returns>
        public string GetNewQrLoginSessionId()
        {
            //respone like this => window.QRLogin.code = 200; window.QRLogin.uuid = "Qa_GBH_IqA==";
            string url = "https://login.weixin.qq.com/jslogin?appid=wx782c26e4c19acffb";
            byte[] bytes = http.GET(url);
            string str = Encoding.UTF8.GetString(bytes);
            if (str == "\0")
            {
                MessageBox.Show("网络错误");
                App.Current.Shutdown();
            }
            string sessionId = str.Split(new string[] { "\"" }, StringSplitOptions.None)[1];
            return sessionId;
        }

        /// <summary>
        /// 获得登录二维码URL
        /// </summary>
        /// <param name="qrLoginSessionId"></param>
        /// <returns></returns>
        public string GetQrCodeUrl(string qrLoginSessionId)
        {
            string url = "https://login.weixin.qq.com/qrcode/" + qrLoginSessionId;
            return url;
        }

        /// <summary>
        /// 获得登录二维码图片
        /// </summary>
        /// <param name="qrLoginSessionId"></param>
        /// <returns></returns>
        public string GetQrCodeImage(string qrLoginSessionId)
        {
            string url = GetQrCodeUrl(qrLoginSessionId);

            //var bytes = http.GET(url);
            return url; //Image.FromStream(new MemoryStream(bytes));
        }

        /// <summary>
        /// 登录检查
        /// </summary>
        /// <param name="qrLoginSessionId"></param>
        /// <returns></returns>
        public LoginResult Login(string qrLoginSessionId)
        {
            string url = "https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?loginicon=true&uuid=" + qrLoginSessionId;
            byte[] bytes = http.GET(url);
            string loginResult = Encoding.UTF8.GetString(bytes);
            LoginResult result = new LoginResult();
            result.code = 408;
            if (loginResult.Contains("window.code=201")) //已扫描 未登录
            {
                var arr = loginResult.Split(new string[] { "\'" }, StringSplitOptions.None);
                if (arr.Length > 1)
                {
                    string base64Image = arr[1].Split(',')[1];
                    result.UserAvatar = base64Image;
                }
                result.code = 201;
            }
            else if (loginResult.Contains("window.code=200"))  //已扫描 已登录
            {
                string loginRedirectUrl = loginResult.Split(new string[] { "\"" }, StringSplitOptions.None)[1];
                result.code = 200;
                result.redirect_uri = loginRedirectUrl;

                if (loginRedirectUrl.IndexOf("wx2") > -1)
                {
                    Server = "wx2";
                }
            }

            return result;
        }

        public LoginRedirectResult LoginRedirect(string redirect_uri)
        {
            string url = redirect_uri + "&fun=new&version=v2&lang=zh_CN";
            byte[] bytes = http.GET(url);
            string rep = Encoding.UTF8.GetString(bytes);
            LoginRedirectResult result = new LoginRedirectResult();
            result.pass_ticket = rep.Split(new string[] { "pass_ticket" }, StringSplitOptions.None)[1].TrimStart('>').TrimEnd('<', '/');
            result.skey = rep.Split(new string[] { "skey" }, StringSplitOptions.None)[1].TrimStart('>').TrimEnd('<', '/');
            result.wxsid = rep.Split(new string[] { "wxsid" }, StringSplitOptions.None)[1].TrimStart('>').TrimEnd('<', '/');
            result.wxuin = rep.Split(new string[] { "wxuin" }, StringSplitOptions.None)[1].TrimStart('>').TrimEnd('<', '/');
            result.isgrayscale = rep.Split(new string[] { "isgrayscale" }, StringSplitOptions.None)[1].TrimStart('>').TrimEnd('<', '/');
            return result;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="passTicket"></param>
        /// <param name="baseReq"></param>
        /// <returns>InitResponse</returns>
        public InitResponse Init(string passTicket, BaseRequest baseReq)
        {
            string url = "https://" + Server + ".qq.com/cgi-bin/mmwebwx-bin/webwxinit?r={0}&pass_ticket={1}";
            url = string.Format(url, GetTimestamp(DateTime.Now), passTicket);
            InitRequest initReq = new InitRequest();
            initReq.BaseRequest = baseReq;
            string requestJson = JsonConvert.SerializeObject(initReq);
            string repJsonStr = http.POST_UTF8String(url, requestJson);
            var rep = JsonConvert.DeserializeObject<InitResponse>(repJsonStr);
            return rep;
        }

        /// <summary>
        /// 获得联系人列表
        /// </summary>
        /// <param name="passTicket"></param>
        /// <param name="skey"></param>
        /// <returns></returns>
        public GetContactResponse GetContact(string passTicket, string skey)
        {
            string url = "https://" + Server + ".qq.com/cgi-bin/mmwebwx-bin/webwxgetcontact?pass_ticket={0}&r={1}&seq=0&skey={2}";
            url = string.Format(url, passTicket, GetTimestamp(DateTime.Now), skey);
            string json = http.GET_UTF8String(url);
            var rep = JsonConvert.DeserializeObject<GetContactResponse>(json);
            return rep;
        }

        /// <summary>
        /// 批量获取联系人详细信息
        /// </summary>
        /// <param name="requestContacts"></param>
        /// <param name="passTicket"></param>
        /// <param name="baseReq"></param>
        /// <returns></returns>
        public BatchGetContactResponse BatchGetContact(string[] requestContacts, string passTicket, BaseRequest baseReq)
        {
            string url = "https://" + Server + ".qq.com/cgi-bin/mmwebwx-bin/webwxbatchgetcontact?type=ex&r={0}&lang=zh_CN&pass_ticket={1}";
            url = string.Format(url, GetTimestamp(DateTime.Now), passTicket);

            BatchGetContactRequest req = new BatchGetContactRequest();
            req.BaseRequest = baseReq;
            req.Count = requestContacts.Length;

            List<BatchUser> requestUsers = new List<BatchUser>();
            for (int i = 0; i < req.Count; i++)
            {
                var tmp = new BatchUser();
                tmp.UserName = requestContacts[i];
                requestUsers.Add(tmp);
            }

            req.List = requestUsers.ToArray();
            string requestJson = JsonConvert.SerializeObject(req);
            string repJsonStr = http.POST_UTF8String(url, requestJson);
            var rep = JsonConvert.DeserializeObject<BatchGetContactResponse>(repJsonStr);
            return rep;
        }


        public SyncCheckResponse SyncCheck(SyncItem[] syncItems, BaseRequest baseReq)
        {
            string synckey = "";
            for (int i = 0; i < syncItems.Length; i++)
            {
                if (i != 0)
                {
                    synckey += "|";
                }
                synckey += syncItems[i].Key + "_" + syncItems[i].Val;
            }
            string url = "https://webpush." + Server + ".qq.com/cgi-bin/mmwebwx-bin/synccheck?skey={0}&sid={1}&uin={2}&deviceid={3}&synckey={4}&_={5}&r={6}";
            url = string.Format(url, baseReq.Skey.Replace("@", "%40"), baseReq.Sid, baseReq.Uin, baseReq.DeviceID, synckey, GetTimestamp(DateTime.Now) - 10, GetTimestamp(DateTime.Now));
            string repStr = http.GET_UTF8String(url);
            SyncCheckResponse rep = new SyncCheckResponse();
            if (repStr.StartsWith("window.synccheck="))
            {
                repStr = repStr.Substring("window.synccheck=".Length);
                rep = JsonConvert.DeserializeObject<SyncCheckResponse>(repStr);
            }

            return rep;
        }

        private static long GetTimestamp(DateTime time)
        {
            return (long)(time.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public SyncResponse Sync(SyncKey syncKey, string pass_ticket, BaseRequest baseReq)
        {
            string url = "https://" + Server + ".qq.com/cgi-bin/mmwebwx-bin/webwxsync?sid={0}&skey={1}&lang=zh_CN&pass_ticket={2}";
            url = string.Format(url, baseReq.Sid, baseReq.Skey, pass_ticket);
            SyncRequest req = new SyncRequest();
            req.BaseRequest = baseReq;
            req.SyncKey = syncKey;
            req.rr = GetTimestamp(DateTime.Now);
            string requestJson = JsonConvert.SerializeObject(req);
            string repJsonStr = http.POST_UTF8String(url, requestJson);
            var rep = JsonConvert.DeserializeObject<SyncResponse>(repJsonStr);
            return rep;
        }

        public StatusnotifyResponse Statusnotify(string formUser, string toUser, string pass_ticket, BaseRequest baseReq)
        {
            string url = "https://" + Server + ".qq.com/cgi-bin/mmwebwx-bin/webwxstatusnotify?lang=zh_CN&pass_ticket=" + pass_ticket;
            StatusnotifyRequest req = new StatusnotifyRequest();
            req.BaseRequest = baseReq;
            req.ClientMsgId = GetTimestamp(DateTime.Now);
            req.FromUserName = formUser;
            req.ToUserName = toUser;
            req.Code = 3;
            string requestJson = JsonConvert.SerializeObject(req);
            string repJsonStr = http.POST_UTF8String(url, requestJson);
            var rep = JsonConvert.DeserializeObject<StatusnotifyResponse>(repJsonStr);
            return rep;
        }

        public SendMsgResponse SendMsg(Msg msg, string passTicket, BaseRequest baseReq)
        {
            string url = "https://" + Server + ".qq.com/cgi-bin/mmwebwx-bin/webwxsendmsg?sid={0}&r={1}&lang=zh_CN&pass_ticket={2}";
            url = string.Format(url, baseReq.Sid, GetTimestamp(DateTime.Now), passTicket);
            SendMsgRequest req = new SendMsgRequest();
            req.BaseRequest = baseReq;
            req.Msg = msg;
            req.rr = DateTime.Now.Millisecond;
            string requestJson = JsonConvert.SerializeObject(req);
            string repJsonStr = http.POST_UTF8String(url, requestJson);
            var rep = JsonConvert.DeserializeObject<SendMsgResponse>(repJsonStr);
            return rep;
        }

        public UploadmediaResponse Uploadmedia(string fromUserName, string toUserName, string id, string mime_type, int uploadType, int mediaType, byte[] buffer, string fileName, string pass_ticket, BaseRequest baseReq)
        {
            UploadmediaRequest req = new UploadmediaRequest();
            req.BaseRequest = baseReq;
            req.ClientMediaId = GetTimestamp(DateTime.Now);
            req.DataLen = buffer.Length;
            req.StartPos = 0;
            req.TotalLen = buffer.Length;
            req.MediaType = mediaType;
            req.FromUserName = fromUserName;
            req.ToUserName = toUserName;
            req.UploadType = uploadType;
            req.FileMd5 = UniversalTool.getMD5(buffer);

            string url = "https://file." + Server + ".qq.com/cgi-bin/mmwebwx-bin/webwxuploadmedia?f=json";
            string requestJson = JsonConvert.SerializeObject(req);
            NameValueCollection data = new NameValueCollection();
            data.Add("id", id);
            data.Add("name", fileName);
            data.Add("type", mime_type);
            data.Add("lastModifiedDate", "Thu Mar 17 2016 14:35:28 GMT+0800 (中国标准时间)");
            data.Add("size", buffer.Length.ToString());
            string mt = "doc";
            if (mime_type.StartsWith("image/"))
            {
                mt = "pic";
            }
            data.Add("mediatype", mt);
            data.Add("uploadmediarequest", requestJson);
            var dataTicketCookie = http.GetCookie("webwx_data_ticket");
            data.Add("webwx_data_ticket", dataTicketCookie.Value);
            data.Add("pass_ticket", pass_ticket);
            string repJsonStr = http.UploadFile_UTF8String(url, buffer, fileName, mime_type, data, Encoding.UTF8);
            var rep = JsonConvert.DeserializeObject<UploadmediaResponse>(repJsonStr);
            return rep;
        }

        public SendMsgImgResponse SendMsgImg(ImgMsg msg, string pass_ticket, BaseRequest baseReq)
        {
            string url = "https://" + Server + ".qq.com/cgi-bin/mmwebwx-bin/webwxsendmsgimg?fun=async&f=json&pass_ticket={0}";
            url = string.Format(url, pass_ticket);
            SendMsgImgRequest req = new SendMsgImgRequest();
            req.BaseRequest = baseReq;
            req.Msg = msg;
            req.Scene = 0;
            string requestJson = JsonConvert.SerializeObject(req);
            string repJsonStr = http.POST_UTF8String(url, requestJson);
            var rep = JsonConvert.DeserializeObject<SendMsgImgResponse>(repJsonStr);
            return rep;
        }

        public OplogResponse Oplog(string userName, int cmdID, int op, string pass_ticket, BaseRequest baseReq)
        {
            string url = "https://" + Server + ".qq.com/cgi-bin/mmwebwx-bin/webwxoplog?pass_ticket={0}";
            url = string.Format(url, pass_ticket);
            OplogRequest req = new OplogRequest();
            req.BaseRequest = baseReq;
            req.UserName = userName;
            req.CmdId = cmdID;
            req.OP = op;
            string requestJson = JsonConvert.SerializeObject(req);
            string repJsonStr = http.POST_UTF8String(url, requestJson);
            var rep = JsonConvert.DeserializeObject<OplogResponse>(repJsonStr);
            return rep;
        }

        public Bitmap GetImage(string headImgUrl)
        {
            Bitmap bmp = null;
            try
            {
                string url = WechatApiService.GetAvatorUrl + headImgUrl;
                var bytes = this.http.GET(url);
                var dataStream = new MemoryStream(bytes);
                var img = System.Drawing.Image.FromStream(dataStream);
                dataStream.Close();
                bmp = new System.Drawing.Bitmap(img);
            }
            catch (Exception)
            {
                bmp = new System.Drawing.Bitmap(10, 10);
            }
            return bmp;
        }

    }
}
