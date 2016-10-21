using System.Collections.Generic;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Virtion.WeChat.Server.Wx;
using Wechat.API;

namespace Virtion.WeChat.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
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

        public MainViewModel()
        {
            App.WechatClient.OnAddUser += OnAddUser;
            App.WechatClient.OnInitComplate += OnInitComplate;
            App.WechatClient.OnRecvMsg += OnRecvMsg;
            App.WechatClient.OnGetContact += OnGetContact;
            App.WechatClient.OnGetRecent += OnGetRecent;
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
                App.WechatClient.AvatarConverter.SetRequest(App.WechatClient.CurrentUser,App.MainWindow.I_Avator);

                this.IsLoading = false;
            });
        }


    }
}