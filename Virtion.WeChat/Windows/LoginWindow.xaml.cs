using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ComponentModel;
using Virtion.WeChat;
using Virtion.Util;

namespace Virtion.WeChat
{
    public partial class LoginWindow : Window
    {
        private int tip;//state 
        private string uuid;
        private string redirectUrl;
        private BackgroundWorker backgroundWorker;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += Login_DoWork;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.ProgressChanged += Login_StateChange;
            backgroundWorker.RunWorkerCompleted += Login_Completed;
            backgroundWorker.RunWorkerAsync();
        }

        private void Login_DoWork(object sender, DoWorkEventArgs e)
        {
            GetQrCode();
            backgroundWorker.ReportProgress(0);

            //等待登录
            while (!backgroundWorker.CancellationPending)
            {
                string url = WXApi.LoginUrl +
                    "?tip=" + tip +
                    "&uuid=" + uuid +
                    "&_=" + Time.Now();

                string ret = HttpRequest.GetSync(url);

                Console.WriteLine("等待登录");
                Console.WriteLine(ret);

                string[] rets = ret.Split(new char[] { '=', ';' });
                string code = rets[1];
                switch (rets[1])
                {
                    case "408"://超时
                        break;
                    case "201"://已扫描
                        tip = 0;
                        //状态报告(1);
                        //状态报告(2);
                        backgroundWorker.ReportProgress(201);
                        break;
                    case "200"://已登录
                        //状态报告(3);
                        backgroundWorker.ReportProgress(201);
                        redirectUrl = ret.Split('"')[1];
                        backgroundWorker.CancelAsync();
                        break;
                    default://400,500
                        GetQrCode();
                        backgroundWorker.ReportProgress(0);
                        break;
                }
            }
        }

        private void Login_StateChange(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 0:
                    login_info.Content = "请使用微信扫描二维码以登录";
                    break;
                case 201:
                    login_info.Content = "成功扫描,请在手机上点击确认以登录";
                    break;
                case 200:
                    login_info.Content = "正在登录...";
                    break;
            }
        }

        private void Login_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (string.IsNullOrEmpty(redirectUrl) == true)
                return;

            App.MainWindow.RedirectUrl = this.redirectUrl;

            this.Close();
        }

        private void GetQrCodeUuid()
        {
            string ret = HttpRequest.GetSync(WXApi.QrCodeUuidUrl);
            this.uuid = ret.Split('"')[1];
            this.tip = 1;
        }

        private void GetQrCode()
        {
            GetQrCodeUuid();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Uri uri = new Uri(WXApi.QrCodeImageUrl + uuid + "?t=webwx&_=" + Time.Now(), UriKind.Absolute);
                this.I_OrCode.Source = new BitmapImage(uri);
            }));
        }
    }
}
