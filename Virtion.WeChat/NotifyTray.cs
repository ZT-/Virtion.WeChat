using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Resources;
using Virtion.WeChat.Util;
using Virtion.WeChat.Windows;

namespace Virtion.WeChat
{
    public class NotifyTray : IDisposable
    {
        private NotifyIcon notifyIcon;
        private MenuWindow menuWindow;

        public NotifyTray()
        {
            this.menuWindow = new MenuWindow();

            this.notifyIcon = new NotifyIcon();
            notifyIcon.Text = "微信助手";

            this.notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        public void InitialTray()
        {
            this.notifyIcon.Icon =
                Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            this.notifyIcon.Visible = true;

            //these operation are  to get ActualWidth and ActualHeight;
            this.menuWindow.SetPosition(-1000, -1000);
            this.menuWindow.Show();
            this.menuWindow.Hide();
        }

        public void Dispose()
        {
            this.notifyIcon.Visible = false;
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                POINT pt = new POINT();
                User32.GetCursorPos(ref pt);
  
                this.menuWindow.SetPosition(pt.x, pt.y);
                this.menuWindow.Show();
            }
            else if (e.Button == MouseButtons.Left)
            {
                App.MainWindow.Show();
            }
        }
    }

}
