using System;

namespace Virtion.WeChat.Util
{
    public static class Time
    {
        /// <summary>
        /// 将时间转换成UNIX时间戳
        /// </summary>
        /// <param name="dt">时间</param>
        /// <returns>UNIX时间戳</returns>
        public static UInt32 Now()
        {
            TimeSpan ts = DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            UInt32 uiStamp = Convert.ToUInt32(ts.TotalSeconds);
            return uiStamp;
        }
    }
}
