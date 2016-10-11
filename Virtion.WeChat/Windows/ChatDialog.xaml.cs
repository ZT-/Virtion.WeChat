using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Virtion.Util;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using Virtion.WeChat.Struct;
using MahApps.Metro.Controls;
using Virtion.WeChat.Windows;
using System.Windows.Media;

namespace Virtion.WeChat
{
    class ChatRoom
    {
        public string UserName;
        public string EncryChatRoomId;
    }

    public partial class ChatDialog : MetroWindow
    {
        public static SolidColorBrush HightLightBackgroundBrush
                 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C3C3C"));

        public static SolidColorBrush NormalBackgroundBrush
                         = new SolidColorBrush(Colors.Transparent);

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
        private User user;
        private Dictionary<string, string> nameTable;

        //private List<User> backUser;

        public ChatDialog(User user)
        {
            InitializeComponent();

            if (user == null)
                return;

            this.user = user;
            this.Title = user.DisplayName;

        }

        private void LoadSetting()
        {
            if (File.Exists(this.configPath) == true)
            {
                string s = File.ReadAllText(this.configPath);
                this.chatConfig = JsonConvert.DeserializeObject<ChatConfig>(s);
                this.CB_Monitor.IsChecked = this.chatConfig.IsMonitor;
            }
            else
            {
                this.chatConfig = new ChatConfig();

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

            if (this.chatConfig.IsFilterSelf == true || firstUser != null)
            {
                this.DeleteMenber(firstUser);
            }
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


        private void ShowMemberList(User[] list)
        {
            this.LB_WhiteList.Items.Clear();
            this.LB_MemberList.Items.Clear();
            foreach (var item in list)
            {
                item.SetDisplayName();

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
            long time = Time.Now();
            string url = WXApi.GetDetailUrl
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
                this.nameTable = new Dictionary<string, string>();
                foreach (var userItem in obj.ContactList)
                {
                    this.nameTable[userItem.UserName] = userItem.DisplayName;
                }

                this.ShowMemberList(obj.ContactList);
                var s = JsonConvert.SerializeObject(obj.ContactList);
                Console.WriteLine(s);
            });

        }

        public void GetGroupDetail()
        {
            this.LB_MemberList.Items.Clear();
            long time = Time.Now();
            string url = WXApi.GetDetailUrl
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
            if (member == CurrentUser.Me)
            {
                MessageBox.Show("不能删除自己");
                return;
            }

            string url = WXApi.DeleteMenberUrl + "&lang=zh_CN&pass_ticket=" + CurrentUser.PassTicket;

            JObject jsonObj = new JObject();
            jsonObj.Add("BaseRequest", JObject.FromObject(CurrentUser.BaseRequest));
            jsonObj.Add("ChatRoomName", user.UserName);

            List<string> list = new List<string>();
            list.Add(member.UserName);
            jsonObj.Add("DelMemberList", member.UserName);

            Console.WriteLine(url);
            Console.WriteLine(jsonObj);
            Object obj = HttpRequest.PostJsonSync<Object>(url, jsonObj);

            this.LB_MemberList.Items.Remove(this.LB_MemberList.SelectedItem);
        }

        public void ReceiveMessage(Msg msg)
        {
            if (msg.MsgType != 1)
                return;

            if (this.chatConfig.IsFilterMsg == true)
            {
                FilterMaxCountMessage(msg);
            }


            if (CurrentUser.ChatTable.ContainsKey(msg.FromUserName) == true)
            {
                TB_Receive.Text += CurrentUser.ChatTable[msg.FromUserName].DisplayName + ":\n";
            }
            else
            {
                if (CurrentUser.ContactTable.ContainsKey(msg.FromUserName) == true)
                {
                    TB_Receive.Text += CurrentUser.ContactTable[msg.FromUserName].DisplayName + ":\n";
                }
                else
                {
                    if (this.nameTable != null && this.nameTable.ContainsKey(msg.FromUserName) == true)
                    {
                        TB_Receive.Text += this.nameTable[msg.FromUserName] + ":\n";
                    }
                    else
                    {
                        TB_Receive.Text += msg.FromUserName + ":\n";
                    }
                }

            }

            TB_Receive.Text += msg.Content + "\n";
            TB_Receive.ScrollToEnd();

            if (this.chatConfig.IsFilterUserMsg == true)
            {
                this.FilterUserDefineMessage(msg);
            }
        }

        public void FilterUserDefineMessage(Msg msg)
        {
            if (this.chatConfig.UserMsg == msg.Content)
            {
                var random = new Random();
                var index = random.Next(this.chatConfig.DefineList.Count);

                var thread = new Thread(() =>
                {
                    if (this.chatConfig.Delay > 0)
                    {
                        Thread.Sleep(this.chatConfig.Delay);
                    }
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.SendMessage(this.chatConfig.DefineList[index]);
                    }));
                });
                thread.Start();
            }
        }

        public void SendMessage(string word)
        {
            long time = Time.Now();
            string url = WXApi.SendMessageUrl +
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

            Msg recvmsg = new Msg();
            recvmsg.MsgId = wxsendmsg.MsgID;
            recvmsg.FromUserName = msg.FromUserName;
            recvmsg.ToUserName = msg.ToUserName;
            recvmsg.MsgType = msg.Type;
            recvmsg.Content = msg.Content;
            recvmsg.CreateTime = msg.LocalID;

            Console.WriteLine("发送消息");
            Console.WriteLine(recvmsg.Content);

            App.MainWindow.DealMessage(recvmsg);

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

        public void Send(object sender, MouseButtonEventArgs e)
        {
            this.SendMessage(TB_SendBox.Text.Replace("\r", ""));
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (user.UserName.StartsWith("@@") == true)
            {
                this.LoadSetting();
                //this.ShowMemberList();
                this.GetMemberList();
            }
            else
            {
                this.G_Content.ColumnDefinitions[1].Width = new GridLength(0);
                this.G_Setting.Visibility = System.Windows.Visibility.Hidden;
                this.CB_Monitor.Visibility = System.Windows.Visibility.Hidden;
            }

            if (CurrentUser.MessageTable.ContainsKey(user.UserName))
            {
                var list = CurrentUser.MessageTable[user.UserName];
                for (int i = 0; i < list.Count; i++)
                {
                    Msg msg = list[i];
                    ReceiveMessage(msg);
                }
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

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CurrentUser.DialogTable.Remove(user.UserName);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
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
                Send(null, null);
                e.Handled = true;
            }
        }

        private void G_TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void I_Setting_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.settingWindow = new ChatSettingWindow();
            this.settingWindow.SetConfig(this.chatConfig);
            this.settingWindow.ShowDialog();
            this.chatConfig = this.settingWindow.GetConfig();
            this.SaveConfig();
        }

        private void G_Setting_MouseEnter(object sender, MouseEventArgs e)
        {
            this.G_Setting.Background = HightLightBackgroundBrush;
        }

        private void G_Setting_MouseLeave(object sender, MouseEventArgs e)
        {
            this.G_Setting.Background = NormalBackgroundBrush;
        }

        private void L_SendBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            this.L_SendBtn.Background = HightLightBackgroundBrush;
        }

        private void L_SendBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            this.L_SendBtn.Background
                = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCC"));
        }

        private void I_Reflash_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.GetGroupDetail();
            this.GetMemberList();
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

    }
}
