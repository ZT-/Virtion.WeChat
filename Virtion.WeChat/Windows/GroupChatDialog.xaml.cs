using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Virtion.WeChat.Server;
using Virtion.WeChat.Struct;
using Virtion.WeChat.Util;

namespace Virtion.WeChat.Windows
{
    class ChatRoom
    {
        public string UserName;
        public string EncryChatRoomId;
    }

    public partial class GroupChatDialog : ChatDialog
    {
        private string configPath
        {
            get
            {
                return App.CurrentPath + "Data\\" + CurrentUser.WxUin + "\\" + user.PseudoUID + ".json";
            }
        }
        private List<string> whiteList
        {
            get
            {
                return this.chatConfig == null ? null : this.chatConfig.WhiteList;
            }
        }
        private ChatSettingWindow settingWindow;
        private ChatConfig chatConfig;
        private Dictionary<string, string> nameTable;

        //private List<User> backUser;

        public GroupChatDialog(User user)
        {
            InitializeComponent();

            if (user == null)
                return;

            this.user = user;
            this.Title = user.DisplayName;

        }

        public new void Show()
        {
            this.TB_Receive.Text = "";
            base.Show();
        }

        private User FindUserByDisplayName(string name)
        {
            foreach (ListBoxItem item in this.LB_MemberList.Items)
            {
                var user = item.DataContext as User;
                if (user.DisplayName == name)
                {
                    return user;
                }
            }
            return null;
        }

        private bool IsWhiteList(User user)
        {
            foreach (var item in this.whiteList)
            {
                if (item == user.PseudoUID)
                {
                    return true;
                }
            }
            return false;
        }

        private void ShowMemberList(User[] list)
        {
            this.nameTable = new Dictionary<string, string>();
            this.L_Count.Content = "本群共有（" + list.Length + "）人";
            this.LB_WhiteList.Items.Clear();
            this.LB_MemberList.Items.Clear();
            foreach (var item in list)
            {
                item.SetDisplayName();
                this.nameTable[item.UserName] = item.DisplayName;

                this.FilterWhiteList(item);

                ListBoxItem listItem = new ListBoxItem()
                {
                    Content = item.DisplayName,
                    DataContext = item
                };
                listItem.ContextMenu = new System.Windows.Controls.ContextMenu();
                MenuItem deleteMenuItem = new MenuItem()
                {
                    Header = "从群成员中踢除"
                };
                listItem.ContextMenu.Items.Add(deleteMenuItem);
                deleteMenuItem.Click += DeleteMenuItem_Click;

                MenuItem addWhiteMenuItem = new MenuItem()
                {
                    Header = "添加到白名单"
                };
                listItem.ContextMenu.Items.Add(addWhiteMenuItem);
                addWhiteMenuItem.Click += AddWhiteMenuItem_Click;

                this.LB_MemberList.Items.Add(listItem);
            }
        }

        private void GetMemberList()
        {
            this.LM_Marsk.IsLoading = true;
            long time = Time.Now();
            string url = WxApi.GetDetailUrl
                + "type=ex&lang=zh_CN&r=" + time
                + "&pass_ticket=" + CurrentUser.PassTicket;

            JObject jsonObj = new JObject();
            jsonObj.Add("BaseRequest", JObject.FromObject(CurrentUser.BaseRequest));
            jsonObj.Add("Count", this.user.MemberCount);
            List<ChatRoom> list = new List<ChatRoom>();
            foreach (var item in user.MemberList)
            {
                var room = new ChatRoom()
                {
                    UserName = item.UserName,
                    EncryChatRoomId = user.UserName
                };
                list.Add(room);
            }

            jsonObj.Add("List", JArray.FromObject(list));
            HttpRequest.PostJson<GroupMenber>(url, jsonObj, (obj) =>
            {
                this.ShowMemberList(obj.ContactList);
                //var s = JsonConvert.SerializeObject(obj.ContactList);
                //Console.WriteLine(s);
                this.LM_Marsk.IsLoading = false;
            });

        }

