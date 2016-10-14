using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Net;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using MahApps.Metro.Controls;
using Virtion.WeChat.Struct;
using System.Windows.Media;
using System.Windows.Controls;
using Virtion.WeChat.Controls.Item;
using Virtion.WeChat.Server;
using Virtion.WeChat.Util;
using Virtion.WeChat.Windows;

namespace Virtion.WeChat
{
    public partial class MainWindow : MetroWindow
    {
        public static SolidColorBrush HightLightBackgroundBrush
            = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C3C3C"));

        public static SolidColorBrush NormalBackgroundBrush
            = new SolidColorBrush(Colors.Transparent);

        private enum ListType
        {
            Session,
            Contact
        }

        private BackgroundWorker backgroundWorker;
        public string RedirectUrl;
        public AvatarConverter AvatarConverter;
        public Config Config;
        bool isRecieveInitMessage = false;

        private string configPath
        {
            get { return App.CurrentPath + "//Data//Config.json"; }
        }

        public MainWindow()
        {
            //ConsoleHelper.Show();
            ShowNotifyIcon();

            (new LoginWindow()).ShowDialog();

            if (string.IsNullOrEmpty(this.RedirectUrl) == true)
            {
                this.Close();
                App.Current.Shutdown();
                return;
            }

            InitializeComponent();
        }

        private void ShowNotifyIcon()
        {
            NotifyTray notifyTray = new NotifyTray();
            notifyTray.InitialTray();
        }

