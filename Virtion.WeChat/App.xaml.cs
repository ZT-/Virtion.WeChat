using System.Windows;

namespace Virtion.WeChat
{
    public partial class App : Application
    {
        public const string Version = " - V 1.0";
        private static string path = "";
        public new static MainWindow MainWindow
        {
            get
            {
                return App.Current.MainWindow as MainWindow;
            }
        }
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

    }
}
