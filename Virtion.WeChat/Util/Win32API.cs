using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Virtion.WeChat.Util
{
    public class User32
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern long GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long GetWindowLongW(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long SetWindowPos(IntPtr hwnd, long hWndInsertAfter, long x, long y, long cx, long cy, long wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        [DllImport("user32.dll", EntryPoint = "PostMessageA", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hwnd, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll", SetLastError = true)] 
        public static extern int GetCursorPos( ref POINT lpPoint );

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr WindowFromPoint(POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr ChildWindowFromPoint(IntPtr parentHwnd,int xPoint, int yPoint);
        
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd,StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCont);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetTopWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ScreenToClient(IntPtr hwnd, ref POINT lpPoint);

        /// <summary>
        /// 获取系统错误信息描述
        /// </summary>
        /// <param name="errCode">系统错误码</param>
        /// <returns></returns>
        public static string GetLastError()
        {
            var errCode = Marshal.GetLastWin32Error();
            IntPtr tempptr = IntPtr.Zero;
            string msg = null;
            FormatMessage(0x1300, ref tempptr, errCode, 0, ref msg, 255, ref tempptr);
            return msg;
        }
        /// <summary>
        /// 获取系统错误信息描述
        /// </summary>
        /// <param name="errCode">系统错误码</param>
        /// <returns></returns>
        public static string GetLastErrorString(int errCode)
        {
            IntPtr tempptr = IntPtr.Zero;
            string msg = null;
            FormatMessage(0x1300, ref tempptr, errCode, 0, ref msg, 255, ref tempptr);
            return msg;
        }

        [DllImport("Kernel32.dll")]
        public extern static int FormatMessage(int flag, ref IntPtr source, int msgid, int langid, ref string buf, int size, ref IntPtr args);

        //public static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong)
        //{
        //    if (IntPtr.Size == 4)
        //    {
        //        return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        //    }
        //    return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        //}

    }
}
