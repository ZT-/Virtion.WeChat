using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Virtion.WeChat.Server;
using Virtion.WeChat.Util;

namespace Virtion.WeChat.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private bool isEnabled;
        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                this.isEnabled = value;
                base.RaisePropertyChanged("IsEnabled");
            }
        }

        private string loginInfo;
        public String LoginInfo
        {
            get { return this.loginInfo; }
            set
            {
                this.loginInfo = value;
                base.RaisePropertyChanged("LoginInfo");
            }
        }

        private ImageSource source;
        public ImageSource Source
        {
            get { return this.source; }
            set
            {
                this.source = value;
                base.RaisePropertyChanged("Source");
            }
        }

        public RelayCommand StartCommand
        {
            get;
            private set;
        }

        public LoginViewModel()
        {
            this.isEnabled = true;
            StartCommand = new RelayCommand(Window_Loaded);
        }

        private void Window_Loaded()
        {
            LoginInfo = "正在加载二维码";

            App.WechatClient.OnGetQrCodeImage += this.OnGetQrCodeImage;
            App.WechatClient.OnUserScanQrCode += this.OnUserScanQrCode;
            App.WechatClient.OnLoginSucess += this.OnLoginSucess;

            (App.Current as App).StartClient();
        }

        private void OnGetQrCodeImage(string url)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                this.LoginInfo = "正在生成二维码 ....";
                var image = new BitmapImage(new Uri(url));
                this.Source = image;
                this.LoginInfo = "正在等待扫码 ....";
            });
        }

        private void OnUserScanQrCode(string data)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                this.LoginInfo = "正在等待确认 ....";
                if (string.IsNullOrEmpty(data) == false)
                {
                    byte[] base64_image_bytes = Convert.FromBase64String(data);
                    MemoryStream memoryStream = new MemoryStream(base64_image_bytes, 0, base64_image_bytes.Length);
            
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = memoryStream;
                    image.EndInit();

                    this.Source = image;
                }
            });
        }


        private void OnLoginSucess()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                this.IsEnabled = false;
                (App.Current as App).ShowMainWindow();
            });

        }

    }
}
