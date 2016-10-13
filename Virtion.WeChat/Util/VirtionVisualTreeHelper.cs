using System;
using System.Windows;
using System.Windows.Media;

namespace Virtion.WeChat.Util
{
    public static class VirtionVisualTreeHelper
    {

        public static T FindByPath<T>(DependencyObject reference, int[] path)
        {
            DependencyObject obj = null;
            try
            {
                obj = VisualTreeHelper.GetChild(reference, path[0]);
                for (int i = 1; i < path.Length; i++)
                {
                    obj = VisualTreeHelper.GetChild(obj, path[i]);
                }
            }
            catch (Exception)
            {
                throw;
            }
            var result = (T)Convert.ChangeType(obj, typeof(T));
            return result;
        }


    }
}