        public void GetGroupDetail()
        {
            this.LB_MemberList.Items.Clear();
            long time = Time.Now();
            string url = WxApi.GetDetailUrl
                + "type=ex&lang=zh_CN&r=" + time
                + "&pass_ticket=" + CurrentUser.PassTicket;

            JObject jsonObj = new JObject();
            jsonObj.Add("BaseRequest", JObject.FromObject(CurrentUser.BaseRequest));
            jsonObj.Add("Count", 1);
            List<ChatRoom> list = new List<ChatRoom>();
            var room = new ChatRoom()
            {
                UserName = user.UserName,
                EncryChatRoomId = ""
            };
            list.Add(room);
            jsonObj.Add("List", JArray.FromObject(list));

            HttpRequest.PostJson<GroupMenber>(url, jsonObj, (obj) =>
            {
                this.user = obj.ContactList[0];
                var s = JsonConvert.SerializeObject(this.user);
                Console.WriteLine(s);

                this.GetMemberList();
            });

        }

        public void DeleteMenber(User member)
        {
            if (member.PseudoUID == CurrentUser.Me.PseudoUID)
            {
                //MessageBox.Show("不能删除自己");
                return;
            }

            string url = WxApi.DeleteMenberUrl + "&lang=zh_CN&pass_ticket=" + CurrentUser.PassTicket;

            JObject jsonObj = new JObject();
            jsonObj.Add("BaseRequest", JObject.FromObject(CurrentUser.BaseRequest));
            jsonObj.Add("ChatRoomName", user.UserName);

            List<string> list = new List<string>();
            list.Add(member.UserName);
            jsonObj.Add("DelMemberList", member.UserName);

            Console.WriteLine(url);
            Console.WriteLine(jsonObj);
            Object obj = HttpRequest.PostJsonSync<Object>(url, jsonObj);

            this.TB_Receive.Text += "用户【" + member.DisplayName + "】 被踢出！\n";
            if (this.LB_MemberList.SelectedItem == null)
            {
                for (int i = 0; i < this.LB_MemberList.Items.Count; i++)
                {
                    ListBoxItem item = this.LB_MemberList.Items[i] as ListBoxItem;
                    if (item.Content as string == member.DisplayName)
                    {
                        this.LB_MemberList.Items.Remove(item);
                    }
                }
            }
            else
            {
                this.LB_MemberList.Items.Remove(this.LB_MemberList.SelectedItem);
            }
        }

        public void ReceiveImage(Msg msg)
        {
            string content = msg.Content;

            string formUser = "fromusername = \"";
            int pos = content.IndexOf(formUser, StringComparison.Ordinal);
            string userName = "";

            if(pos==-1)
            {
                formUser = "fromusername=\"";
                pos = content.IndexOf(formUser, StringComparison.Ordinal);
            }

            if (pos > -1)
            {

                int dotPos = content.IndexOf("\"", pos + formUser.Length, StringComparison.Ordinal);
                userName = content.Substring(pos + formUser.Length, dotPos - formUser.Length - pos);

                bool flag = true;
                if (string.IsNullOrEmpty(this.chatConfig.ImageUserName) == true || userName != this.chatConfig.ImageUserName)
                {
                    flag = false;
                }

                if (content.IndexOf(this.chatConfig.UserImage) == -1)
                {
                    flag = false;
                }

                if (this.chatConfig.IsFilterUserImage == true)
                {
                    if (flag == true)
                    {
                        this.SendRandomMessage();
                    }
                }

            }

            this.TB_Receive.Text += userName + "(图片消息)：\n";
            this.TB_Receive.Text += content + "\n";

            //this.DealAllFilter(msg);
        }

        public override void ReceiveMessage(Msg msg)
        {
            if (msg.MsgType == 47)
            {
                this.ReceiveImage(msg);
                return;
            }

            int pos = msg.Content.IndexOf(":<br/>");
            if (pos > -1)
            {
                msg.FromUserName = msg.Content.Substring(0, pos);
                msg.Content = msg.Content.Substring(pos + 6);
            }

            this.DealAllFilter(msg);

            if (pos > -1)
            {
                var userID = msg.FromUserName;
                if (this.nameTable != null && this.nameTable.ContainsKey(userID) == true)
                {
                    TB_Receive.Text += this.nameTable[userID] + ":\n";
                }
                else
                {
                    TB_Receive.Text += msg.FromUserName + ":\n";
                }
                TB_Receive.Text += msg.Content + "\n";
            }
            else
            {
                TB_Receive.Text += "我：\n" + msg.Content + "\n";
            }

        }



