using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Newtonsoft.Json;
using Virtion.WeChat.Struct;
using Wechat.API;
using User = Virtion.WeChat.Server.Wx.User;

namespace Virtion.WeChat.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        public RelayCommand SaveCommand
        {
            get;
            set;
        }

        public Config Config;
        private string configPath
        {
            get { return App.CurrentPath + "//Data//Config.json"; }
        }

        private string userName;
        public string UserName
        {
            get { return this.userName; }
            set
            {
                this.userName = value;
                base.RaisePropertyChanged("UserName");
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get { return this.isLoading; }
            set
            {
                this.isLoading = value;
                base.RaisePropertyChanged("IsLoading");
            }
        }

        private double top;
        public double Top
        {
            get { return top; }
            set
            {
                top = value;
                base.RaisePropertyChanged("Top");
            }
        }

        private double left;
        public double Left
        {
            get { return left; }
            set
            {
                left = value;
                base.RaisePropertyChanged("Left");
            }
        }

        private double width;
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                base.RaisePropertyChanged("Width");
            }
        }

        private double height;
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                base.RaisePropertyChanged("Height");
            }
        }

        public MainViewModel()
        {
            App.WechatClient.OnAddUser += OnAddUser;
            App.WechatClient.OnInitComplate += OnInitComplate;
            App.WechatClient.OnRecvMsg += OnRecvMsg;
            App.WechatClient.OnGetContact += OnGetContact;
            App.WechatClient.OnGetRecent += OnGetRecent;

            this.SaveCommand = new RelayCommand(SaveConfig);
        }

        private void SaveConfig()
        {

        }

        private void OnGetRecent(List<User> list)
        {
            //foreach (var user in list)
            //{
            //    Messenger.Default.Send<User>(user, "AddRecent");
            //}
        }

        private void OnGetContact(List<User> list)
        {
            foreach (var user in list)
            {
                Messenger.Default.Send<User>(user, "AddContactUser");
            }
        }

        private void OnAddUser(User user)
        {
            Messenger.Default.Send<User>(user, "AddRecent");
        }

        private void OnRecvMsg(AddMsg msg)
        {


        }

        private void OnInitComplate()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                this.UserName = App.WechatClient.CurrentUser.DisplayName;
                App.WechatClient.AvatarConverter.SetRequest(App.WechatClient.CurrentUser, App.MainWindow.I_Avator);

                this.IsLoading = false;
            });
        }


        private void Window_Loaded()
        {
            //ShowNotifyIcon();
            this.IsLoading = true;

            if (File.Exists(this.configPath) == true)
            {
                string s = File.ReadAllText(this.configPath);
                try
                {
                    this.Config = JsonConvert.DeserializeObject<Config>(s);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ÅäÖÃÎÄ¼þËð»µ" + ex.ToString());
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

        }


    }
}