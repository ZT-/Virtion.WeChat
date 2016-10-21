using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Virtion.WeChat.Server.Wx;
using Virtion.WeChat.Util;
using Wechat.API;
using Wechat.API.RPC;

namespace Virtion.WeChat.Server
{
    public class WechatClient
    {
        private readonly WechatApiService api;

        public AvatarConverter AvatarConverter;
        private BaseRequest baseReq;

        //首次初始化已完成(收到MsgType=51的消息,且获取完Contact)
        private bool firstInited;

        //---------------------------------------------------------
        // 所有缓存的用户信息
        private readonly Dictionary<string, User> mCachedUsers = new Dictionary<string, User>();

        // 联系人列表
        private readonly List<User> mContactList = new List<User>();
        // 群聊列表
        private readonly List<User> mGroupList = new List<User>();

        // 最近联系人
        private readonly List<User> mRecentContacts = new List<User>();


        public Action<User> OnAddUser;
        public Action<List<User>> OnGetContact;
        public Action<List<User>> OnGetRecent;

        //callback
        public Action<string> OnGetQrCodeImage;
        public Action OnInitComplate;
        public Action OnLoginSucess;
        public Action<AddMsg> OnRecvMsg;
        public Action<User> OnUpdateUser;
        public Action<string> OnUserScanQrCode;
        private string passTicket;
        private int upLoadMediaCount;

        public WechatClient()
        {
            this.api = new WechatApiService(new HttpClient());
        }

        public bool IsLogin { get; private set; }

        // 当前用户
        public User CurrentUser { get; private set; }

        private void CacheUser(User user)
        {
            if (user.UserName.StartsWith("@@") && !this.mGroupList.Contains(user))
            {
                this.mGroupList.Add(user);
            }

            if (!this.mCachedUsers.ContainsKey(user.UserName))
            {
                this.mCachedUsers[user.UserName] = user;
                this.OnAddUser.Invoke(user);
            }
            else
            {
                this.mCachedUsers[user.UserName] = user;
                if (this.OnUpdateUser != null)
                    this.OnUpdateUser.Invoke(user);
            }
        }

        private void CacheContactUser(User user)
        {
            this.mCachedUsers[user.UserName] = user;
        }

        public User GetCachedUser(string userName)
        {
            if (this.mCachedUsers.ContainsKey(userName))
                return this.mCachedUsers[userName];
            return null;
        }

        private void GetContact()
        {
            Debug.Write("[*] 正在获取联系人列表 ....");
            var getContactResult = this.api.GetContact(this.passTicket, this.baseReq.Skey);

            if ((getContactResult != null) && (getContactResult.BaseResponse != null) &&
                (getContactResult.BaseResponse.ret == 0))
            {
                Debug.Write("成功\n");
                Debug.WriteLine("[*] 共有 " + getContactResult.MemberCount + " 个联系人.");
            }
            else
            {
                Debug.Write("失败. 错误码:" + getContactResult.BaseResponse.ret);
                return;
            }

            foreach (var user in getContactResult.MemberList)
            {
                this.mContactList.Add(user);
                this.CacheContactUser(user);
            }

            this.OnGetContact.Invoke(this.mContactList);
        }

        private void DoLogion()
        {
            do
            {
                Debug.Write("[*] 正在获取Session ....");
                var session = this.api.GetNewQrLoginSessionId();
                if (!string.IsNullOrWhiteSpace(session))
                    Debug.Write("成功\n");
                else
                    continue;
                Debug.Write("[*] 正在生成二维码 ....");
                var QRImg = this.api.GetQrCodeImage(session);
                if (QRImg != null)
                    Debug.Write("成功\n");
                else
                    continue;
                Debug.Write("[*] 正在等待扫码 ....");
                this.OnGetQrCodeImage.Invoke(QRImg);
                //login check
                while (true)
                {
                    var loginResult = this.api.Login(session);
                    if (loginResult.code == 200)
                    {
                        // 登录成功
                        var redirectResult = this.api.LoginRedirect(loginResult.redirect_uri);
                        this.baseReq = new BaseRequest();
                        this.baseReq.Skey = redirectResult.skey;
                        this.baseReq.Sid = redirectResult.wxsid;
                        this.baseReq.Uin = redirectResult.wxuin;
                        // 生成DeviceID
                        var ran = new Random();
                        var rand1 = ran.Next(10000, 99999);
                        var rand2 = ran.Next(10000, 99999);
                        var rand3 = ran.Next(10000, 99999);
                        this.baseReq.DeviceID = string.Format("e{0}{1}{2}", rand1, rand2, rand3);
                        this.passTicket = redirectResult.pass_ticket;
                        this.IsLogin = true;
                        Debug.Write("已确认\n");
                        break;
                    }
                    if (loginResult.code == 201)
                    {
                        // 已扫描,但是未确认登录
                        // convert base64 to image
                        if (string.IsNullOrEmpty(loginResult.UserAvatar) == false)
                        {
                            this.OnUserScanQrCode.Invoke(loginResult.UserAvatar);
                        }
                        else
                        {
                            this.OnUserScanQrCode.Invoke("");
                        }

                        Debug.Write("已扫码\n");
                        Debug.Write("[*] 正在等待确认 ....");
                    }
                    else
                    {
                        // 超时
                        Debug.Write("超时\n");
                        break;
                    }
                }
            } while (!this.IsLogin);

            this.OnLoginSucess.Invoke();
        }

