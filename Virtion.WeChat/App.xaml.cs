using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Virtion.WeChat.Server;
using Virtion.WeChat.ViewModel;
using Virtion.WeChat.Windows;

namespace Virtion.WeChat
{
    public partial class App : Application
    {
        public static WechatClient WechatClient;
        public const string Version = " - V 1.0";
        private static string path = "";
        public new static MainWindow MainWindow;


        public static string CurrentPath
        {
            get
            {
                if (string.IsNullOrEmpty(path) != true)
                {
                    return path;
                }
                else
                {
                    return System.AppDomain.CurrentDomain.BaseDirectory;
                }
            }
        }

        public App()
        {
            DispatcherHelper.Initialize();
            WechatClient = new WechatClient();
        }

        public void StartClient()
        {
            var thread = new Thread(() =>
            {
                WechatClient.Run();
            });
            thread.Start();
        }

        public void ShowMainWindow()
        {
            MainWindow = new MainWindow();
            MainWindow.Show();

        }

    }
}
