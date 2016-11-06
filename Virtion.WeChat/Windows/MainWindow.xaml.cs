using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Virtion.WeChat.Controls.Item;
using Virtion.WeChat.Server;
using Virtion.WeChat.Struct;
using Virtion.WeChat.Util;

namespace Virtion.WeChat.Windows
{
    public partial class MainWindow : MetroWindow
    {
        public string RedirectUrl;
        public NotifyTray NotifyTray;

        public MainWindow()
        {
            //ConsoleHelper.Show();
            ShowNotifyIcon();

            InitializeComponent();

            this.Unloaded += (sender, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<Server.Wx.User>(this, "AddContactUser", AddContactUser);
            Messenger.Default.Register<Server.Wx.User>(this, "AddRecent", AddRecent);

        }

        private void AddContactUser(Server.Wx.User user)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ContactListItem listItem = new ContactListItem()
                {
                    DisplayName = user.DisplayName,
                    User = user
                };
                this.LB_ContactList.Items.Add(listItem);
            });
        }

        private void AddRecent(Server.Wx.User user)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ContactListItem listItem = new ContactListItem()
                {
                    DisplayName = user.DisplayName,
                    User = user
                };
                this.LB_SessionList.Items.Add(listItem);
            });
        }

        private void ShowNotifyIcon()
        {
            NotifyTray = new NotifyTray();
            NotifyTray.InitialTray();
        }



        #region Window Event
        private void OpenChatDialog(object sender, MouseButtonEventArgs e)
        {
            ContactListItem item = (sender as ListBox).SelectedItem as ContactListItem;
            item.RemoveTip();
            var user = item.User;

            if (CurrentUser.DialogTable.ContainsKey(user.UserName))
            {
                var dailog = CurrentUser.DialogTable[user.UserName];
                dailog.Show();
                dailog.Activate();
            }
            else
            {
                ChatDialog dialog = null;
                if (user.UserName.StartsWith("@@") == true)
                {

                    //dialog = new GroupChatDialog(user);
                }
                else
                {
                    //dialog = new UserChatDialog(user);
                }
                CurrentUser.DialogTable.Add(user.UserName, dialog);
                dialog.Show();
            }
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
                        //User = user
                    };
                    this.LB_ContactList.Items.Add(listItem);
                }
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
            this.B_Chat.Background = Theme.HightLightBackgroundBrush;
        }

        private void B_Chat_MouseLeave(object sender, MouseEventArgs e)
        {
            this.B_Chat.Background = Theme.NormalBackgroundBrush;
        }

        private void B_Contact_MouseEnter(object sender, MouseEventArgs e)
        {
            this.B_Contact.Background = Theme.HightLightBackgroundBrush;
        }

        private void B_Contact_MouseLeave(object sender, MouseEventArgs e)
        {
            this.B_Contact.Background = Theme.NormalBackgroundBrush;
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
                string name = item.Value.DisplayName;
                if (name.IndexOf(this.TB_Search.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ContactListItem listItem = new ContactListItem()
                    {
                        DisplayName = item.Value.DisplayName,
                        //User = item.Value
                    };
                    list.Items.Add(listItem);
                    continue;
                }

                if (PinYinConverter.Get(name).ToLower().IndexOf(this.TB_Search.Text.ToLower(), StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ContactListItem listItem = new ContactListItem()
                    {
                        DisplayName = item.Value.DisplayName,
                        //User = item.Value
                    };
                    list.Items.Add(listItem);
                    continue;
                }

                if (PinYinConverter.GetFirst(name).ToLower().IndexOf(this.TB_Search.Text.ToLower(), StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ContactListItem listItem = new ContactListItem()
                    {
                        DisplayName = item.Value.DisplayName,
                        //User = item.Value
                    };
                    list.Items.Add(listItem);
                }

            }
        }
        #endregion

    }
}