        private bool OpenStatusNotify()
        {
            Debug.Write("[*] 正在开启系统通知 ....");
            var statusNotifyRep = this.api.Statusnotify(this.CurrentUser.UserName,
                this.CurrentUser.UserName,
                this.passTicket,
                this.baseReq);

            if ((statusNotifyRep != null) && (statusNotifyRep.BaseResponse != null) &&
                (statusNotifyRep.BaseResponse.ret == 0))
            {
                Debug.Write("成功\n");
                return true;
            }
            else
            {
                Debug.Write("失败.错误码:" + statusNotifyRep.BaseResponse.ret);
                FailedClear();
                return false;
            }
        }

        private void AddRecentContacts(InitResponse initResult)
        {
            this.mRecentContacts.Clear();
            foreach (var user in initResult.ContactList)
            {
                CacheUser(user);
                this.mRecentContacts.Add(user);
            }
            this.OnGetRecent(this.mRecentContacts);
        }

        private void GetUserDetail(List<string> waitingToCacheUserList)
        {
            if (waitingToCacheUserList.Count > 0)
            {
                // 获得群详细信息
                Debug.WriteLine("[*] 正在获取联系人详细信息 ....");

                foreach (var userName in waitingToCacheUserList)
                {
                    if (userName.StartsWith("@@"))
                    {
                        RefreshGroupMemberInfo(userName);
                    }
                }

                var batchResult = this.api.BatchGetContact(waitingToCacheUserList.ToArray(),
                    this.passTicket, this.baseReq);

                if ((batchResult != null) && (batchResult.ContactList != null))
                {
                    foreach (var user in batchResult.ContactList)
                        CacheUser(user);

                    Debug.WriteLine("[*] 获取到联系人详细信息 " + batchResult.Count + "个");
                }
                waitingToCacheUserList.Clear();
            }

        }


        /// <summary>
        ///     运行微信Client主逻辑,推荐放在独立的线程中执行这个方法
        /// </summary>
        public void Run()
        {
            // 启动流程
            // 1.登陆
            // 2.初始化
            // 3.开启系统通知
            // 4.获得联系人列表
            // 5.进入同步主循环

            // ----------1.登陆
            DoLogion();

        RetryInit:
            // ----------2.初始化
            Debug.Write("[*] 正在初始化 ....");
            var initResult = this.api.Init(this.passTicket, this.baseReq);
            if (initResult == null)
            {
                Debug.Write("网络错误....\n");
                Debug.Write("重试中...\n");
                Thread.Sleep(1000);
                goto RetryInit;
            }
            if (initResult.BaseResponse.ret == 0)
            {
                Debug.Write("成功\n");
            }
            else
            {
                Debug.Write("失败.错误码:" + initResult.BaseResponse.ret + "\n");
                return;
            }

            this.CurrentUser = initResult.User;
            this.AvatarConverter = new AvatarConverter();
            this.AvatarConverter.Start();

            //最近联系人
            this.AddRecentContacts(initResult);

            // chatsets 里有需要获取详细信息的联系人.
            var waitingToCacheUserList = new List<string>();
            var chatsets = initResult.ChatSet.Split(',');
            foreach (var username in chatsets)
            {
                if (!username.StartsWith("@"))
                {
                    continue;
                }

                if (!this.mCachedUsers.ContainsKey(username) && !waitingToCacheUserList.Contains(username))
                {
                    waitingToCacheUserList.Add(username);
                }
            }


            // ----------3.开启状态通知
            //if (this.OpenStatusNotify() == false)
            //{
            //    return;
            //}

            // ----------4.获得联系人列表
            GetContact();


            //-----------5.批量获取群组详细信息
            Debug.Write("[*] 正在请求群聊成员详细信息 ....\n");
            foreach (var user in this.mCachedUsers.Values)
            {
                if (user.UserName.StartsWith("@@"))
                {
                    waitingToCacheUserList.Add(user.UserName);
                    //RefreshGroupMemberInfo();
                }
            }


            var syncKey = initResult.SyncKey;
            // ----------6.同步主循环
            Debug.WriteLine("[*] 进入同步循环 ....");
            while (true)
            {
                var hasInitMsg = false;
                // 同步
                if (syncKey.Count > 0)
                {
                    var syncCheckResult = this.api.SyncCheck(syncKey.List, this.baseReq);
                    if (syncCheckResult == null)
                        continue;

                    if (syncCheckResult.retcode != "0")
                    {
                        Debug.WriteLine("[*] 登陆已失效,请重新登陆 ....");
                        this.IsLogin = false;
                        FailedClear();
                        Thread.Sleep(1000);

                        goto RetryInit;
                    }

                    if ((syncCheckResult.retcode == "0") && (syncCheckResult.selector != "0"))
                    {
                        Debug.WriteLine("[*] 同步检查 RetCode:{0} Selector:{1}", syncCheckResult.retcode,
                            syncCheckResult.selector);
                        var syncResult = this.api.Sync(syncKey, this.passTicket, this.baseReq);
                        syncKey = syncResult.SyncKey;
                        Debug.WriteLine("[*] 同步结果 AddMsgCount:{0} ModContactCount:{1}", syncResult.AddMsgCount,
                            syncResult.ModContactCount);

                        // addmsg
                        if (syncResult.AddMsgCount > 0)
                            foreach (var msg in syncResult.AddMsgList)
                            {
                                // 过滤系统信息
                                if (msg.MsgType != 51)
                                    this.OnRecvMsg.Invoke(msg);
                                else
                                    hasInitMsg = true;

                                var notifyUserNames = msg.StatusNotifyUserName.Split(',');
                                foreach (var username in notifyUserNames)
                                {
                                    if (!username.StartsWith("@"))
                                        continue;

                                    if (!this.mCachedUsers.ContainsKey(username) &&
                                        !waitingToCacheUserList.Contains(username))
                                    {
                                        waitingToCacheUserList.Add(username);
                                    }

                                }
                            }

                        // modify contact
                        if (syncResult.ModContactList != null)
                            foreach (var modContact in syncResult.ModContactList)
                            {
                                CacheUser(modContact);
                            }
                    }
                }

                this.GetUserDetail(waitingToCacheUserList);

                // 初始化完成回调
                if (hasInitMsg && !this.firstInited)
                {
                    Debug.WriteLine("[*] 初始化完成 ");
                    this.firstInited = true;
                    this.OnInitComplate.Invoke();
                }
            }
        }

