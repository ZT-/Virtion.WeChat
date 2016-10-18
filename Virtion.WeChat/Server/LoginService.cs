using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Virtion.WeChat.Util;


namespace Virtion.WeChat.Server
{
    public class LoginService
    {
        private static string _uuid;

        public static BitmapImage GetQrCode()
        {
            string ret = HttpRequest.GetSync(WxApi.QrCodeUuidUrl);
            _uuid = ret.Split('"')[1];
            Uri uri = new Uri(WxApi.QrCodeImageUrl + _uuid + "?t=webwx&_=" + Time.Now(), UriKind.Absolute);
            return new BitmapImage(uri);
        }





    }
}
