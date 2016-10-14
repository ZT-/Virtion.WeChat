using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Virtion.WeChat
{
    public static class Theme
    {
        public static SolidColorBrush HightLightBackgroundBrush
          = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C3C3C"));

        public static SolidColorBrush NormalBackgroundBrush
           = new SolidColorBrush(Colors.Transparent);



    }
}