        /// <summary>
        ///     刷新群聊成员信息(Sync的时候可以返回群聊成员的Uin)
        /// </summary>
        /// <param name="groupUserName"></param>
        public void RefreshGroupMemberInfo(string groupUserName)
        {
            var result = this.api.Oplog(groupUserName, 3, 0, this.passTicket, this.baseReq);

        }

        public bool SendMsg(string toUserName, string content)
        {
            var msg = new Msg();
            msg.FromUserName = this.CurrentUser.UserName;
            msg.ToUserName = toUserName;
            msg.Content = content;
            msg.ClientMsgId = DateTime.Now.Millisecond;
            msg.LocalID = DateTime.Now.Millisecond;
            msg.Type = 1; //type 1 文本消息
            var response = this.api.SendMsg(msg, this.passTicket, this.baseReq);
            if ((response != null) && (response.BaseResponse != null) && (response.BaseResponse.ret == 0))
                return true;
            return false;
        }

        public bool SendMsg(string toUserName, Image img, ImageFormat format = null, string imageName = null)
        {
            if (img == null) return false;
            var fileName = imageName != null ? imageName : "img_" + this.upLoadMediaCount;
            var imgFormat = format != null ? format : ImageFormat.Png;

            fileName += "." + imgFormat.ToString().ToLower();

            var ms = new MemoryStream();
            img.Save(ms, imgFormat);
            ms.Seek(0, SeekOrigin.Begin);
            var data = new byte[ms.Length];
            var readCount = ms.Read(data, 0, data.Length);
            if (readCount != data.Length) return false;

            var mimetype = "image/" + imgFormat.ToString().ToLower();
            var response = this.api.Uploadmedia(this.CurrentUser.UserName, toUserName,
                "WU_FILE_" + this.upLoadMediaCount, mimetype, 2, 4, data, fileName, this.passTicket, this.baseReq);
            if ((response != null) && (response.BaseResponse != null) && (response.BaseResponse.ret == 0))
            {
                this.upLoadMediaCount++;
                var mediaId = response.MediaId;
                var msg = new ImgMsg();
                msg.FromUserName = this.CurrentUser.UserName;
                msg.ToUserName = toUserName;
                msg.MediaId = mediaId;
                msg.ClientMsgId = DateTime.Now.Millisecond;
                msg.LocalID = DateTime.Now.Millisecond;
                msg.Type = 3;
                var sendImgRep = this.api.SendMsgImg(msg, this.passTicket, this.baseReq);
                if ((sendImgRep != null) && (sendImgRep.BaseResponse != null) && (sendImgRep.BaseResponse.ret == 0))
                    return true;
                return false;
            }
            return false;
        }

        public Bitmap GetImage(string url)
        {
            return this.api.GetImage(url);
        }

        public void FailedClear()
        {
            if (this.AvatarConverter != null)
            {
                this.AvatarConverter.Quit();
            }

            this.mCachedUsers.Clear();
            this.mRecentContacts.Clear();
            this.mContactList.Clear();
            this.mGroupList.Clear();
        }
    }
}