        private void BackgroundWork_DoWork(object sender, DoWorkEventArgs e)
        {
            this.GetLoginInfo();

            this.GetInitInfo();

            this.SyncList();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.B_Chat_Click(null, null);
                this.LM_Marsk.IsLoading = false;
            }));

            this.GetContact();

            this.GetStatusNotify();

            while (backgroundWorker.CancellationPending == false)
            {
                this.SyncCheck();
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void GetLoginInfo()
        {
            string url = this.RedirectUrl + "&fun=new";
            Console.WriteLine(url);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(dataStream);

            XmlNode node = xmlDoc["error"];
            if (!node["ret"].InnerText.Equals("0"))
            {
                MessageBox.Show("登录失败, Ret:" + node["ret"].InnerText, "错误",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                App.Current.Shutdown();
            }

            CurrentUser.Skey = node["skey"].InnerText;
            CurrentUser.WxSid = node["wxsid"].InnerText;
            CurrentUser.WxUin = node["wxuin"].InnerText;
            CurrentUser.PassTicket = node["pass_ticket"].InnerText;
            CurrentUser.BaseRequest = new BaseRequest(CurrentUser.WxUin,
                CurrentUser.WxSid, CurrentUser.Skey, CurrentUser.DeviceId);

            string[] temp = response.Headers[HttpResponseHeader.SetCookie].Split(new char[] { ',', ';' });
            foreach (string c in temp)
            {
                if (c.Contains("webwx_data_ticket"))
                {
                    CurrentUser.WebWxDataTicket = c.Split('=')[1];
                    break;
                }
            }

            CurrentUser.Cookie =
                "webwx_data_ticket=" + CurrentUser.WebWxDataTicket +
                "; wxsid=" + CurrentUser.WxSid +
                "; wxuin=" + CurrentUser.WxUin;

            dataStream.Close();
            response.Close();
        }

        private void GetInitInfo()
        {
            Console.WriteLine("GetInitInfo");

            string url = WxApi.GetInitUrl +
                         "?pass_ticket=" + CurrentUser.PassTicket +
                         "&skey=" + CurrentUser.Skey +
                         "&r=" + Time.Now();

            JObject jsonObj = new JObject();
            jsonObj.Add("BaseRequest", JObject.FromObject(CurrentUser.BaseRequest));

            WXInitList init = HttpRequest.PostJsonSync<WXInitList>(url, jsonObj);

            CurrentUser.Me = init.User;
            CurrentUser.Me.SetDisplayName();
            //Console.WriteLine(JsonConvert.SerializeObject(init.User));

            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.AvatarConverter = new AvatarConverter();
                this.AvatarConverter.Start();

                this.L_Name.Content = CurrentUser.Me.NickName;
                this.AvatarConverter.SetRequest(CurrentUser.Me, this.I_Avator);
            }));

            foreach (User user in init.ContactList)
            {
                this.AddSessionList(user, false);
            }

            if (init == null || init.BaseResponse.Ret != 0)
            {
                this.GetStatusNotify();
                MessageBox.Show("初始化失败", "错误");
                App.Current.Shutdown();
                //return;
            }
            CurrentUser.Synckey = init.SyncKey;
        }

        private void SyncList()
        {
            long time = Time.Now();
            string url = WxApi.SyncMsgUrl +
                         "pass_ticket=" + CurrentUser.PassTicket +
                         "&sid=" + CurrentUser.WxSid +
                         "&skey=" + CurrentUser.Skey +
                         "&r=" + time;

            JObject jsonObj = new JObject();
            jsonObj.Add("BaseRequest", JObject.FromObject(CurrentUser.BaseRequest));
            jsonObj.Add("SyncKey", JObject.FromObject(CurrentUser.Synckey));
            jsonObj.Add("rr", time);

            WxSync sync = HttpRequest.PostJsonSync<WxSync>(url, jsonObj);

            //Console.WriteLine("BaseResponse.Ret:" + sync.BaseResponse.Ret);
            //Console.WriteLine("AddMsgCount:" + sync.AddMsgCount);
            //Console.WriteLine(sync.AddMsgList.ToString());
            //Console.WriteLine("ModContactCount:" + sync.ModContactCount);
            //Console.WriteLine("DelContactCount:" + sync.DelContactCount);
            //Console.WriteLine("ModChatRoomMemberCount:" + sync.ModChatRoomMemberCount);
            if (sync == null || sync.BaseResponse.Ret != 0)
            {
                //MessageBox.Show("读取消息失败", "错误");
                return;
                //Close();
            }

            CurrentUser.Synckey = sync.SyncKey;
            foreach (Msg msg in sync.AddMsgList)
            {
                DealMessage(msg);
            }
        }

        public User[] GetGroupDetail(List<string> userList)
        {
            long time = Time.Now();
            string url = WxApi.GetDetailUrl
                         + "type=ex&lang=zh_CN&r=" + time
                         + "&pass_ticket=" + CurrentUser.PassTicket;

            JObject jsonObj = new JObject();
            jsonObj.Add("BaseRequest", JObject.FromObject(CurrentUser.BaseRequest));
            jsonObj.Add("Count", userList.Count);
            List<ChatRoom> list = new List<ChatRoom>();
            foreach (var item in userList)
            {
                var room = new ChatRoom()
                {
                    UserName = item,
                    EncryChatRoomId = ""
                };
                list.Add(room);
            }
            jsonObj.Add("List", JArray.FromObject(list));

            var obj = HttpRequest.PostJsonSync<GroupMenber>(url, jsonObj);
            return obj.ContactList;
        }

        public void DealMessage(Msg msg)
        {
            Console.WriteLine("记录消息");
            Console.WriteLine("消息类型:" + msg.MsgType);
            var s = JsonConvert.SerializeObject(msg);
            Console.WriteLine("消息内容:" + s);

            switch (msg.MsgType)
            {
                case 1:
                    ReceiveUserMessgae(msg);
                    break;
                case 51:
                    ReceiveInitSession(msg);
                    break;
                case 10000:
                    ReceiveSystemMsg(msg);
                    break;
                default:
                    break;
            }

            //刷新ui
            //if (current_isChat)
            //Dispatcher.BeginInvoke(new Action<ListType>(ReflashList), ListType.Session);

            //播放声音
            //if (!msg.FromUserName.Equals(CurrentUser.Me.UserName))
            //    CurrentUser.player.Play();
        }

        private void ReceiveSystemMsg(Msg msg)
        {
            //var s = JsonConvert.SerializeObject(msg);
            //Console.WriteLine(s);
            if (msg.Status == 3 || msg.Status == 4)
            {
                if (CurrentUser.DialogTable.ContainsKey(msg.FromUserName) == true)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var dailog = CurrentUser.DialogTable[msg.FromUserName];
                        dailog.GetGroupDetail();
                        dailog.FilterInvite(msg);
                    }));
                }
            }
        }

        private void ReceiveInitSession(Msg msg)
        {
            if (isRecieveInitMessage == true)
            {
                return;
            }
            isRecieveInitMessage = true;
            var list = msg.StatusNotifyUserName.Split(',');
            var groupList = new List<string>();
            foreach (var item in list)
            {
                if (CurrentUser.ContactTable.ContainsKey(item) == true)
                {
                    this.AddSessionList(CurrentUser.ContactTable[item], false);
                }
                else
                {
                    if (item.StartsWith("@@") == true)
                    {
                        groupList.Add(item);
                    }
                }
            }

            if (groupList.Count > 0)
            {
                var userList = this.GetGroupDetail(groupList);

                foreach (var item in userList)
                {
                    if (string.IsNullOrEmpty(item.NickName))
                    {
                        for (int i = 0; i < item.MemberList.Length; i++)
                        {
                            item.MemberList[i].SetDisplayName();
                            item.NickName += " " + item.MemberList[i].DisplayName;
                            if (i > 3)
                            {
                                break;
                            }
                        }
                    }
                    this.AddSessionList(item, false);
                }
            }
        }

        private void ReceiveUserMessgae(Msg msg)
        {
            string friend = msg.FromUserName.Equals(CurrentUser.Me.UserName)
                ? msg.ToUserName
                : msg.FromUserName;

            //记录消息
            if (!CurrentUser.MessageTable.ContainsKey(friend))
                CurrentUser.MessageTable.Add(friend, new List<Msg>());

            CurrentUser.MessageTable[friend].Add(msg);

            bool hasWindow = false;
            //刷新消息
            if (CurrentUser.DialogTable.ContainsKey(friend) && CurrentUser.DialogTable[friend] != null)
            {
                hasWindow = true;
                //更新窗口
                Action<Msg> updateAction = new Action<Msg>(CurrentUser.DialogTable[friend].ReceiveMessage);
                CurrentUser.DialogTable[friend].Dispatcher.BeginInvoke(updateAction, msg);
                //任务栏闪烁
                //System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(Data.dialogs[friend]);
                //flashTaskBar(wndHelper.Handle, falshType.FLASHW_TIMERNOFG);
            }

            //
            if (CurrentUser.ChatTable.ContainsKey(friend) == true)
            {
                if (hasWindow == false)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (ContactListItem item in this.LB_SessionList.Items)
                        {
                            if (item.User.UserName == friend)
                            {
                                this.LB_SessionList.Items.Remove(item);
                                this.LB_SessionList.Items.Insert(0, item);
                                item.AddTipNumber();
                                break;
                            }
                        }
                    }));
                }
                //
            }
            else //add new 
            {
                User user = null;
                if (CurrentUser.ContactTable.ContainsKey(friend) == false)
                {
                    user = GetGroupDetail(new List<string>() { friend })[0];
                }
                else
                {
                    user = CurrentUser.ContactTable[friend];
                }

                this.AddSessionList(user, true, !hasWindow);
            }
        }

        private void AddSessionList(User user, bool order = false, bool isAddTip = false)
        {
            if (CurrentUser.ChatTable.ContainsKey(user.UserName) == true)
            {
                return;
            }
            CurrentUser.ChatTable[user.UserName] = user;
            user.SetDisplayName();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ContactListItem listItem = new ContactListItem()
                {
                    DisplayName = user.DisplayName,
                    User = user
                };
                if (order == true)
                {
                    this.LB_SessionList.Items.Insert(0, listItem);
                }
                else
                {
                    this.LB_SessionList.Items.Add(listItem);
                }

                if (isAddTip == true)
                {
                    listItem.AddTipNumber();
                }
            }));
        }

        private void GetContact()
        {
            string url = WxApi.GetContactUrl +
                         "pass_ticket=" + CurrentUser.PassTicket +
                         "&skey=" + CurrentUser.Skey +
                         "&r=" + Time.Now();

            HttpRequest.PostJson<WxContact>(url, "", GetContactCallBack, CurrentUser.Cookie);
        }

        private void GetContactCallBack(WxContact getcontact)
        {
            foreach (User user in getcontact.MemberList)
            {
                user.SetDisplayName();
                CurrentUser.ContactTable.Add(user.UserName, user);
            }

            if (getcontact.BaseResponse.Ret != 0)
            {
                MessageBox.Show("获取通讯录失败,webwxgetcontact.BaseResponse.Ret:"
                                + getcontact.BaseResponse.Ret, "错误",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GetStatusNotify()
        {
            long time = Time.Now();
            string url = WxApi.StatusNotifyUrl +
                         "?pass_ticket=" + CurrentUser.PassTicket +
                         "&sid=" + CurrentUser.WxSid +
                         "&skey=" + CurrentUser.Skey +
                         "&r=" + time;

            JObject jsonObj = new JObject();
            jsonObj.Add("BaseRequest", JObject.FromObject(CurrentUser.BaseRequest));
            jsonObj.Add("Code", 3);
            jsonObj.Add("FromUserName", CurrentUser.Me.UserName);
            jsonObj.Add("ToUserName", CurrentUser.Me.UserName);
            jsonObj.Add("ClientMsgId", time);

            WxStatusNotify statusnotify = HttpRequest.PostJsonSync<WxStatusNotify>(url, jsonObj);

            Console.WriteLine("状态提醒");
            //Console.WriteLine(url);
            Console.WriteLine(statusnotify.ToString());
        }

        private void SyncCheck()
        {
            string url = WxApi.SyncCheckUrl +
                         "?pass_ticket=" + CurrentUser.PassTicket +
                         "&skey=" + CurrentUser.Skey +
                         "&sid=" + CurrentUser.WxSid +
                         "&uin=" + CurrentUser.WxUin +
                         "&deviceid=" + CurrentUser.DeviceId +
                         "&synckey=" + CurrentUser.Synckey.get_urlstring() +
                         "&_=" + Time.Now();

            try
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string ret_str = reader.ReadToEnd().Split('=')[1];
                synccheck ret = JsonConvert.DeserializeObject<synccheck>(ret_str);

                reader.Close();
                dataStream.Close();
                response.Close();

                Console.WriteLine("同步消息");
                //Console.WriteLine(url);
                Console.WriteLine(ret_str);

                if (!ret.retcode.Equals("0"))
                {
                    MessageBox.Show("由于登录过于频繁系统拒绝登录稍等1分钟重新登录" + ret.retcode, "错误");
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.Close();
                        App.Current.Shutdown();
                    }));
                }

                if (!ret.selector.Equals("0"))
                {
                    SyncList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
        }


        #region Window Event
        private void OpenChatDialog(object sender, MouseButtonEventArgs e)
        {
            ContactListItem item = (sender as ListBox).SelectedItem as ContactListItem;
            item.RemoveTip();
            var user = item.User;

            if (CurrentUser.DialogTable.ContainsKey(user.UserName))
            {
                CurrentUser.DialogTable[user.UserName].Activate();
            }
            else
            {
                ChatDialog dialog = new ChatDialog(user);
                CurrentUser.DialogTable.Add(user.UserName, dialog);
                dialog.Show();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ShowNotifyIcon();

            this.LM_Marsk.IsLoading = true;

            if (File.Exists(this.configPath) == true)
            {
                string s = File.ReadAllText(this.configPath);
                try
                {
                    this.Config = JsonConvert.DeserializeObject<Config>(s);
                }
                catch (Exception)
                {
                }
            }

            if (this.Config == null)
            {
                this.Config = new Config()
                {
                    Left = 0,
                    Top = 0,
                    Height = 650,
                    Width = 350,
                    IsLoadAvatar = true
                };
            }

            this.Left = this.Config.Left;
            this.Top = this.Config.Top;
            this.Width = this.Config.Width;
            this.Height = this.Config.Height;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWork_DoWork;
            backgroundWorker.RunWorkerAsync();
        }

        private void G_TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void B_Chat_Click(object sender, MouseButtonEventArgs e)
        {
            this.LB_SessionList.Visibility = System.Windows.Visibility.Visible;
            this.LB_ContactList.Visibility = System.Windows.Visibility.Hidden;
        }

        private void B_Contact_Click(object sender, MouseButtonEventArgs e)
        {
            this.LB_SessionList.Visibility = System.Windows.Visibility.Hidden;
            this.LB_ContactList.Visibility = System.Windows.Visibility.Visible;

            if (this.LB_ContactList.Items.Count == 0)
            {
                foreach (var item in CurrentUser.ContactTable)
                {
                    var user = item.Value;
                    ContactListItem listItem = new ContactListItem()
                    {
                        DisplayName = user.DisplayName,
                        User = user
                    };
                    this.LB_ContactList.Items.Add(listItem);
                }
            }
        }

        private void I_Mini_Click(object sender, MouseButtonEventArgs e)
        {
            Hide();
        }

        private void I_Close_Click(object sender, MouseButtonEventArgs e)
        {
            if (CurrentUser.DialogTable.Count == 0 ||
                MessageBox.Show("您还有会话窗口未关闭,确认要退出微信?", "提示",
                MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
            }
        }

        private void B_Avator_MouseEnter(object sender, MouseEventArgs e)
        {
            this.B_Avator.BorderBrush = new SolidColorBrush(Colors.Blue);
        }

        private void B_Avator_MouseLeave(object sender, MouseEventArgs e)
        {
            this.B_Avator.BorderBrush = new SolidColorBrush(Colors.Black);
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(this.RedirectUrl) == true)
            {
                return;
            }
            e.Cancel = true;
            this.Hide();

            this.Config.Left = this.Left;
            this.Config.Top = this.Top;
            this.Config.Width = this.Width;
            this.Config.Height = this.Height;

            string s = JsonConvert.SerializeObject(this.Config);
            File.WriteAllText(this.configPath, s);
        }

        private void B_Chat_MouseEnter(object sender, MouseEventArgs e)
        {
            this.B_Chat.Background = HightLightBackgroundBrush;
        }

        private void B_Chat_MouseLeave(object sender, MouseEventArgs e)
        {
            this.B_Chat.Background = NormalBackgroundBrush;
        }

        private void B_Contact_MouseEnter(object sender, MouseEventArgs e)
        {
            this.B_Contact.Background = HightLightBackgroundBrush;
        }

        private void B_Contact_MouseLeave(object sender, MouseEventArgs e)
        {
            this.B_Contact.Background = NormalBackgroundBrush;
        }

        private void TB_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ListBox list = null;
            Dictionary<string, User> dataList = null;
            if (this.LB_SessionList.Visibility == System.Windows.Visibility.Visible)
            {
                list = this.LB_SessionList;
                dataList = CurrentUser.ChatTable;
            }
            else
            {
                list = this.LB_ContactList;
                dataList = CurrentUser.ContactTable;
            }

            list.Items.Clear();
            foreach (var item in dataList)
            {
                if (item.Value.DisplayName.IndexOf(this.TB_Search.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ContactListItem listItem = new ContactListItem()
                    {
                        DisplayName = item.Value.DisplayName,
                        User = item.Value
                    };
                    list.Items.Add(listItem);
                }
            }
        }
        #endregion

    }
}
