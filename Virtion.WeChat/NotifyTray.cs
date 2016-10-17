using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Resources;
using Virtion.WeChat.Struct;
using Virtion.WeChat.Util;
using Virtion.WeChat.Windows;

namespace Virtion.WeChat
{
    public class NotifyTray : IDisposable
    {
        private NotifyIcon notifyIcon;
        private MenuWindow menuWindow;
        private List<User> groupList;

        public NotifyTray()
        {
            this.menuWindow = new MenuWindow();

            this.notifyIcon = new NotifyIcon();
            notifyIcon.Text = "微信助手";

            this.groupList = new List<User>();
            this.notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        public void RemoveMonitorName(User user)
        {
            this.groupList.Remove(user);
            notifyIcon.Text = this.GetTip();
        }

        public void AddMonitorName(User user)
        {
            this.groupList.Add(user);
            notifyIcon.Text = this.GetTip();
        }

        private string GetTip()
        {
            string tip = "微信助手--正在监控（" + this.groupList.Count + "）个群\n";
            foreach (var item in groupList)
            {
                tip += "【" + item.DisplayName + "】\n";
            }
            return tip;
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

        //static SoundPlayer getPlayer()
        //{
        //    Assembly asm = Assembly.GetEntryAssembly();
        //    Stream SoundStream = asm.GetManifestResourceStream(asm.GetName().Name + ".Resources.msg.wav");
        //    return new SoundPlayer(SoundStream);
        //}

        /*
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        public enum falshType : uint
        {
            FLASHW_STOP = 0,    //停止闪烁
            FALSHW_CAPTION = 1,  //只闪烁标题
            FLASHW_TRAY = 2,   //只闪烁任务栏
            FLASHW_ALL = 3,     //标题和任务栏同时闪烁
            FLASHW_PARAM1 = 4,
            FLASHW_PARAM2 = 12,
            FLASHW_TIMER = FLASHW_TRAY | FLASHW_PARAM1,   //无条件闪烁任务栏直到发送停止标志，停止后高亮
            FLASHW_TIMERNOFG = FLASHW_TRAY | FLASHW_PARAM2  //未激活时闪烁任务栏直到发送停止标志或者窗体被激活，停止后高亮
        }

        public static bool flashTaskBar(IntPtr hWnd, falshType type)
        {
            FLASHWINFO fInfo = new FLASHWINFO();
            fInfo.cbSize = Convert.ToUInt32(System.Runtime.InteropServices.Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;//要闪烁的窗口的句柄，该窗口可以是打开的或最小化的
            fInfo.dwFlags = (uint)type;//闪烁的类型
            fInfo.uCount = 3;//闪烁窗口的次数 UInt32.MaxValue
            fInfo.dwTimeout = 0; //窗口闪烁的频度，毫秒为单位；若该值为0，则为默认图标的闪烁频度
            return FlashWindowEx(ref fInfo);

            //下面的调用：未激活时闪烁任务栏直到发送停止标志或者窗体被激活，停止后高亮
            //flashTaskBar(this.Handle, falshType.FLASHW_TIMERNOFG);
            //下面的调用：停止闪烁，停止后如果未激活窗口，窗口高亮
            //flashTaskBar(this.Handle, falshType.FLASHW_STOP);
        }
        */
        
    
    }

}