        public override bool SendMessage(string word)
        {
            long time = Time.Now();
            string url = WxApi.SendMessageUrl +
                "?sid=" + CurrentUser.WxSid +
                "&skey=" + CurrentUser.Skey +
                "&pass_ticket=" + CurrentUser.PassTicket +
                "&r=" + time;

            JObject jsonObj = new JObject();
            jsonObj.Add("BaseRequest", JObject.FromObject(CurrentUser.BaseRequest));
            SendMsg msg = new SendMsg();
            msg.FromUserName = CurrentUser.Me.UserName;
            msg.ToUserName = user.UserName;
            msg.Type = 1;
            msg.Content = word;
            msg.ClientMsgId = time;
            msg.LocalID = time;
            TB_SendBox.Clear();
            jsonObj.Add("Msg", JObject.FromObject(msg));

            WxSendMsg wxsendmsg = HttpRequest.PostJsonSync<WxSendMsg>(url, jsonObj);

            TB_Receive.Text += "我：\n" + msg.Content + "\n";

            return true;
        }



        #region Filter
        private void DealAllFilter(Msg msg)
        {
            if (this.CB_Monitor.IsChecked.Value == true && this.chatConfig != null)
            {
                if (this.chatConfig.IsFilterMsgCount == true)
                {
                    FilterMaxCountMessage(msg);
                }
                if (this.chatConfig.IsFilterUserMsg == true)
                {
                    this.FilterUserDefineMessage(msg);
                }
            }
        }

        public void FilterUserDefineMessage(Msg msg)
        {
            if (msg.Content.IndexOf(this.chatConfig.UserMsg) > -1)
            {
                this.SendRandomMessage();
            }
        }

