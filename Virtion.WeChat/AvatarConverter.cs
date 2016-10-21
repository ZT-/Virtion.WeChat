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
using System.Text;
using System.Security.Cryptography;
using GalaSoft.MvvmLight.Threading;
using Virtion.WeChat.Server;
using Virtion.WeChat.Server.Wx;

namespace Virtion.WeChat
{
    public class AvatarConverter
    {
        private readonly Dictionary<string, ImageSource> avatorTable = new Dictionary<string, ImageSource>();
        private readonly Queue<AvatarRequest> avatarRequestQueue = new Queue<AvatarRequest>();
        private bool isEnd;

        private string avatarTmpFolder
        {
            get
            {
                return App.CurrentPath + "Data\\" + App.WechatClient.CurrentUser.Uin + "\\Avatar\\";
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

        private ImageSource GetImage(User user)
        {
            string jpgPath = avatarTmpFolder + user.PseudoUID + ".jpg";
            System.Drawing.Bitmap bmp = null;
            if (File.Exists(jpgPath) == true)
            {
                bmp = new System.Drawing.Bitmap(jpgPath);
            }
            else
            {
                bmp = App.WechatClient.GetImage(user.HeadImgUrl);
                bmp.Save(jpgPath);
            }

            IntPtr hBitmap = bmp.GetHbitmap();
            ImageSource WpfBitmap =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            this.avatorTable[user.UserName] = WpfBitmap;
            //Console.WriteLine(user.UserName);

            return WpfBitmap;
        }

        public void SetRequest(User user, System.Windows.Controls.Image source)
        {
            if (this.avatorTable.ContainsKey(user.UserName))
            {
                source.Source = this.avatorTable[user.UserName];
                return;
            }

            avatarRequestQueue.Enqueue(new AvatarRequest()
            {
                User = user,
                ImageSource = source
            });
        }

        public void Quit()
        {
            this.isEnd = true;
        }

        public void Start()
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    if (this.isEnd == true)
                    {
                        return;
                    }

                    Thread.Sleep(50);
                    if (avatarRequestQueue.Count > 0)
                    {
                        var request = avatarRequestQueue.Dequeue();

                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            request.ImageSource.Source = this.GetImage(request.User);
                        });
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }


    }
}
