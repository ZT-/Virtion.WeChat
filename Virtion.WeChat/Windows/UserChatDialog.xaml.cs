using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using Virtion.WeChat.Server;
using Virtion.WeChat.Struct;
using Virtion.WeChat.Util;

namespace Virtion.WeChat.Windows
{
    public partial class UserChatDialog : ChatDialog
    {
        public UserChatDialog(User user)
        {
            InitializeComponent();
            this.user = user;
        }

        public override void ReceiveMessage(Msg msg)
        {
            if (msg.MsgType != 1)
                return;

            TB_Receive.Text += this.user.DisplayName + ":\n";
            TB_Receive.Text += msg.Content + "\n";
            TB_Receive.ScrollToEnd();
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

        private void UserChatDialog_OnClosing(object sender, CancelEventArgs e)
        {
            CurrentUser.DialogTable.Remove(user.UserName);
        }

        private void TB_SendBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
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
                L_Send_OnMouseDown(null, null);
                e.Handled = true;
            }
        }

        private void L_Send_OnMouseEnter(object sender, MouseEventArgs e)
        {
            this.L_Send.Background = Theme.HightLightBackgroundBrush;
        }

        private void L_Send_OnMouseLeave(object sender, MouseEventArgs e)
        {
            this.L_Send.Background = Theme.NormalBackgroundBrush;
        }

        private void L_Send_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.TB_SendBox.Text) == true)
            {
                MessageBox.Show("不能发送空消息");
                return;
            }
            this.SendMessage(this.TB_SendBox.Text);
        }

        private void UserChatDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Title = user.DisplayName;

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
    }



}
