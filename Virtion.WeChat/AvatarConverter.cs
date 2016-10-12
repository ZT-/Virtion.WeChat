using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Threading;
using System.Windows.Controls;
using Virtion.WeChat.Struct;
using System.Text;
using System.Security.Cryptography;
using Virtion.Util;

namespace Virtion.WeChat
{
    public class AvatarConverter : UserControl
    {
        public Dictionary<string, ImageSource> AvatorTable = new Dictionary<string, ImageSource>();

        private string avatarTmpFolder
        {
            get
            {
                return App.CurrentPath + "Data\\" + CurrentUser.WxUin + "\\Avatar\\";
            }
        }

        private class AvatarRequest
        {
            public User User;
            public System.Windows.Controls.Image ImageSource;
        };

        public AvatarConverter()
        {
            if (Directory.Exists(this.avatarTmpFolder) == false)
            {
                Directory.CreateDirectory(this.avatarTmpFolder);
            }
        }

        private static Queue<AvatarRequest> avatarRequestQueue = new Queue<AvatarRequest>();



        private ImageSource GetHead(User user)
        {
            string jpgPath = avatarTmpFolder + user.PseudoUID + ".jpg";

            System.Drawing.Bitmap bmp = null;

            if (File.Exists(jpgPath) == true)
            {
                bmp = new System.Drawing.Bitmap(jpgPath);
            }
            else
            {
                string url = "http://wx.qq.com" + user.HeadImgUrl;
                WebRequest request = WebRequest.Create(url);
                request.Headers.Add(HttpRequestHeader.Cookie, CurrentUser.Cookie);
                WebResponse response = request.GetResponse();
                //Console.WriteLine("获取头像,长度:" + response.ContentLength);
                if (response.ContentLength == 0)
                {
                    response.Close();
                    return null;
                }
                System.Drawing.Image img = null;
                try
                {
                    Stream dataStream = response.GetResponseStream();
                    img = System.Drawing.Image.FromStream(dataStream);
                    dataStream.Close();
                    response.Close();
                    bmp = new System.Drawing.Bitmap(img);
                }
                catch (Exception)
                {
                    bmp = new System.Drawing.Bitmap(10, 10);
                }
                bmp.Save(jpgPath);
            }

            IntPtr hBitmap = bmp.GetHbitmap();
            ImageSource WpfBitmap =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            this.AvatorTable[user.UserName] = WpfBitmap;
            //Console.WriteLine(user.UserName);

            return WpfBitmap;
        }

        public void SetRequest(User user, System.Windows.Controls.Image source)
        {
            if (this.AvatorTable.ContainsKey(user.UserName))
            {
                source.Source = this.AvatorTable[user.UserName];
                return;
            }

            avatarRequestQueue.Enqueue(new AvatarRequest()
            {
                User = user,
                ImageSource = source
            });
        }

        public void Start()
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(50);
                    if (avatarRequestQueue.Count > 0)
                    {
                        var request = avatarRequestQueue.Dequeue();
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            request.ImageSource.Source = this.GetHead(request.User);
                        }));
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

    }
}