        private void SendRandomMessage()
        {
            var random = new Random();
            var index = random.Next(this.chatConfig.DefineList.Count);

            var thread = new Thread(() =>
            {
                if (this.chatConfig.Delay > 0)
                {
                    Thread.Sleep(this.chatConfig.Delay);
                }
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.SendMessage(this.chatConfig.DefineList[index]);
                }));
            });
            thread.Start();

        }

        public void FilterMaxCountMessage(Msg msg)
        {
            if (msg.Content.Length > this.chatConfig.MaxMsgLength)
            {
                foreach (ListBoxItem listItem in this.LB_MemberList.Items)
                {
                    var user = listItem.DataContext as User;
                    if (user.UserName == msg.FromUserName)
                    {
                        int pos = this.whiteList.IndexOf(user.PseudoUID);
                        if (pos == -1)
                        {
                            this.DeleteMenber(user);
                        }
                        break;
                    }
                }
            }
        }

        public void FilterInvite(Msg msg)
        {
            var content = msg.Content;
            //if (content.StartsWith("You've ") == true)
            //{
            //    return;
            //}
            int endPos = content.IndexOf(" to the group chat");
            if (endPos == -1)
            {
                return;
            }
            string midWord = " invited ";
            int midPos = content.IndexOf(midWord);
            if (endPos == -1)
            {
                return;
            }
            var firstUserName = content.Substring(0, midPos);
            var secondUserName = content.Substring(midPos + midWord.Length,
                endPos - (midPos + midWord.Length));

            var firstUser = this.FindUserByDisplayName(firstUserName);
            var secondUser = this.FindUserByDisplayName(secondUserName);

            if (this.IsWhiteList(user) == true)
            {
                return;
            }

            if (this.chatConfig.IsFilterAdd == true || secondUser != null)
            {
                this.DeleteMenber(firstUser);
            }

            if (this.chatConfig.IsFilterSelfDef == true || firstUser != null)
            {
                this.DeleteMenber(firstUser);
            }
        }

        private void FilterWhiteList(User user)
        {
            foreach (var item in this.whiteList)
            {
                if (item == user.PseudoUID)
                {
                    ListBoxItem listItem = new ListBoxItem()
                    {
                        Content = user.DisplayName,
                        DataContext = user
                    };
                    this.LB_WhiteList.Items.Add(listItem);
                    break;
                }
            }
        }

        #endregion

        #region Config
        private void LoadSetting()
        {
            if (File.Exists(this.configPath) == true)
            {
                string s = File.ReadAllText(this.configPath);
                this.chatConfig = JsonConvert.DeserializeObject<ChatConfig>(s);
                this.CB_Monitor.IsChecked = this.chatConfig.IsMonitor;
                this.CB_MonitorOnMini.IsChecked = this.chatConfig.IsMonitorOnMini;
            }
            else
            {
                this.chatConfig = new ChatConfig();

            }
        }

        private void SaveConfig()
        {
            try
            {
                var s = JsonConvert.SerializeObject(this.chatConfig);
                File.WriteAllText(this.configPath, s);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadSetting();
            //this.ShowMemberList();
            this.GetMemberList();

            if (CurrentUser.MessageTable.ContainsKey(user.UserName))
            {
                var list = CurrentUser.MessageTable[user.UserName];
                for (int i = 0; i < list.Count; i++)
                {
                    Msg msg = list[i];
                    ReceiveMessage(msg);
                }
                list.Clear();
            }
        }

        ///白名单漏洞，昵称一样的
        private void AddWhiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var obj = this.LB_MemberList.SelectedItem as ListBoxItem;
            var item = obj.DataContext as User;
            this.whiteList.Add(item.PseudoUID);

            ListBoxItem listItem = new ListBoxItem()
            {
                Content = item.DisplayName,
                DataContext = item
            };

            listItem.ContextMenu = new System.Windows.Controls.ContextMenu();
            MenuItem deleteMenuItem = new MenuItem()
            {
                Header = "删除"
            };
            listItem.ContextMenu.Items.Add(deleteMenuItem);
            deleteMenuItem.Click += DeleteWhiteMenuItem_Click;

            this.LB_WhiteList.Items.Add(listItem);
            this.SaveConfig();
        }

        private void DeleteWhiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var listItem = this.LB_WhiteList.SelectedItem as ListBoxItem;
            var item = listItem.DataContext as User;
            if (item != null)
            {
                this.whiteList.Remove(item.PseudoUID);
                this.LB_WhiteList.Items.Remove(listItem);
                this.SaveConfig();
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var listItem = this.LB_MemberList.SelectedItem as ListBoxItem;
            var data = listItem.DataContext as User;

            this.DeleteMenber(data);
        }

        private void TB_SendBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                // 添加一个换行字符  
                TB_SendBox.SelectedText = Environment.NewLine;
                // 光标向前移动一位  
                TB_SendBox.Select(TB_SendBox.SelectionStart + 1, 0);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                L_Send_MouseDown(null, null);
                e.Handled = true;
            }
        }

        private void I_Setting_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.settingWindow = new ChatSettingWindow();
            this.settingWindow.SetConfig(this.chatConfig);
            this.settingWindow.Show();
            this.settingWindow.B_OK.Click += (s, events) =>
            {
                this.chatConfig = this.settingWindow.GetConfig();
                this.SaveConfig();
            };
        }

        private void G_Setting_MouseEnter(object sender, MouseEventArgs e)
        {
            this.G_Setting.Background = Theme.HightLightBackgroundBrush;
        }

        private void G_Setting_MouseLeave(object sender, MouseEventArgs e)
        {
            this.G_Setting.Background = Theme.NormalBackgroundBrush;
        }

        private void L_SendBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            this.L_SendBtn.Background = Theme.HightLightBackgroundBrush;
        }

        private void L_SendBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            this.L_SendBtn.Background = Theme.NormalBackgroundBrush;
        }

        public void L_Send_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.SendMessage(TB_SendBox.Text.Replace("\r", ""));
        }

        private void I_Reflash_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.GetGroupDetail();
            //this.GetMemberList();
        }

        private void CB_Monitor_Checked(object sender, RoutedEventArgs e)
        {
            this.chatConfig.IsMonitor = true;
            this.SaveConfig();
        }

        private void CB_Monitor_Unchecked(object sender, RoutedEventArgs e)
        {
            this.chatConfig.IsMonitor = false;
            this.SaveConfig();
        }

        private void CB_MonitorOnMini_OnChecked(object sender, RoutedEventArgs e)
        {
            this.chatConfig.IsMonitorOnMini = true;
            this.SaveConfig();
            App.MainWindow.NotifyTray.AddMonitorName(user);
        }

        private void CB_MonitorOnMini_OnUnchecked(object sender, RoutedEventArgs e)
        {
            this.chatConfig.IsMonitorOnMini = false;
            this.SaveConfig();
            App.MainWindow.NotifyTray.RemoveMonitorName(user);
        }

        private void GroupChatDialog_OnClosing(object sender, CancelEventArgs e)
        {
            if (this.chatConfig.IsMonitorOnMini == true)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                CurrentUser.DialogTable.Remove(user.UserName);
            }
        }

        #endregion

    }
}
