using System;

namespace Virtion.WeChat.Util
{
    public static class StringHelper
    {
        public static bool IsNumber(this String message)
        {
            System.Text.RegularExpressions.Regex rex =
                new System.Text.RegularExpressions.Regex(@"^\d+$");

            if (rex.IsMatch(message))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
